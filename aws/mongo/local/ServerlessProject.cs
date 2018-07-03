using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Kalarrs.Sreverless.NetCore;
using Newtonsoft.Json.Linq;

namespace Kalarrs.Serverless.NetCore.Core
{
    public class ServerlessProject
    {
        private readonly string _path;
        private readonly JObject _serverlessJson;
        private readonly ServerlessYaml _serverlessYaml;

        private static readonly Regex RoutePrefixSuffixRegex = new Regex("(^/|/$)");
        private static readonly Regex HandlerRegex = new Regex(".*?\\.Handler::(.+)$");

        private const string DefaultPort = "5000";

        public Dictionary<string, string> EnvironmentVariables => _serverlessYaml?.Provider?.Environment;

        public readonly IDictionary DefaultEnvironmentVariables =
            Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);

        public string Port => _serverlessYaml?.Custom?.LocalDevPort ?? DefaultPort;

        public ServerlessProject(string path = null)
        {
            _path = path ?? Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            Export();
            _serverlessJson = ReadServerlessJson();
            _serverlessYaml = _serverlessJson.ToObject<ServerlessYaml>();
        }

        private JObject ReadServerlessJson()
        {
            var serverlessJson = File.ReadAllText($"{_path}/.serverless/serverless.json");
            return JObject.Parse(serverlessJson);
        }

        public IEnumerable<HttpEvent> GetHttpEvents()
        {
            var httpEvents = new List<HttpEvent>();

            var functions = _serverlessYaml?.Functions;
            if (functions == null) return httpEvents;


            foreach (var funtionKeyValue in functions)
            {
                var function = funtionKeyValue.Value;
                if (function == null) continue;

                var handlerName = function.Handler;

                var events = function.Events;
                if (handlerName == null || events == null) continue;

                foreach (var @event in events)
                {
                    var http = @event.Http;
//                    var schedule = @event.Schedule;
//                    if (http == null && schedule == null) continue;

                    if (http != null)
                    {
                        httpEvents.Add(new HttpEvent()
                        {
                            Handler = HandlerRegex.Replace(handlerName, "$1"),
                            Environment = function.Environment,
                            Method = http.Method?.ToUpperInvariant(),
                            Path = http.Path == null ? null : RoutePrefixSuffixRegex.Replace(http.Path, ""),
                            Cors = http.Cors
                        });
                    }

//                    if (schedule != null)
//                    {
//                    }
                }
            }

            return httpEvents;
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

    public class ServerlessYaml
    {
        public string Service { get; set; }
        public ServerlessYamlProvider Provider { get; set; }
        public ServerlessYamlCustom Custom { get; set; }
        public Dictionary<string, ServerlessYamlFunction> Functions { get; set; }
    }

    public class ServerlessYamlProvider
    {
        public string Stage { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string Runtime { get; set; }
        public Dictionary<string, string> Environment { get; set; }
        public bool? VersionFunctions { get; set; }
    }

    public class ServerlessYamlCustom
    {
        public string LocalDevPort { get; set; }
    }

    public class ServerlessYamlFunction
    {
        public string Handler { get; set; }
        public List<ServerlessYamlFunctionHttpEvent> Events { get; set; }
        public Dictionary<string, string> Environment { get; set; }
    }

    public class ServerlessYamlFunctionHttpEvent
    {
        public ServerlessYamlFunctionHttpEventHttp Http { get; set; }
        //public JObject Schedule { get; set; }
    }

    public class ServerlessYamlFunctionHttpEventHttp
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public bool Cors { get; set; }
    }
}