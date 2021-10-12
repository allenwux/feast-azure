using System;
using System.Collections.Generic;
using System.Text;
using Feast.Core;
using Newtonsoft.Json;

namespace Azure.Feast.Data
{
    public class FeatureServiceRequest
    {
        [JsonProperty(PropertyName = "data", Required = Required.Default)]
        public FeatureService Data { get; set; }

        [JsonProperty(PropertyName = "proto", Required = Required.Default)]
        public string Proto { get; set; }
    }
}
