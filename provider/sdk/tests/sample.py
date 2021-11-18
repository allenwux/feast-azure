

sdf = feature_store.get_historical_features(
        features=driver_stats_fs, 
        entity_df='select driver_id, start_time from sample.trips'
    ).to_sdf()

sdf.plot()
sdf.data_prep()

model = sklearn.train(sdf)

Model.register(ws, model, tags=sdf.get_tags())




    loan_activity_fv = FeatureView(
        name="loan_activity",
        entities=["customer"],
        features=[
            Feature(name="occupation", dtype=ValueType.STRING),
            Feature(name="age", dtype=ValueType.INT32),
            Feature(name="zip_code", dtype=ValueType.INT32),
        ],
        batch_source=sql_data_source,
        ttl=timedelta(seconds=60)
    )