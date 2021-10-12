from datetime import timedelta, datetime
import pandas as pd

from feast import Entity, Feature, FeatureView, ValueType, FeatureService
from feast_azure_provider.feature_store_client import FeatureStoreClient
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig, MsSqlServerSource


print("create the client")
client = FeatureStoreClient(name = 'feastplusplus', 
    project_name='myProject', 
    local_cache_file_path='registry.db', 
    aad_auth = True,
    aad_tenant_id = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    aad_client_id="6f8f795b-2c08-4e93-b142-15642cc011e3")

try:
    print("create entity")
    entity = Entity(name='cats', value_type=ValueType.INT32, join_key='id')
    client.create_entity(entity)
    entities = client.list_entities()

    print("update entity")
    entity = Entity(name='cats', value_type=ValueType.INT32, join_key='cat_id')
    client.update_entity(entity)
    entities = client.list_entities()

    print("get entity")
    entity = client.get_entity('cats')

    print("apply (create or update) entity")
    client.apply_entity(entity)

    sql_data_source = MsSqlServerSource(
        table_ref="sample.trips",
        event_timestamp_column="start_time"
    )

    cat_activity_fv = FeatureView(
        name="cat_activity",
        entities=["cats"],
        features=[
            Feature(name="sleep_hours", dtype=ValueType.INT32),
        ],
        batch_source=sql_data_source,
        ttl=timedelta(seconds=60)
    )

    print("create feature view")
    client.create_feature_view(cat_activity_fv)

    print("get feature view")
    client.get_feature_view("cat_activity")

    cat_activity_fv = FeatureView(
        name="cat_activity",
        entities=["cats"],
        features=[
            Feature(name="sleep_hours", dtype=ValueType.INT32),
            Feature(name="food", dtype=ValueType.STRING),
        ],
        batch_source=sql_data_source,
        ttl=timedelta(seconds=60)
    )

    print("update feature view")
    client.update_feature_view(cat_activity_fv)
    feature_views = client.list_feature_views()

    print("apply (create or update) feature view")
    client.apply_feature_view(cat_activity_fv)


    cat_activity_fs = FeatureService(
        name="cat_activity_service",
        features=[cat_activity_fv]
    )

    print("create feature service")
    client.create_feature_service(cat_activity_fs)

    print("apply (create or udpate) feature service")
    client.apply_feature_service(cat_activity_fs)

    print("get feature service")
    client.get_feature_service("cat_activity_service")

    cat_activity_fs = FeatureService(
        name="cat_activity_service",
        features=[cat_activity_fv[["food"]]]
    )

    print("update feature service")
    client.update_feature_service(cat_activity_fs)
except BaseException as e:
    print('test failed')
    print(str(e))
finally:
    print("delete objects")
    client.delete_feature_view("cat_activity")
    client.delete_entity("cats")
    client.delete_feature_service("cat_activity_service")

    entities = client.list_entities()
    feature_views = client.list_feature_views()
    feature_services = client.list_feature_services()

    print(len(entities))
    print(len(feature_views))
    print(len(feature_services))

try:
    entity = Entity(name='cats', value_type=ValueType.INT32, join_key='id')

    sql_data_source = MsSqlServerSource(
        table_ref="sample.trips",
        event_timestamp_column="start_time"
    )

    cat_activity_fv = FeatureView(
        name="cat_activity",
        entities=["cats"],
        features=[
            Feature(name="sleep_hours", dtype=ValueType.INT32),
        ],
        batch_source=sql_data_source,
        ttl=timedelta(seconds=60)
    )

    cat_activity_fs = FeatureService(
        name="cat_activity_service",
        features=[cat_activity_fv]
    )

    print("apply all")
    client.apply_all([entity, cat_activity_fv, cat_activity_fs])
    entity = client.get_entity('cats')
    client.get_feature_view("cat_activity")
    client.get_feature_service("cat_activity_service")

except BaseException as e:
    print('test failed')
    print(str(e))
finally:
    print("delete objects")
    client.delete_feature_view("cat_activity")
    client.delete_entity("cats")
    client.delete_feature_service("cat_activity_service")

    entities = client.list_entities()
    feature_views = client.list_feature_views()
    feature_services = client.list_feature_services()

    print(len(entities))
    print(len(feature_views))
    print(len(feature_services))