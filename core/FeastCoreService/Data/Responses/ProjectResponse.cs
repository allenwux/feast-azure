using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class ProjectResponse
    {
        [JsonProperty(PropertyName = "project", Required = Required.Always)]
        public string ProjectName { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "online_store", Required = Required.Default)]
        public object? OnlineStore { get; set; }

        [JsonProperty(PropertyName = "offline_store", Required = Required.Default)]
        public object? OfflineStore { get; set; }

        [JsonProperty(PropertyName = "flags", Required = Required.Default)]
        public string Flags { get; set; }

        [JsonProperty(PropertyName = "registry", Required = Required.Default)]
        public string Registry
        {
            get { return "azRegistry"; }
        }

        [JsonProperty(PropertyName = "provider", Required = Required.Default)]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "isDefault", Required = Required.Default)]
        public bool IsDefault { get; set; }

        [JsonProperty(PropertyName = "createdTime", Required = Required.Always)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime", Required = Required.Always)]
        public DateTime LastUpdatedTime { get; set; }
    }
}
