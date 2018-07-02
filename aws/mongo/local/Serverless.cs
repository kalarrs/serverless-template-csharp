using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kalarrs.Sreverless.NetCore
{
    public class HttpEvent
    {
        private static readonly Regex RouteParamRegex = new Regex("\\{(.*?)\\}");

        public Dictionary<string, string> Environment { get; set; }
        public string Handler { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
        public bool Cors { get; set; }

        public string PathToExpressRouteParameters()
        {
            return RouteParamRegex.Replace(Path, ":$1");
        }
    }
}