using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Kalarrs.Sreverless.NetCore;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;

namespace Kalarrs.Serverless.NetCore.Core
{
    public class ServerlessProject
    {
        private readonly string _path;
        private readonly object _serverlessYaml;
        private readonly JObject _environmentJson;

        private static readonly Regex RoutePrefixSuffixRegex = new Regex("(^/|/$)");
        private static readonly Regex HandlerRegex = new Regex(".*?\\.Handler::(.+)$");

        private const string DefaultPort = "5000";
        
        public ServerlessProject(string path = null)
        {
            _path = path ?? Directory.GetParent(Directory.GetCurrentDirectory()).FullName;;
            Export();
            
            _serverlessYaml = ReadServerlessYaml();
            _environmentJson = ReadEnvironmentJson();
        }

        private object ReadServerlessYaml()
        {
            var serverlessYaml = File.ReadAllText($"{_path}/serverless.yml");

            var deserializer = new Deserializer();
            return deserializer.Deserialize(new StringReader(serverlessYaml));
        }

        private JObject ReadEnvironmentJson()
        {
            var environmentJson = File.ReadAllText($"{_path}/.serverless/environment.json");
            return JObject.Parse(environmentJson);
        }

        public Dictionary<string, string> GetEnvironmentVariables()
        {
            /*
            var environmentVariables = new Dictionary<string, string>();
            foreach (var keyValuePair in _environmentJson)
            {
                environmentVariables.Add(keyValuePair.Key, keyValuePair.Value.ToString());                    
            }
            return environmentVariables;
            */
            return _environmentJson.ToObject<Dictionary<string, string>>();
        }

        public IEnumerable<HttpEvent> GetHttpEvents()
        {
            var httpEvents = new List<HttpEvent>();

            var functions = (_serverlessYaml as Dictionary<object, object>)?["functions"];
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
            var yamlDictonary = (Dictionary<object, object>) _serverlessYaml;
            if (yamlDictonary == null || !yamlDictonary.ContainsKey("custom")) return DefaultPort;

            var custom = yamlDictonary["custom"];
            var localDevPortDictonary = (Dictionary<object, object>) custom;
            if (localDevPortDictonary == null || !localDevPortDictonary.ContainsKey("localDevPort")) return DefaultPort;

            return localDevPortDictonary["localDevPort"].ToString() ?? DefaultPort;
        }

        private void Export()
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"sls export\"",
                    WorkingDirectory = _path,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}