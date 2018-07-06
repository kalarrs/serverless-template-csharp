using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace Kalarrs.NetCore.Util
{
    public class HttpConfig
    {
        private static readonly Regex RouteParamRegex = new Regex("\\{(.*?)\\}");
        private string _path;

        public Dictionary<string, string> Environment { get; set; }
        public string Handler { get; set; }
        public HttpMethod? Method { get; set; }

        public string Path
        {
            get => EventType.ToString().ToLowerInvariant() + "/" + _path;
            set => _path = value;
        }

        public bool Cors { get; set; }
        public EventType EventType { get; set; }
        public JObject RequestBody { get; set; }

        public string PathToExpressRouteParameters()
        {
            return RouteParamRegex.Replace(Path, ":$1");
        }
    }
}