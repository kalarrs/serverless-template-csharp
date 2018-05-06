using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Kalarrs.Sreverless.NetCore;
using YamlDotNet.Serialization;

namespace Kalarrs.Serverless.NetCore.Yaml
{
    public class Parser
    {
        private readonly string _path;
        private readonly object _yaml;

        private static readonly Regex RoutePrefixSuffixRegex = new Regex("(^/|/$)");
        private static readonly Regex HandlerRegex = new Regex(".*?\\.Handler::(.+)$");

        private const string DefaultPort = "5000";

        public Parser(string path = "../serverless.yml")
        {
            _path = path;
            _yaml = ReadYaml();
        }

        private object ReadYaml()
        {
            var serverlessYaml = File.ReadAllText(_path);

            var deserializer = new Deserializer();
            return deserializer.Deserialize(new StringReader(serverlessYaml));
        }

        public IEnumerable<HttpEvent> GetHttpEvents()
        {
            var httpEvents = new List<HttpEvent>();

            var functions = (_yaml as Dictionary<object, object>)?["functions"];
            if (functions == null) return httpEvents;

            foreach (var function in (Dictionary<object, object>) functions)
            {
                var handlerName = (function.Value as Dictionary<object, object>)?["handler"]?.ToString();
                var events = (function.Value as Dictionary<object, object>)?["events"];
                if (handlerName == null || events == null) continue;

                foreach (var @event in (List<object>) events)
                {
                    var http = (@event as Dictionary<object, object>)?["http"];
                    var httpDictonary = (Dictionary<object, object>) http;
                    if (httpDictonary == null) continue;

                    var path = httpDictonary["path"]?.ToString();

                    httpEvents.Add(new HttpEvent()
                    {
                        Handler = HandlerRegex.Replace(handlerName, "$1"),
                        Method = httpDictonary["method"]?.ToString().ToUpperInvariant(),
                        Path = path == null ? null : RoutePrefixSuffixRegex.Replace(path, ""),
                        Cors = httpDictonary["method"]?.ToString() == "true"
                    });
                }
            }

            return httpEvents;
        }

        public string GetPort()
        {
            var yamlDictonary = (Dictionary<object, object>) _yaml;
            if (yamlDictonary == null || !yamlDictonary.ContainsKey("custom")) return DefaultPort;

            var custom = yamlDictonary["custom"];
            var localDevPortDictonary = (Dictionary<object, object>) custom;
            if (localDevPortDictonary == null || !localDevPortDictonary.ContainsKey("localDevPort")) return DefaultPort;

            return localDevPortDictonary["localDevPort"].ToString() ?? DefaultPort;
        }
    }
}