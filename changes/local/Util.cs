using System;
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

        public static void AddRoutes<T>(this IRouteBuilder routeBuilder, IEnumerable<HttpEvent> httpEvents) where T : new()
        {
            var handler = new T();
            var handlerType = handler.GetType();
            
            foreach (var httpEvent in httpEvents)
            {
                // TODO : Options. If route has cors then return correct headers.
                
                var handlerMethod = handlerType.GetMethod(httpEvent.Handler);
                if (handlerMethod == null) throw new Exception("The Method was not found!"); // TODO: Return a 500 with appropriate error.

                var cb = HandleRoute(httpEvent, handlerMethod, handlerMethod);
                
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
        }

        private static RequestDelegate HandleRoute<T>(HttpEvent httpEvent, MethodBase handlerMethod, T handler)
        {
            return async (context) =>
            {
                var apiGatewayProxyRequest = await context.ToAPIGatewayProxyRequest(httpEvent.Path);

                if (!(handlerMethod.Invoke(handler, new object[] {apiGatewayProxyRequest, new TestLambdaContext()}) is APIGatewayProxyResponse response)) 
                    throw new Exception("The Method did not send a response.");

                if (response.Headers.Any())
                {
                    foreach (var header in response.Headers)
                    {
                        context.Response.Headers.Add(header.Key, header.Value);
                    }
                }

                context.Response.StatusCode = response.StatusCode;
                await context.Response.WriteAsync(response.Body);
            };
        }
    }
}