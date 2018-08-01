using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Kalarrs.Lambda.ScheduledEvents;
using Amazon.Lambda.TestUtilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Kalarrs.NetCore.Util.RequestDelegates
{
    public static class Schedule
    {
        public static RequestDelegate ScheduleHandler<T>(IDictionary defatultEnvironmentVariables, Dictionary<string, string> serverlessEnvironmentVariables, HttpConfig httpConfig, MethodBase handlerMethod, IReadOnlyList<ParameterInfo> parameterInfos, T handler)
        {
            return async (context) =>
            {
                var type = parameterInfos.Count > 0 ? parameterInfos[0].ParameterType : null;
                var request = httpConfig.RequestBody != null && type != null
                    ? JsonConvert.DeserializeObject(httpConfig.RequestBody.ToString(), type)
                    : new ScheduledEvent()
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

                EnvironmentVariable.PrepareEnvironmentVariables(defatultEnvironmentVariables, serverlessEnvironmentVariables, httpConfig.Environment);
                var handlerResponse = handlerMethod.Invoke(handler, new object[] {request, new TestLambdaContext()});
                    
                object response = null;
                if (handlerResponse is Task<string> task) response = await task.ConfigureAwait(false);
                else if (handlerResponse != null) response = handlerResponse;


                context.Response.StatusCode = 200;
                if (response != null) await context.Response.WriteAsync(JsonConvert.SerializeObject(response)).ConfigureAwait(false);
            };
        }
    }
}