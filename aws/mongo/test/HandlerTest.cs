using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;


namespace mongo.Tests
{
    public class HandlerTest
    {
        public HandlerTest()
        {
        }

        [Fact]
        public async void TetGetMethod()
        {
            TestLambdaContext context;
            APIGatewayProxyRequest request;
            APIGatewayProxyResponse response;

            Handler functions = new Handler();


            request = new APIGatewayProxyRequest();
            context = new TestLambdaContext();
            response = await functions.GetUserGroups(request, context);
            Assert.Equal(200, response.StatusCode);

            var expectedBody =
                "{\"data\":[{\"id\":\"5a1b5ae36758c40453e5e024\",\"description\":\"This is an example\"},{\"id\":\"5a1b5b176758c40453e5e025\",\"description\":\"of a simple mock API\"}]}";

            Assert.Equal(expectedBody, response.Body);
        }
    }
}