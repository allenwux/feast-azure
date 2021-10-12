from datetime import timedelta, datetime
import pandas as pd
import os

from feast import Entity, Feature, FeatureView, ValueType, FeatureService
from feast_azure_provider.feature_store_client import FeatureStoreClient
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig, MsSqlServerSource
from feast.infra.online_stores.redis import RedisOnlineStoreConfig
from feast import RepoConfig

# feature store service name. It can be either the Azure function name or url
service_name = 'feastplusplus'

os.environ["AZURE_TENANT_ID"] = "72f988bf-86f1-41af-91ab-2d7cd011db47"
os.environ["FEAST_CLIENT_ID"] = "6f8f795b-2c08-4e93-b142-15642cc011e3"
os.environ["AZURE_CLIENT_ID"] = "f0d50d2d-7ff0-40e3-82cc-8c44b176c088"
os.environ["AZURE_CLIENT_SECRET"] = "Mau7Q~XnK7AO3exQ516.0Oz5LbCmXgsNPvODc"

print("Step 1: Create the client")
client = FeatureStoreClient(name = service_name, 
    project_name='production', 
    local_cache_file_path='registry.db', 
    aad_auth = True,
    #aad_tenant_id = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    #aad_client_id="6f8f795b-2c08-4e93-b142-15642cc011e3"
)

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
            redis_type = "redis",
            connection_string=redis_conn_string
        ),
    )

    print("Step 2: Create and initialize a new project")
    client.create_project(repo_cfg, "this is just a test project")
    client.init()

    sql_data_source = MsSqlServerSource(
        table_ref="sample.trips",
        event_timestamp_column="start_time"
    )

    print("Step 3: Create entity, feature view and feature service")
    entity = Entity(name='driver_id', value_type=ValueType.INT32, join_key='driver_id')

    driver_stats_fv = FeatureView(
        name="driver_activity_test",
        entities=["driver_id"],
        features=[
            Feature(name="length_in_miles", dtype=ValueType.INT32),
        ],
        batch_source=sql_data_source,
        ttl=timedelta(seconds=60))

    client.apply_all([entity, driver_stats_fv])
    
    driver_stats_fs = FeatureService(
        name="driver_service_test",
        features=[driver_stats_fv[["length_in_miles"]]]
    )

    client.create_feature_service(driver_stats_fs)

    print("Step 4: Get historical features using feature views and feature services")

    feature_store = client.feature_store

    print("Get historical features with feature view")    
    training_df = feature_store.get_historical_features(
        entity_df='select driver_id, start_time from sample.trips where driver_id = 1',
        features=[
            "driver_activity_test:length_in_miles",
        ],
    ).to_df()

    print(training_df.head())

    
    print("Get online features with feature service")    
    feature_store.get_historical_features(
        features=driver_stats_fs, entity_df='select driver_id, start_time from sample.trips where driver_id = 1'
    ).to_df()

    print(training_df.head())

except BaseException as e:
    print('test failed')
    print(str(e))
finally:
    print("Step 5: Clean up")
    client.delete_entity("driver_id")
    client.delete_feature_view("driver_activity_test")
    client.delete_feature_service("driver_service_test")
    client.delete_project("production")