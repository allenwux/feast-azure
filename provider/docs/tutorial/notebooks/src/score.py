# Copyright (c) Microsoft Corporation.
# Licensed under the MIT license.

import os

import logging
import json
import joblib
from feast import FeatureStore, RepoConfig
from feast.registry import RegistryConfig
from feast_azure_provider.mssqlserver import MsSqlServerOfflineStoreConfig
from feast.infra.online_stores.redis import RedisOnlineStoreConfig, RedisOnlineStore


def init():

    sql_conn_str = os.getenv("FEAST_SQL_CONN")
    redis_conn_str = os.getenv("FEAST_REDIS_CONN")
    feast_registry_path = os.getenv("FEAST_REGISTRY_BLOB")

    reg_config = RegistryConfig(
        registry_store_type="feast_azure_provider.registry_store.AzBlobRegistryStore",
        path=feast_registry_path,
    )

    repo_cfg = RepoConfig(
        project="production",
        provider="feast_azure_provider.azure_provider.AzureProvider",
        registry=reg_config,
        offline_store=MsSqlServerOfflineStoreConfig(connection_string=sql_conn_str),
        online_store=RedisOnlineStore(connection_string=redis_conn_str),
    )
    global store
    store = FeatureStore(config=repo_cfg)

    global model
    # AZUREML_MODEL_DIR is an environment variable created during deployment.
    # It is the path to the model folder (./azureml-models/$MODEL_NAME/$VERSION)
    model_path = os.path.join(os.getenv("AZUREML_MODEL_DIR"), "model/model.pkl")
    # deserialize the model file back into a sklearn model
    model = joblib.load(model_path)


def run(raw_data):
    data = json.loads(raw_data)
    feature_vector = store.get_online_features(
        features=[
            "driver_stats:conv_rate",
            "driver_stats:avg_daily_trips",
            "driver_stats:acc_rate",
            "customer_profile:current_balance",
            "customer_profile:avg_passenger_count",
            "customer_profile:lifetime_trip_count",
        ],
        entity_rows=[data],
    ).to_df()
    logging.info(feature_vector)
    if len(feature_vector.dropna()) > 0:
        data = feature_vector[
            [
                "conv_rate",
                "avg_daily_trips",
                "acc_rate",
                "current_balance",
                "avg_passenger_count",
                "lifetime_trip_count",
            ]
        ]

        y_hat = model.predict(data)
        return y_hat.tolist()
    else:
        return 0.0
