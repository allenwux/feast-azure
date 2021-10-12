import os
import warnings
from collections import Counter, OrderedDict, defaultdict
import requests
from datetime import datetime, timedelta
from pathlib import Path
import json
from typing import Any, Dict, Iterable, List, Optional, Set, Tuple, Union, cast
import base64
import logging

from feast.repo_config import RepoConfig, RegistryConfig
from feast.registry import Registry
from feast.feature_store import FeatureStore
from feast.protos.feast.core.Registry_pb2 import Registry as RegistryProto
from feast.infra.provider import get_provider

from feast.errors import EntityNotFoundException

from feast.entity import Entity
from feast.feature_service import FeatureService
from feast.feature_view import (
    DUMMY_ENTITY_ID,
    DUMMY_ENTITY_NAME,
    DUMMY_ENTITY_VAL,
    FeatureView,
)
from feast.protos.feast.core.Entity_pb2 import Entity as EntityV2Proto
from feast.protos.feast.core.FeatureView_pb2 import FeatureView as FeatureViewProto
from feast.protos.feast.core.FeatureService_pb2 import FeatureService as FeatureServiceProto

from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig
from feast.infra.online_stores.redis import RedisOnlineStoreConfig
from feast.value_type import ValueType
from azure.identity import DeviceCodeCredential, EnvironmentCredential, ChainedTokenCredential, AzureCliCredential
from azure.core.exceptions import ClientAuthenticationError
import msal


SQL_OFFLINE_STORE_TYPE = "feast_azure_provider.mssqlserver.MsSqlServerOfflineStore"
RESID_ONLINE_STORE_TYPE = "redis";

class FeastCoreResponseError(Exception):

    status_code: int
    error_message: str

    def __init__(self, status_code: int, error_message: str):
        self.status_code = status_code
        self.error_message = error_message
        super().__init__(
            f"{error_message}"
        )

