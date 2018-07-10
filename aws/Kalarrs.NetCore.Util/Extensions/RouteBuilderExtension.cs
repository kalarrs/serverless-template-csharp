using System;
using Kalarrs.NetCore.Util.RequestDelegates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kalarrs.NetCore.Util.Extensions
{
    public static class RouteBuilderExtension
    {
        public static void AddRoutes<T>(this IRouteBuilder routeBuilder, ServerlessProject serverlessProject) where T : new()
        {
            var handler = new T();
            var handlerType = handler.GetType();
            var handlerPathPrefix = $"CsharpHandlers::{handlerType.Namespace}.{handlerType.Name}::";         

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("----");
            Console.ResetColor();
            foreach (var httpConfig in serverlessProject.GetHttpConfigs())
            {
                // TODO : Options. If route has cors then return correct headers.
                var configHandlerPathPrefix = $"{httpConfig.Assembly}::{httpConfig.Namespace}.{httpConfig.ClassName}::";
                if (handlerPathPrefix != configHandlerPathPrefix) continue;

                var handlerMethod = handlerType.GetMethod(httpConfig.MethodName);
                if (handlerMethod == null) throw new Exception("The Method was not found!"); // TODO: Return a 500 with appropriate error.
                var parameters = handlerMethod.GetParameters();

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"{httpConfig.MethodName}:");
                Console.Write($" {httpConfig.Method} ");
                Console.ResetColor();
                Console.Write($"http://localhost:{serverlessProject.Port}/{httpConfig.PathToExpressRouteParameters()}\n");


                RequestDelegate cb;
                switch (httpConfig.EventType)
                {
                    case EventType.Http:
                        cb = ApiGateway.ApiGatewayHandler(serverlessProject.DefaultEnvironmentVariables, serverlessProject.EnvironmentVariables, httpConfig, handlerMethod, parameters, handler);
                        break;
                    case EventType.Schedule:
                        cb = Schedule.ScheduleHandler(serverlessProject.DefaultEnvironmentVariables, serverlessProject.EnvironmentVariables, httpConfig, handlerMethod, parameters, handler);
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
    }
}