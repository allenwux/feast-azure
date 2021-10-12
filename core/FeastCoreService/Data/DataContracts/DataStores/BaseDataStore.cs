using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    [JsonConverter(typeof(DataStoreJsonConverter))]
    public class BaseDataStore
    {
        [JsonProperty(PropertyName = "type", Required = Required.Default)]
        public string Type { get; set; }
    }
}
