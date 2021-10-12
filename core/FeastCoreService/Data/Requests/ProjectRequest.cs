using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class ProjectRequest
    {
        [JsonProperty(PropertyName = "projectName", Required = Required.Always)]
        public string ProjectName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "online_store", Required = Required.Default)]
        public BaseDataStore OnlineStore { get; set; }

        [JsonProperty(PropertyName = "offline_store", Required = Required.Default)]
        public BaseDataStore OfflineStore { get; set; }

        [JsonProperty(PropertyName = "provider", Required = Required.Default)]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "flags", Required = Required.Default)]
        public string Flags { get; set; }

        [JsonProperty(PropertyName = "isDefault", Required = Required.Default)]
        public bool IsDefault { get; set; }
    }
}
