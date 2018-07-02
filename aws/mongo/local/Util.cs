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
using Kalarrs.Sreverless.NetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kalarrs.Serverless.NetCore.Util
{
    public static class Extensions
    {
        public static async Task<APIGatewayProxyRequest> ToAPIGatewayProxyRequest(this HttpContext context, string resource)
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

        public static async void AddRoutes<T>(this IRouteBuilder routeBuilder, IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, IEnumerable<HttpEvent> httpEvents, string port) where T : new()
        {
            var handler = new T();
            var handlerType = handler.GetType();


            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----");
            Console.ResetColor();
            foreach (var httpEvent in httpEvents)
            {
                // TODO : Options. If route has cors then return correct headers.


                var handlerMethod = handlerType.GetMethod(httpEvent.Handler);
                if (handlerMethod == null) throw new Exception("The Method was not found!"); // TODO: Return a 500 with appropriate error.

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{httpEvent.Handler}:");
                Console.Write($" {httpEvent.Method} ");
                Console.ResetColor();
                Console.Write($"http://localhost:{port}/{httpEvent.PathToExpressRouteParameters()}\n");

                var cb = await HandleRoute(defatultEnvironmentVariables, serverlessEnvironmentVariables, httpEvent, handlerMethod, handler).ConfigureAwait(false);

                switch (httpEvent.Method)
                {
                    case "GET":
                        routeBuilder.MapGet(httpEvent.Path, cb);
                        break;
                    case "POST":
                    {
                        routeBuilder.MapPost(httpEvent.Path, cb);
                        break;
                    }
                    case "PUT":
                        routeBuilder.MapPut(httpEvent.Path, cb);
                        break;
                    case "DELETE":
                        routeBuilder.MapDelete(httpEvent.Path, cb);
                        break;
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----");
            Console.ResetColor();
        }

        private static async Task<RequestDelegate> HandleRoute<T>(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, HttpEvent httpEvent, MethodBase handlerMethod, T handler)
        {
            return async (context) =>
            {
                APIGatewayProxyResponse response;
                var apiGatewayProxyRequest = await context.ToAPIGatewayProxyRequest(httpEvent.Path).ConfigureAwait(false);

                // Remove all ENV variables.
                foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process))
                    Environment.SetEnvironmentVariable(environmentVariable.Key.ToString(), string.Empty, EnvironmentVariableTarget.Process);
                // Restore default ENV values
                if (defatultEnvironmentVariables != null)
                    foreach (DictionaryEntry defaultEnvironmentVariable in defatultEnvironmentVariables)
                        Environment.SetEnvironmentVariable(defaultEnvironmentVariable.Key.ToString(), defaultEnvironmentVariable.Value.ToString(), EnvironmentVariableTarget.Process);
                // Set Serverless Provider Level
                if (serverlessEnvironmentVariables != null)
                    foreach (var serverlessKeyValuePair in serverlessEnvironmentVariables)
                        Environment.SetEnvironmentVariable(serverlessKeyValuePair.Key, serverlessKeyValuePair.Value, EnvironmentVariableTarget.Process);
                // Set Serverless Function Level
                if (httpEvent.Environment != null)
                    foreach (var functionKeyValuePair in httpEvent.Environment)
                        Environment.SetEnvironmentVariable(functionKeyValuePair.Key, functionKeyValuePair.Value, EnvironmentVariableTarget.Process);
                
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
    }
}