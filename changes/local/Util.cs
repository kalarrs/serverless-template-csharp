using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
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
    }
}