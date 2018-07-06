using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Kalarrs.Serverless.NetCore.Core;
using mongo.Local;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;

namespace Kalarrs.Serverless.NetCore.Util
{
    public static class Extensions
    {
        public static async Task<APIGatewayProxyRequest> ToApiGatewayProxyRequest(this HttpContext context, string resource)
        {
            var request = context.Request;
            string body = null;

            if (request.Body != null)
            {
                using (var reader = new StreamReader(request.Body, Encoding.UTF8))
                {
                    body = await reader.ReadToEndAsync().ConfigureAwait(false);
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

        public static void AddRoutes<T>(this IRouteBuilder routeBuilder, ServerlessProject serverlessProject) where T : new()
        {
            var handler = new T();
            var handlerType = handler.GetType();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----");
            Console.ResetColor();
            foreach (var httpConfig in serverlessProject.GetHttpConfigs())
            {
                // TODO : Options. If route has cors then return correct headers.

                var handlerMethod = handlerType.GetMethod(httpConfig.Handler);
                if (handlerMethod == null) throw new Exception("The Method was not found!"); // TODO: Return a 500 with appropriate error.
                var parameters = handlerMethod.GetParameters();

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{httpConfig.Handler}:");
                Console.Write($" {httpConfig.Method} ");
                Console.ResetColor();
                Console.Write($"http://localhost:{serverlessProject.Port}/{httpConfig.EventType.ToString().ToLowerInvariant()}/{httpConfig.PathToExpressRouteParameters()}\n");


                RequestDelegate cb;
                switch (httpConfig.EventType)
                {
                    case EventType.Http:
                        cb = ApiGatewayHandler(serverlessProject.DefaultEnvironmentVariables, serverlessProject.EnvironmentVariables, httpConfig, handlerMethod, parameters, handler);
                        break;
                    case EventType.Schedule:
                        cb = ScheduleHandler(serverlessProject.DefaultEnvironmentVariables, serverlessProject.EnvironmentVariables, httpConfig, handlerMethod, parameters, handler);
                        break;
                    default:
                        continue;
                }

                switch (httpConfig.Method)
                {
                    case HttpMethod.Get:
                        routeBuilder.MapGet(httpConfig.Path, cb);
                        break;
                    case HttpMethod.Post:
                    {
                        routeBuilder.MapPost(httpConfig.Path, cb);
                        break;
                    }
                    case HttpMethod.Put:
                        routeBuilder.MapPut(httpConfig.Path, cb);
                        break;
                    case HttpMethod.Delete:
                        routeBuilder.MapDelete(httpConfig.Path, cb);
                        break;
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----");
            Console.ResetColor();
        }

        private static RequestDelegate ApiGatewayHandler<T>(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, HttpConfig httpConfig, MethodBase handlerMethod, IReadOnlyList<ParameterInfo> parameterInfos, T handler)
        {
            return async (context) =>
            {
                APIGatewayProxyResponse response;
                var apiGatewayProxyRequest = await context.ToApiGatewayProxyRequest(httpConfig.Path).ConfigureAwait(false);

                PrepareEnvironmentVariables(defatultEnvironmentVariables, serverlessEnvironmentVariables,httpConfig.Environment);
                var handlerResponse = handlerMethod.Invoke(handler, new object[] {apiGatewayProxyRequest, new TestLambdaContext()});

                if (handlerResponse is Task<APIGatewayProxyResponse> task) response = await task.ConfigureAwait(false);
                else if (handlerResponse is APIGatewayProxyResponse proxyResponse) response = proxyResponse;
                else throw new Exception("The Method did not return an APIGatewayProxyResponse.");

                if (response.Headers.Any())
                {
                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers.Add(header.Key, header.Value);
                    }
                }

                context.Response.StatusCode = response.StatusCode;
                if (response.Body != null) await context.Response.WriteAsync(response.Body).ConfigureAwait(false);
            };
        }
        
        private static RequestDelegate ScheduleHandler<T>(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, HttpConfig httpConfig, MethodBase handlerMethod, IReadOnlyList<ParameterInfo> parameterInfos, T handler)
        {
            return async (context) =>
            {
                object request = new ScheduledEvent();
                var defaultScheduleEvent = new ScheduledEvent()
                {
                    Account = "123456789012",
                    Region = "us-east-1",
                    Detail = { },
                    DetailType = "Scheduled Event",
                    Source = "aws.events",
                    Time = DateTime.UtcNow,
                    Id = "cdc73f9d-aea9-11e3-9d5a-835b769c0d9c",
                    Resources = new List<string>() {"arn:aws:events:us-east-1:123456789012:rule/my-schedule"}
                };
                var type = parameterInfos.Count > 0 ? parameterInfos[0].ParameterType : null;
                if (httpConfig.RequestBody != null && type != null) request = JsonConvert.DeserializeObject(httpConfig.RequestBody.ToString(), type); 

                PrepareEnvironmentVariables(defatultEnvironmentVariables, serverlessEnvironmentVariables, httpConfig.Environment);
                var handlerResponse = handlerMethod.Invoke(handler, new object[] {request, new TestLambdaContext()});
                    

                object response;
                if (handlerResponse is Task<object> task) response = await task.ConfigureAwait(false);
                else if (handlerResponse != null) response = handlerResponse;
                else throw new Exception("The Method did not return an APIGatewayProxyResponse.");


                context.Response.StatusCode = 200;
                if (response != null) await context.Response.WriteAsync(response.ToString()).ConfigureAwait(false);
            };
        }

        private static void PrepareEnvironmentVariables(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, Dictionary<string, string> functionEnvironmentVariables)
        {
            // Remove all ENV variables.
            foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
                Environment.SetEnvironmentVariable(environmentVariable.Key.ToString(), string.Empty, EnvironmentVariableTarget.Process);
            // Restore default ENV variables.
            if (defatultEnvironmentVariables != null)
                foreach (DictionaryEntry defaultEnvironmentVariable in defatultEnvironmentVariables)
                    Environment.SetEnvironmentVariable(defaultEnvironmentVariable.Key.ToString(), defaultEnvironmentVariable.Value.ToString(), EnvironmentVariableTarget.Process);
            // Set Serverless Provider Level ENV variables.
            if (serverlessEnvironmentVariables != null)
                foreach (var serverlessKeyValuePair in serverlessEnvironmentVariables)
                    Environment.SetEnvironmentVariable(serverlessKeyValuePair.Key, serverlessKeyValuePair.Value, EnvironmentVariableTarget.Process);
            // Set Serverless Function Level ENV variables.
            if (functionEnvironmentVariables != null)
                foreach (var functionKeyValuePair in functionEnvironmentVariables)
                    Environment.SetEnvironmentVariable(functionKeyValuePair.Key, functionKeyValuePair.Value, EnvironmentVariableTarget.Process);
        }
    }
}