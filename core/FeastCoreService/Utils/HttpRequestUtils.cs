using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace Azure.Feast.Utils
{

    public class FeastCoreRequestHeaders
    {
        public string UserId;

        public string UserName;

        public string APIVersion;

        public string TraceId;

        public Dictionary<string, object> GetLoggingScopeProperties()
        {
            var properties = new Dictionary<string, object>();
            properties.Add("Feast.UserName", this.UserName);
            properties.Add("Feast.UserId", this.UserId);
            properties.Add("Feast.APIVersion", this.APIVersion);
            properties.Add("Feast.TraceId", this.TraceId);
            return properties;
        }
    }

    public class HttpRequestUtils
    {
        private const string AAD_USER_ID_HEADER_NAME = "X-MS-CLIENT-PRINCIPAL-ID";
        private const string AAD_USER_NAME_HEADER_NAME = "X-MS-CLIENT-PRINCIPAL-NAME";

        private const string USER_ID_HEADER_NAME = "Feast-Core-User-Id";
        private const string USER_NAME_HEADER_NAME = "Feast-Core-User-Name";

        private const string API_VERSION_HEADER_NAME = "api-version";
        private const string TRACE_ID_HEADER_NAME = "trace-id";

        private const string DEFAULT_API_VERSION = "0.13";

        public static FeastCoreRequestHeaders ParseHeaders(HttpRequest req)
        {
            var headers = new FeastCoreRequestHeaders();

            headers.UserId = req.Headers.ContainsKey(USER_ID_HEADER_NAME) ?
                req.Headers[USER_ID_HEADER_NAME].ToString() : string.Empty;

            // Overwrite the user id if AAD auth is used
            headers.UserId = req.Headers.ContainsKey(AAD_USER_ID_HEADER_NAME) ?
                req.Headers[AAD_USER_ID_HEADER_NAME].ToString() : headers.UserId;

            headers.UserName = req.Headers.ContainsKey(USER_NAME_HEADER_NAME) ?
                req.Headers[USER_NAME_HEADER_NAME].ToString() : string.Empty;

            // Overwrite the user name if AAD auth is used
            headers.UserName = req.Headers.ContainsKey(AAD_USER_NAME_HEADER_NAME) ?
                req.Headers[AAD_USER_NAME_HEADER_NAME].ToString() : headers.UserName;

            // Also support providing api version in query parameters
            headers.APIVersion = req.Query.ContainsKey(API_VERSION_HEADER_NAME) ?
                req.Query[API_VERSION_HEADER_NAME].ToString() : DEFAULT_API_VERSION;

            headers.APIVersion = req.Headers.ContainsKey(API_VERSION_HEADER_NAME) ?
                req.Headers[API_VERSION_HEADER_NAME].ToString() : headers.APIVersion;

            headers.TraceId = req.Headers.ContainsKey(TRACE_ID_HEADER_NAME) ?
                req.Headers[TRACE_ID_HEADER_NAME].ToString() : Guid.NewGuid().ToString();

            return headers;

        }
    }
}
