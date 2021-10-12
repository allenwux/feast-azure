using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class RedisOnlineStore : BaseDataStore
    {
        [JsonProperty(PropertyName = "redis_type", Required = Required.Always)]
        public string RedisType { get; set; }

        [JsonProperty(PropertyName = "connection_string", Required = Required.Always)]
        public string ConnectionString { get; set; }
    }
}
