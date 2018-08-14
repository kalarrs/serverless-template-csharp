using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

namespace template.Test
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
            /*
            response = Handler.Get(request, context);
            Assert.Equal(200, response.StatusCode);

            const string expectedBody = "{\"data\":[{\"id\":\"5a1b5ae36758c40453e5e024\",\"description\":\"This is an example\"},{\"id\":\"5a1b5b176758c40453e5e025\",\"description\":\"of a simple mock API\"}]}";
            Assert.Equal(expectedBody, response.Body);
            */
        }
    }
}
