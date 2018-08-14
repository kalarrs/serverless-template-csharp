using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Kalarrs.Lambda.Serialization.Json.JsonSerializer))]

namespace template
{
    public class Handler
    {
    }
}