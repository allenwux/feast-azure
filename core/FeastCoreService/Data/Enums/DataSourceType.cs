using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public enum DataSourceType
    {
        INVALID = 0,
        BATCH_FILE = 1,
        BATCH_BIGQUERY = 2,
        STREAM_KAFKA = 3,
        STREAM_KINESIS = 4,
        BATCH_REDSHIFT = 5,
        CUSTOM_SOURCE = 6,
        REQUEST_SOURCE = 7,
    }
}
