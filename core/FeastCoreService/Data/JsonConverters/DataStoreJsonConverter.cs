using Azure.Feast.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Feast.Data
{
    public class DataStoreJsonConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.BaseType == typeof(BaseDataStore);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken jObject = JToken.ReadFrom(reader);

            if (jObject["type"] == null)
            {
                // TODO: need to find a way to back fill the trace id
                throw new FeastCoreBadRequestUserException(
                    ErrorUtils.FormatErrorMessage(ErrorMessages.MISSING_PARAMETER, "type"), 
                    (int)ErrorCode.MISSING_PARAMETER,
                    string.Empty);
            }

            BaseDataStore result;
            switch (jObject["type"].ToString())
            {
                case DataStoreType.Redis:
                    result = new RedisOnlineStore();
                    break;
                case DataStoreType.MsSql:
                    result = new MsSqlServerOfflineStore();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            serializer.Populate(jObject.CreateReader(), result);

            return result;
        }
    }
}
