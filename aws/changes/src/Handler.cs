using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using MongoDB.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace changes
{
    public class Handler
    {
        private static readonly List<Change> Changes = new List<Change>
        {
            new Change
            {
                Id = new ObjectId("5a1b5ae36758c40453e5e024"),
                Description = "This is an example"
            },
            new Change
            {
                Id = new ObjectId("5a1b5b176758c40453e5e025"),
                Description = "of a simple mock API"
            }
        };


        /// <summary>
        /// A Lambda function to respond to HTTP GET /csharp/changes
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The list of changes</returns>
        public static APIGatewayProxyResponse GetChanges(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Get Changes\n");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new ApiResponse<List<Change>>
                {
                    Data = Changes
                }, Util.DefaultSerializerSettings),
                Headers = Util.DefaultHeaders
            };
        }

        /// <summary>
        /// A Lambda function to respond to HTTP POST /csharp/changes
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A new change</returns>
        public static APIGatewayProxyResponse PostChanges(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Post Changes\n");

            try
            {
                var body = JsonConvert.DeserializeObject<PostChangeRequest>(request.Body);
                if (body.Id == null) throw new JsonSerializationException("Id is required");

                if (Changes.Any(c => c.Id == ObjectId.Parse(body.Id)))
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int) HttpStatusCode.Conflict,
                        Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                        {
                            Data = new ApiError
                            {
                                Type = ApiError.ErrorType.ApiError,
                                Message = "A change with that Id already exits."
                            }
                        }, Util.DefaultSerializerSettings),
                        Headers = Util.DefaultHeaders
                    };
                }

                var change = new Change() {Id = ObjectId.Parse(body.Id), Description = body.Description};

                Changes.Add(change);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.Created,
                    Body = JsonConvert.SerializeObject(new ApiResponse<Change>
                    {
                        Data = change,
                    }, Util.DefaultSerializerSettings),
                    Headers = Util.DefaultHeaders
                };
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = e is JsonSerializationException
                        ? (int) HttpStatusCode.BadRequest
                        : (int) HttpStatusCode.InternalServerError,

                    Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                    {
                        Data = new ApiError
                        {
                            Type = ApiError.ErrorType.ApiError,
                            Message = e.Message
                        }
                    }, Util.DefaultSerializerSettings),
                    Headers = Util.DefaultHeaders
                };
            }
        }

        /// <summary>
        /// A Lambda function to respond to HTTP PUT /csharp/changes/{changeId}
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An updated change</returns>
        public static APIGatewayProxyResponse PutChange(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Put Change\n");

            try
            {
                var body = JsonConvert.DeserializeObject<PostChangeRequest>(request.Body);
                if (body.Id == null) throw new JsonSerializationException("Id is required");

                var change = Changes.FirstOrDefault(c => c.Id == ObjectId.Parse(body.Id));
                if (change == null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int) HttpStatusCode.NotFound,
                        Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                        {
                            Data = new ApiError
                            {
                                Type = ApiError.ErrorType.ApiError,
                                Message = "Not Found"
                            }
                        }, Util.DefaultSerializerSettings),
                        Headers = Util.DefaultHeaders
                    };
                }

                change.Description = body.Description;

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(new ApiResponse<Change>
                    {
                        Data = change
                    }, Util.DefaultSerializerSettings),
                    Headers = Util.DefaultHeaders
                };
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = e is JsonSerializationException
                        ? (int) HttpStatusCode.BadRequest
                        : (int) HttpStatusCode.InternalServerError,

                    Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                    {
                        Data = new ApiError
                        {
                            Type = ApiError.ErrorType.ApiError,
                            Message = e.Message
                        }
                    }, Util.DefaultSerializerSettings),
                    Headers = Util.DefaultHeaders
                };
            }
        }

        /// <summary>
        /// A Lambda function to respond to HTTP DELETE /csharp/changes/{changeId}
        /// </summary>
        /// <param name="request"></param>
        /// <returns>An updated change</returns>
        public static APIGatewayProxyResponse DeleteChange(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Delete Change\n");

            try
            {
                if (!request.PathParameters.ContainsKey("changeId"))
                    throw new JsonSerializationException("Id is required");

                var change = Changes.FirstOrDefault(c => c.Id == ObjectId.Parse(request.PathParameters["changeId"]));
                if (change == null)
                {
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = (int) HttpStatusCode.NotFound,
                        Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                        {
                            Data = new ApiError
                            {
                                Type = ApiError.ErrorType.ApiError,
                                Message = "Not Found"
                            }
                        }, Util.DefaultSerializerSettings),
                        Headers = Util.DefaultHeaders
                    };
                }

                Changes.Remove(change);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.Accepted,
                    Headers = Util.DefaultHeaders
                };
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = e is JsonSerializationException
                        ? (int) HttpStatusCode.BadRequest
                        : (int) HttpStatusCode.InternalServerError,

                    Body = JsonConvert.SerializeObject(new ApiResponse<ApiError>
                    {
                        Data = new ApiError
                        {
                            Type = ApiError.ErrorType.ApiError,
                            Message = e.Message
                        }
                    }, Util.DefaultSerializerSettings),
                    Headers = Util.DefaultHeaders
                };
            }
        }
    }

    public static class Util
    {
        public static readonly JsonSerializerSettings DefaultSerializerSettings =
            new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()};

        public static readonly Dictionary<string, string> DefaultHeaders =
            new Dictionary<string, string>
            {
                {"Content-Type", "application/json"},
                {"Access-Control-Allow-Origin", "*"}, // Required for CORS support to work
                {"Access-Control-Allow-Credentials", "true"},
            };

        public class ObjectIdConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(ObjectId).IsAssignableFrom(objectType);
            }
        }
    }

    public class ApiResponse<T>
    {
        public T Data { get; set; }
    }


    public class ApiError
    {
        public enum ErrorType
        {
            ApiError,
            Other
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public ErrorType Type { get; set; }

        public string Message { get; set; }
    }

    public class Change
    {
        public ObjectId Id { get; set; }
        public string Description { get; set; }
    }

    public class PostChangeRequest
    {
        //[JsonProperty(Required = Required.Always)]
        //[JsonConverter(typeof(Util.ObjectIdConverter))]
        public string Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; }
    }

    public class SnakeCaseEample
    {
        [JsonProperty("first_name")] public string FirstName { get; set; }

        [JsonProperty("last_name")] public string LastName { get; set; }
    }
}