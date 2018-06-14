using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace mongo.utils
{
    public static class ApiGatewayUtil
    {
        public static readonly JsonSerializerSettings DefaultSerializerSettings =
            new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()};

        public static readonly Dictionary<string, string> DefaultHeaders =
            new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}, // Required for CORS support to work
                {"Access-Control-Allow-Credentials", "true"},
            };
        
        public class ApiResponse<T>
        {
            public T Data { get; set; }
        }


        public class ApiError
        {
            public enum ErrorType
            {
                ApiError,
                Other
            }

            [JsonConverter(typeof(StringEnumConverter))]
            public ErrorType Type { get; set; }

            public string Message { get; set; }
        }
    }
}