using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using changes;

namespace changes.Tests
{
    public class HandlerTest
    {
        [Fact]
        public void TetGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = Handler.GetChanges(request, context);
            Assert.Equal(200, response.StatusCode);

            const string expectedBody = "{\"data\":[{\"id\":\"5a1b5ae36758c40453e5e024\",\"description\":\"This is an example\"},{\"id\":\"5a1b5b176758c40453e5e025\",\"description\":\"of a simple mock API\"}]}";

            Assert.Equal(expectedBody, response.Body);
        }
    }
}
