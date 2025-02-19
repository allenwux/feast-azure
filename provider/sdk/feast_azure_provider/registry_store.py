# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

import os
import uuid

from feast.protos.feast.core.Registry_pb2 import Registry as RegistryProto
from feast.registry import RegistryConfig
from feast.registry_store import RegistryStore
from pathlib import Path
from datetime import datetime
from urllib.parse import urlparse
from tempfile import TemporaryFile

REGISTRY_SCHEMA_VERSION = "1"


class AzBlobRegistryStore(RegistryStore):
    def __init__(self, registry_config: RegistryConfig, repo_path: Path):
        try:
            from azure.storage.blob import BlobClient
            from azure.identity import DefaultAzureCredential
            from azure.storage.blob import BlobServiceClient
            import logging
        except ImportError as e:
            from feast.errors import FeastExtrasDependencyImportError

            raise FeastExtrasDependencyImportError("az", str(e))

        self._uri = urlparse(registry_config.path)
        self._account_url = self._uri.scheme + "://" + self._uri.netloc
        container_path = self._uri.path.lstrip("/").split("/")
        self._container = container_path.pop(0)
        self._path = "/".join(container_path)

        try:
            # turn the verbosity of the blob client to warning and above (this reduces verbosity)
            logger = logging.getLogger("azure")
            logger.setLevel(logging.ERROR)
            default_credential = DefaultAzureCredential(
                exclude_shared_token_cache_credential=True
            )

            client = BlobServiceClient(
                account_url=self._account_url, credential=default_credential
            )
            self.blob = client.get_blob_client(
                container=self._container, blob=self._path
            )
        except:
            print(
                "Could not connect to blob. Check the following\nIs the URL specified correctly?\nIs you IAM role set to Storage Blob Data Contributor?\n"
            )

        return

    def get_registry_proto(self):
        file_obj = TemporaryFile()
        registry_proto = RegistryProto()

        if self.blob.exists():
            download_stream = self.blob.download_blob()
            file_obj.write(download_stream.readall())

            file_obj.seek(0)
            registry_proto.ParseFromString(file_obj.read())
            return registry_proto
        raise FileNotFoundError(
            f'Registry not found at path "{self._uri.geturl()}". Have you run "feast apply"?'
        )

    def update_registry_proto(self, registry_proto: RegistryProto):
        self._write_registry(registry_proto)

    def teardown(self):
        self.blob.delete_blob()

    def _write_registry(self, registry_proto: RegistryProto):
        registry_proto.version_id = str(uuid.uuid4())
        registry_proto.last_updated.FromDatetime(datetime.utcnow())

        file_obj = TemporaryFile()
        file_obj.write(registry_proto.SerializeToString())
        file_obj.seek(0)
        self.blob.upload_blob(file_obj, overwrite=True)
        return
