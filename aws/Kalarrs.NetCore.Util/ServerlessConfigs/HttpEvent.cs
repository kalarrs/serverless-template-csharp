using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kalarrs.NetCore.Util.ServerlessConfigs
{
    public class HttpEvent
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public HttpMethod? Method { get; set; }
        public string Path { get; set; }
        public bool Cors { get; set; }
    }
}