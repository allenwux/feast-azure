from feast import Entity, Feature, FeatureView, ValueType, FeatureService
from datetime import timedelta, datetime
import pandas as pd

from feast_azure_provider.feature_store_client import FeatureStoreClient
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig, MsSqlServerSource

client = FeatureStoreClient(name = 'feastplus', project_name='myProject', local_cache_file_path='registry.db')

entity_df = pd.DataFrame.from_dict(
    {
        "drivers": [1,2],
        "event_timestamp": [
            datetime(2021, 5, 25, 9),
            datetime(2021, 5, 25, 9, 32),
        ],
    }
)

sql_data_source = MsSqlServerSource(
    table_ref="sample.trips",
    event_timestamp_column="start_time"
)

entity = Entity(name='driver_id', value_type=ValueType.INT32, join_key='driver_id')


driver_stats_fv = FeatureView(
    name="driver_activity_test",
    entities=["driver_id"],
    features=[
        Feature(name="length_in_miles", dtype=ValueType.INT32),
    ],
    batch_source=sql_data_source,
    ttl=timedelta(seconds=60)
)

#client.create_entity(entity)
#client.create_feature_view(driver_stats_fv)

training_df = client.local.get_historical_features(
    entity_df='select driver_id, start_time from sample.trips where driver_id = 1',
    features=[
        "driver_activity_test:length_in_miles",
    ],
).to_df()

print(training_df.head())