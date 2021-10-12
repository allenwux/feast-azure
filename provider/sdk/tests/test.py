import os
from feast import FeatureStore, RepoConfig
from feast.registry import RegistryConfig
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig, MsSqlServerSource

from feast.infra.online_stores.redis import RedisOnlineStoreConfig

from feast import Entity, Feature, FeatureView, ValueType, FeatureService
from datetime import timedelta
from datetime import timedelta, datetime
import pandas as pd
import base64

registry_blob_url = "https://xiwutestai.blob.core.windows.net/public/Registry.db"

reg_config = RegistryConfig(
    registry_store_type="feast_azure_provider.registry_store.AzBlobRegistryStore",
    path=registry_blob_url,
)

# update this to your location

# update with your connection strings
sql_conn_string = "mssql+pyodbc://cloudsa:Yukon900Yukon900@feastplus.database.windows.net:1433/feastcore?driver=ODBC+Driver+17+for+SQL+Server&autocommit=True"

redis_conn_string = "xiwufeast-redis.redis.cache.windows.net:6380,password=UsVSKAhg9BPqJ6Qr4EEQXJGZDmAMwUX+eKw0MW0MK+U=,ssl=True,abortConnect=False"

repo_cfg = RepoConfig(
    registry=reg_config,
    project="production",
    provider="feast_azure_provider.azure_provider.AzureProvider",
    offline_store=MsSqlServerOfflineStoreConfig(
        connection_string=sql_conn_string
        ),
    online_store=RedisOnlineStoreConfig(
        connection_string=redis_conn_string
    ),
)

store = FeatureStore(config=repo_cfg)

entity = Entity(name='id', value_type=ValueType.INT32, join_key='id')

entity_proto = entity.to_proto().SerializeToString()
entity_proto_str = base64.b64encode(entity_proto).decode('utf-8')
print(entity_proto_str)

sql_data_source = MsSqlServerSource(
    table_ref="sample.trips",
    event_timestamp_column="start_time"
)

driver_stats_fv = FeatureView(
    name="driver_activity",
    entities=["drivers"],
    features=[
        Feature(name="length_in_miles", dtype=ValueType.INT32),
    ],
    batch_source=sql_data_source,
    ttl=timedelta(seconds=60)
)

driver_info_fv = FeatureView(
    name="driver_info",
    entities=["drivers"],
    features=[
        Feature(name="name", dtype=ValueType.STRING),
        Feature(name="age", dtype=ValueType.INT32),
    ],
    batch_source=sql_data_source,
    ttl=timedelta(seconds=60)
)


feature_view_proto = driver_stats_fv.to_proto().SerializeToString()
feature_view_proto_string = base64.b64encode(feature_view_proto).decode('utf-8')
print(feature_view_proto_string)

feature_view_proto = driver_info_fv.to_proto().SerializeToString()
feature_view_proto_string = base64.b64encode(feature_view_proto).decode('utf-8')
print(feature_view_proto_string)

driver_stats_fs = FeatureService(
    name="driver_service",
    features=[driver_stats_fv, driver_info_fv[["name"]]]
)

feature_service_proto = driver_stats_fs.to_proto().SerializeToString()
feature_service_proto_string = base64.b64encode(feature_service_proto).decode('utf-8')
print(feature_service_proto_string)

store.apply([entity, driver_stats_fv, driver_info_fv, driver_stats_fs])


print(f"Get historical feature.")
entity_df = pd.DataFrame.from_dict(
    {
        "drivers": [1,2],
        "event_timestamp": [
            datetime(2021, 5, 25, 9),
            datetime(2021, 5, 25, 10, 20),
        ],
    }
)

training_df = store.get_historical_features(
    entity_df=entity_df,
    features=[
        "driver_activity:length_in_miles",
    ],
).to_df()

print(training_df.head())

