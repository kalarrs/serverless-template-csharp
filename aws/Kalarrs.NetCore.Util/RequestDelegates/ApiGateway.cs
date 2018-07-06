using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Kalarrs.NetCore.Util.Extensions;
using Microsoft.AspNetCore.Http;

namespace Kalarrs.NetCore.Util.RequestDelegates
{
    public static class ApiGateway
    {
        public static RequestDelegate ApiGatewayHandler<T>(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, HttpConfig httpConfig, MethodBase handlerMethod, IReadOnlyList<ParameterInfo> parameterInfos, T handler)
        {
            return async (context) =>
            {
                APIGatewayProxyResponse response;
                var apiGatewayProxyRequest = await context.ToApiGatewayProxyRequest(httpConfig.Path).ConfigureAwait(false);

                EnvironmentVariable.PrepareEnvironmentVariables(defatultEnvironmentVariables, serverlessEnvironmentVariables,httpConfig.Environment);
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