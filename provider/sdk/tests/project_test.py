from datetime import timedelta, datetime
import pandas as pd

from feast import Entity, Feature, FeatureView, ValueType, FeatureService
from feast_azure_provider.feature_store_client import FeatureStoreClient
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig, MsSqlServerSource
from feast.infra.online_stores.redis import RedisOnlineStoreConfig
from feast import FeatureStore, RepoConfig


service_name = 'feastplusplus'

print("create the client")
client = FeatureStoreClient(name = service_name, 
    project_name='production', 
    local_cache_file_path='registry.db', 
    aad_auth = True,
    aad_tenant_id = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    aad_client_id="6f8f795b-2c08-4e93-b142-15642cc011e3")

try:
    
    sql_conn_string = "mssql+pyodbc://cloudsa:Yukon900Yukon900@feastplus.database.windows.net:1433/feastcore?driver=ODBC+Driver+17+for+SQL+Server&autocommit=True"

    redis_conn_string = "xiwufeast-redis.redis.cache.windows.net:6380,password=UsVSKAhg9BPqJ6Qr4EEQXJGZDmAMwUX+eKw0MW0MK+U=,ssl=True,abortConnect=False"

    repo_cfg = RepoConfig(
        registry="azRegistry",
        project="production",
        provider="feast_azure_provider.azure_provider.AzureProvider",
        offline_store=MsSqlServerOfflineStoreConfig(
            connection_string=sql_conn_string
            ),
        online_store=RedisOnlineStoreConfig(
            connection_string=redis_conn_string
        ),
    )

    print("create project")
    client.create_project(repo_cfg, "this is just a test project")

    print("update project")
    client.update_project(repo_cfg, "this is just a test project but updated")

    print("get project")
    client.get_project("production")

    print("list projects")
    projects = client.list_projects()
    print(len(projects))

except BaseException as e:
    print('test failed')
    print(str(e))
finally:
    print("delete project")
    client.delete_project("production")