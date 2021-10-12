using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class MsSqlServerOfflineStore : BaseDataStore
    {
        [JsonProperty(PropertyName = "connection_string", Required = Required.Always)]
        public string ConnectionString { get; set; }
    }
}