class FeatureStoreClient():
    
    def __init__(
        self, name: str, 
        project_name: str, 
        local_cache_file_path: str, 
        refresh_local_cache: bool = True, 
        aad_auth: bool = False,
        aad_tenant_id: str = None,
        aad_client_id: str = None,
    ):
        logger = logging.getLogger()

        logger.setLevel(logging.WARNING)
        self.repo_path = Path(os.getcwd())

        self._uri = self._get_service_uri(name)
        self._local_cache_file_path = local_cache_file_path
        self._project_name = project_name
        self._aad_client_id = aad_client_id
        self._aad_tenant_id = aad_tenant_id
        self._aad_auth = aad_auth
        self._default_credential = None
        self._refresh_local_cache = refresh_local_cache
        self._aad_token = None
        self._aad_token_expire_on = datetime.utcnow()

        if self._aad_auth and self._aad_client_id == None:
            if "FEAST_CLIENT_ID" in os.environ:
               self._aad_client_id = os.environ["FEAST_CLIENT_ID"]
            else:
                logger.error("Feast service client ID is required for AAD authentication. You can provide it as input parameter or environment variable with name FEAST_CLIENT_ID.")

        if self._aad_auth and self._aad_tenant_id == None:
            if "AZURE_TENANT_ID" in os.environ:
                self._aad_tenant_id = os.environ["AZURE_TENANT_ID"]
            else:
                logger.error("AAD tenant ID is required for AAD authentication. You can provide it as input parameter or environment variable with name AZURE_TENANT_ID.")


        self.init()
        
    def init(
        self
    ):
        logger = logging.getLogger()
        try:
            project = self.get_project(self._project_name)
        except FeastCoreResponseError as err:
            if (err.status_code == 404):
                logger.warning(f"The project {self._project_name} does not exist. Call create_project() to create the project and then call init() to reinitialize the client.")
                return
            
            # re-throw the error if failed with different status code
            raise err
        
        self.config = RepoConfig(**project)

        self.config.registry = RegistryConfig(path = self._local_cache_file_path, registry_store_type = "LocalRegistryStore")
        
        self.local = FeatureStore(config= self.config)
        self.feature_store = self.local

        if self._refresh_local_cache:
            self._load_objects()

    def _load_objects(
        self
    ) -> Union[
            Entity,
            FeatureView,
            FeatureService]:

        objects = []
        objects = objects + self.list_entities()
        objects = objects + self.list_feature_views()
        objects = objects + self.list_feature_services()

        return objects
    
    def _get_request_header_internal(
        self
    ) -> dict:
        headers = {}
        if self._aad_auth:
            headers["authorization"] = f"Bearer {self._get_aad_token_internal()}"

        return headers

    def _get_aad_token_internal(
        self
    ) -> str:
        if self._aad_token != None and self._aad_token_expire_on > datetime.utcnow():
            return self._aad_token


        if "AZURE_CLIENT_ID" in os.environ and "AZURE_CLIENT_SECRET" in os.environ:
            
            spn_client_id = os.environ["AZURE_CLIENT_ID"]
            spn_client_secret = os.environ["AZURE_CLIENT_SECRET"]
            # Get token from service principal
            authority = f"https://login.microsoftonline.com/{self._aad_tenant_id}"
            
            app = msal.ConfidentialClientApplication(
                spn_client_id, client_credential=spn_client_secret, authority=authority
            )

            scope = f"api://{self._aad_client_id}/.default"

            result = app.acquire_token_for_client(scope)
            self._aad_token = result["access_token"]
            self._aad_token_expire_on = datetime.utcnow() + timedelta(seconds=result["expires_in"])
            return self._aad_token
        
        else:
        
            scope = f"api://{self._aad_client_id}/Feast.All"
            if self._default_credential == None:
                self._default_credential = DeviceCodeCredential(
                    tenant_id=self._aad_tenant_id,
                    client_id=self._aad_client_id
                )

            token = self._default_credential.get_token(scope)
            self._aad_token = token.token
            self._aad_token_expire_on = datetime.utcfromtimestamp(token.expires_on)
            return self._aad_token

    def _get_service_uri(
        self,
        name: str
    ) -> str:
        if not name.startswith('https:'):
            name = f"https://{name}.azurewebsites.net"
        return name

    def _handle_error_response(
        self,
        response
    ):
        if response.status_code != 200 and response.status_code != 204:
            response_obj = response.json()
            error_message = "Unknown error."
            if "Message" in response_obj:
                error_message = response_obj["ErrorMessage"]
            
            raise FeastCoreResponseError(
                response.status_code, error_message
            )

    def _data_store_config_to_json(
        self,
        config: Union[MsSqlServerOfflineStoreConfig, RedisOnlineStoreConfig]
    ) -> dict:
        if isinstance(config, MsSqlServerOfflineStoreConfig):
            return {
                'type': SQL_OFFLINE_STORE_TYPE, 
                'connection_string': config.connection_string
                }
        elif isinstance(config, RedisOnlineStoreConfig):
            return {
                'type': RESID_ONLINE_STORE_TYPE, 
                'redis_type': config.redis_type,
                'connection_string': config.connection_string
                }
        return {}

    def _repo_config_to_json(
        self,
        config: RepoConfig, description: str, isDefault: bool
    ) -> dict:
        # have to compose the request body manually since RepoConfig is not json serializable
        content = {}
        content["projectName"]=config.project
        content["description"]=description
        content["provider"]=config.provider
        if config.offline_store != None:
            content["offline_store"]=self._data_store_config_to_json(config.offline_store)
        if config.online_store != None:
            content["online_store"]=self._data_store_config_to_json(config.online_store)
        content["flags"]=config.flags
        content["isDefault"]=isDefault
        return content

    def create_project(
        self, 
        config: RepoConfig, 
        description: str, 
        isDefault: bool = False
    ):
        content = self._repo_config_to_json(config, description, isDefault)
        response = requests.put(f"{self._uri}/api/projects/{config.project}", 
            headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

    def update_project(
        self, 
        config: RepoConfig, 
        description: str, 
        isDefault: bool = False
    ):
        content = self._repo_config_to_json(config, description, isDefault)
        response = requests.patch(f"{self._uri}/api/projects/{config.project}", 
            headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)
        
    def delete_project(
        self,
        project_name: str
    ):
        response = requests.delete(f"{self._uri}/api/projects/{project_name}", 
            headers=self._get_request_header_internal())
        self._handle_error_response(response)
    
    def get_project(
        self,
        project_name: str
    ) -> dict:
        response = requests.get(f"{self._uri}/api/projects/{project_name}", 
            headers=self._get_request_header_internal())
        self._handle_error_response(response)
        project = response.json()
        return project
    
    def list_projects(
        self,
    ) -> dict:
        response = requests.get(f"{self._uri}/api/projects", 
            headers=self._get_request_header_internal())
        self._handle_error_response(response)
        projects = response.json()
        return projects

    def apply_entity(
        self, entity: Entity, refresh_local_cache = True
    ):
        entity.is_valid()
        proto_bytes = entity.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.post(f"{self._uri}/api/projects/{self._project_name}/entities/{entity.name}/apply", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_entity(entity, project=self._project_name)
           
    def create_entity(
        self, entity: Entity, refresh_local_cache = True
    ):
        # make sure nobody mess up with the DUMMY ENTITY.
        if entity.name == DUMMY_ENTITY_NAME:
            entity.join_key = DUMMY_ENTITY_ID
            entity.value_type = ValueType.INT32
        
        entity.is_valid()
        proto_bytes = entity.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.put(f"{self._uri}/api/projects/{self._project_name}/entities/{entity.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_entity(entity, project=self._project_name)
            
    def update_entity(
        self, entity: Entity, refresh_local_cache = True
    ):
        # make sure nobody mess up with the DUMMY ENTITY.
        if entity.name == DUMMY_ENTITY_NAME:
            entity.join_key = DUMMY_ENTITY_ID
            entity.value_type = ValueType.INT32
        
        entity.is_valid()
        proto_bytes = entity.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.patch(f"{self._uri}/api/projects/{self._project_name}/entities/{entity.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_entity(entity, project=self._project_name)
    
    def _delete_entity_local(
        self, entity_name: str
    ):
        self.local._registry._prepare_registry_for_changes()
        assert self.local._registry.cached_registry_proto

        for idx, feature_service_proto in enumerate(
            self.local._registry.cached_registry_proto.entities
        ):
            if (
                feature_service_proto.spec.name == entity_name
                and feature_service_proto.spec.project == self._project_name
            ):
                del self.local._registry.cached_registry_proto.entities[idx]
                self.local._registry.commit()
                return
        raise EntityNotFoundException(entity_name, self._project_name)

    def delete_entity(
        self, entity_name: str, refresh_local_cache = True
    ):
        # make sure nobody mess up with the DUMMY ENTITY.
        if entity_name == DUMMY_ENTITY_NAME:
            raise ValueError(f"Can not delete entity {DUMMY_ENTITY_NAME}")
        
        response = requests.delete(f"{self._uri}/api/projects/{self._project_name}/entities/{entity_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        if refresh_local_cache:
            self._delete_entity_local(entity_name)

    def get_entity(
        self, entity_name: str, refresh_local_cache = True
    ) -> Entity:
        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/entities/{entity_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        entityResponse = response.json()
        entity_proto = EntityV2Proto()
        entity_proto.ParseFromString(base64.b64decode(entityResponse["proto"].encode('ascii')))
        entity = Entity.from_proto(entity_proto)
        if refresh_local_cache:
            self.local._registry.apply_entity(entity, project=self._project_name)

        return entity
    
    def list_entities(
        self, refresh_local_cache = True
    ) -> List[Entity]:

        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/entities", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        entitie_response = response.json()
        entities = []
        for entity in entitie_response:
            entity_proto = EntityV2Proto()
            entity_proto.ParseFromString(base64.b64decode(entity["proto"].encode('ascii')))
            entities.append(Entity.from_proto(entity_proto))
        
        if refresh_local_cache:
            self.local.apply(entities)
        return entities

    def apply_feature_view(
        self, feature_view: FeatureView, refresh_local_cache = True
    ):
        feature_view.is_valid()
        proto_bytes = feature_view.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.post(f"{self._uri}/api/projects/{self._project_name}/featureviews/{feature_view.name}/apply", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_view(feature_view, project=self._project_name)
        
    def create_feature_view(
        self, feature_view: FeatureView, refresh_local_cache = True
    ):
        feature_view.is_valid()
        proto_bytes = feature_view.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.put(f"{self._uri}/api/projects/{self._project_name}/featureviews/{feature_view.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_view(feature_view, project=self._project_name)
            
    def update_feature_view(
        self, feature_view: FeatureView, refresh_local_cache = True
    ):
        feature_view.is_valid()
        proto_bytes = feature_view.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.patch(f"{self._uri}/api/projects/{self._project_name}/featureviews/{feature_view.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_view(feature_view, project=self._project_name)
    
    def delete_feature_view(
        self, feature_view_name: str, refresh_local_cache = True
    ):
        response = requests.delete(f"{self._uri}/api/projects/{self._project_name}/featureviews/{feature_view_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        if refresh_local_cache:
            self.local._registry.delete_feature_view(feature_view_name, project=self._project_name)

    def get_feature_view(
        self, feature_view_name: str, refresh_local_cache = True
    ) -> FeatureView:
        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/featureviews/{feature_view_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        feature_viewResponse = response.json()
        feature_view_proto = FeatureViewProto()
        feature_view_proto.ParseFromString(base64.b64decode(feature_viewResponse["proto"].encode('ascii')))
        feature_view = FeatureView.from_proto(feature_view_proto)
        if refresh_local_cache:
            self.local._registry.apply_feature_view(feature_view, project=self._project_name)

        return feature_view
    
    def list_feature_views(
        self, refresh_local_cache = True
    ) -> List[FeatureView]:

        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/featureviews", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        entitie_response = response.json()
        featureviews = []
        for feature_view in entitie_response:
            feature_view_proto = FeatureViewProto()
            feature_view_proto.ParseFromString(base64.b64decode(feature_view["proto"].encode('ascii')))
            featureviews.append(FeatureView.from_proto(feature_view_proto))
        
        if refresh_local_cache:
            self.local.apply(featureviews)
        return featureviews

    def apply_feature_service(
        self, feature_service: FeatureService, refresh_local_cache = True
    ):
        proto_bytes = feature_service.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.post(f"{self._uri}/api/projects/{self._project_name}/featureservices/{feature_service.name}/apply", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_service(feature_service, project=self._project_name)
           

    def create_feature_service(
        self, feature_service: FeatureService, refresh_local_cache = True
    ):
        proto_bytes = feature_service.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.put(f"{self._uri}/api/projects/{self._project_name}/featureservices/{feature_service.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_service(feature_service, project=self._project_name)
            
    def update_feature_service(
        self, feature_service: FeatureService, refresh_local_cache = True
    ):
        proto_bytes = feature_service.to_proto().SerializeToString()
        proto_base64_str = base64.b64encode(proto_bytes).decode('utf-8')
        content = {'proto': proto_base64_str}
        response = requests.patch(f"{self._uri}/api/projects/{self._project_name}/featureservices/{feature_service.name}", headers=self._get_request_header_internal(), json=content)
        self._handle_error_response(response)

        if refresh_local_cache:
            self.local._registry.apply_feature_service(feature_service, project=self._project_name)
    
    def delete_feature_service(
        self, feature_service_name: str, refresh_local_cache = True
    ):
        response = requests.delete(f"{self._uri}/api/projects/{self._project_name}/featureservices/{feature_service_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        if refresh_local_cache:
            self.local._registry.delete_feature_service(feature_service_name, project=self._project_name)

    def get_feature_service(
        self, feature_service_name: str, refresh_local_cache = True
    ) -> FeatureService:
        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/featureservices/{feature_service_name}", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        
        feature_serviceResponse = response.json()
        feature_service_proto = FeatureServiceProto()
        feature_service_proto.ParseFromString(base64.b64decode(feature_serviceResponse["proto"].encode('ascii')))
        feature_service = FeatureService.from_proto(feature_service_proto)
        if refresh_local_cache:
            self.local._registry.apply_feature_service(feature_service, project=self._project_name)

        return feature_service
    
    def list_feature_services(
        self, refresh_local_cache = True
    ) -> List[FeatureService]:

        response = requests.get(f"{self._uri}/api/projects/{self._project_name}/featureservices", headers=self._get_request_header_internal())
        self._handle_error_response(response)
        entitie_response = response.json()
        featureservices = []
        for feature_service in entitie_response:
            feature_service_proto = FeatureServiceProto()
            feature_service_proto.ParseFromString(base64.b64decode(feature_service["proto"].encode('ascii')))
            featureservices.append(FeatureService.from_proto(feature_service_proto))
        
        if refresh_local_cache:
            self.local.apply(featureservices)
        return featureservices

    def apply_all(
        self, objects: List[Union[FeatureView, Entity, FeatureService]], refresh_local_cache: bool = True
    ):
        # DUMMY_ENTITY is a placeholder entity used in entityless FeatureViews
        DUMMY_ENTITY = Entity(
            name=DUMMY_ENTITY_NAME,
            join_key=DUMMY_ENTITY_ID,
            value_type=ValueType.INT32,
        )
        self.apply_entity(DUMMY_ENTITY, refresh_local_cache)

        for obj in objects:
            if isinstance(obj, Entity):
                self.apply_entity(obj, refresh_local_cache)
            elif isinstance(obj, FeatureView):
                self.apply_feature_view(obj, refresh_local_cache)
            elif isinstance(obj, FeatureService):
                self.apply_feature_service(obj, refresh_local_cache)
            else:
                raise ValueError("Unknown object type provided.")