using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using changes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;

namespace tmp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var routeBuilder = new RouteBuilder(app);

            var serverlessYaml = File.ReadAllText("../serverless.yml");

            var deserializer = new Deserializer();
            var yamlObject = deserializer.Deserialize(new StringReader(serverlessYaml));


            //var routeRegex = new Regex(":(.*)?(/|$)");
            var routePrefixSuffixRegex = new Regex("(^/|/$)");
            var handlerRegex = new Regex(".*?\\.Handler::(.+)$");
            var httpEvents = new List<HttpEvent>();

            var functions = (yamlObject as Dictionary<object, object>)?["functions"];
            if (functions != null)
            {
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

                        // TODO : Get the "cors" value out and if so add Option route that returns accept headers.
                        httpEvents.Add(new HttpEvent()
                        {
                            Handler = handlerRegex.Replace(handlerName, "$1"),
                            Method = httpDictonary["method"]?.ToString().ToUpperInvariant(),
                            Path = path == null ? null : routePrefixSuffixRegex.Replace(path, "")
                        });
                    }
                }
            }

            var handler = new Handler();
            var handlerType = handler.GetType();

            foreach (var httpEvent in httpEvents)
            {
                switch (httpEvent.Method)
                {
                    case "GET":
                        routeBuilder.MapGet(httpEvent.Path, async context => 
                        {
                            var handlerMethod = handlerType.GetMethod(httpEvent.Handler);
                            // TODO: Return a 500 with appropriate error.
                            if (handlerMethod == null) throw new Exception("The Method was not found!");

                            var apiGatewayProxyRequest = await context.ToAPIGatewayProxyRequest(httpEvent.Path);

                            if (!(handlerMethod.Invoke(handler, new object[] {apiGatewayProxyRequest, new TestLambdaContext()}) is APIGatewayProxyResponse response)) throw new Exception("The Method did not send a response.");
                            
                            if (response.Headers.Any())
                            {
                                foreach (var header in response.Headers)
                                {
                                    context.Response.Headers.Add(header.Key, header.Value);
                                }
                            }
                            context.Response.StatusCode = response.StatusCode;
                            await context.Response.WriteAsync(response.Body);
                        });
                        break;
                    case "POST":
                        routeBuilder.MapPost(httpEvent.Path, async context =>
                        {
                            var handlerMethod = handlerType.GetMethod(httpEvent.Handler);
                            // TODO: Return a 500 with appropriate error.
                            if (handlerMethod == null) throw new Exception("The Method was not found!");

                            var apiGatewayProxyRequest = await context.ToAPIGatewayProxyRequest(httpEvent.Path);

                            if (!(handlerMethod.Invoke(handler, new object[] {apiGatewayProxyRequest, new TestLambdaContext()}) is APIGatewayProxyResponse response)) throw new Exception("The Method did not send a response.");

                            if (response.Headers.Any())
                            {
                                foreach (var header in response.Headers)
                                {
                                    context.Response.Headers.Add(header.Key, header.Value);
                                }
                            }
                            context.Response.StatusCode = response.StatusCode;
                            await context.Response.WriteAsync(response.Body);
                        });
                        break;
                }
            }

            var routes = routeBuilder.Build();
            app.UseRouter(routes);
        }
    }

    public class HttpEvent
    {
        public string Handler { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }

    public static class Util
    {
        public static async Task<APIGatewayProxyRequest> ToAPIGatewayProxyRequest(this HttpContext context, string resource)
        {
            var request = context.Request;
            string body = null;
            
            if (request.Body != null) {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                {  
                    body = await reader.ReadToEndAsync();
                }
            }

            return new APIGatewayProxyRequest()
            {
                Resource = resource,
                Path = request.Path,
                HttpMethod = request.Method,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                QueryStringParameters = request.QueryString.ToString()
                    .Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Split("&")).ToDictionary(s => s[0], s => s[1]),
                PathParameters = context.GetRouteData().Values.ToDictionary(p => p.Key, p => p.Value.ToString()),
                StageVariables = null,
                Body = body,
                IsBase64Encoded = false
            };
        }
    }
}