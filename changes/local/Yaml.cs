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
    }
}