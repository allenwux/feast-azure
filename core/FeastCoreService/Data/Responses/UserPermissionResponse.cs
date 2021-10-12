using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class UserPermissionResponse
    {
        [JsonProperty(PropertyName = "userId", Required = Required.Always)]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "userName", Required = Required.Always)]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "role", Required = Required.Always)]
        public int Role { get; set; }

        [JsonProperty(PropertyName = "projectName", Required = Required.Default)]
        public string ProjectName { get; set; }

        [JsonProperty(PropertyName = "permission", Required = Required.Default)]
        public int Permission { get; set; }

        [JsonProperty(PropertyName = "createdTime", Required = Required.Always)]
        public DateTime CreatedTime { get; set; }

        [JsonProperty(PropertyName = "lastUpdatedTime", Required = Required.Always)]
        public DateTime LastUpdatedTime { get; set; }

    }
}
