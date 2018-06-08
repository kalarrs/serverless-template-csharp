using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using mongo.models;
using mongo.models.mongo;
using mongo.models.requests;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace mongo
{
    public class Handler
    {
        private static readonly MongoClient Client = new MongoClient(Environment.GetEnvironmentVariable("MONGODB_URI"));
        private static readonly IMongoDatabase Database = Client.GetDatabase("kalarrs");
        private static readonly IMongoCollection<MongoUserGroup> UserGroupsCollection = Database.GetCollection<MongoUserGroup>("userGroups");
        private static readonly IMongoCollection<MongoUser> UsersCollection = Database.GetCollection<MongoUser>("users");
        
        /// <summary>
        /// A Lambda function to respond to HTTP Get /api/user-groups
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of userGroups</returns>
        public static async Task<APIGatewayProxyResponse> GetUserGroups(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userGroups = await UserGroupsCollection.AsQueryable().ToListAsync().ConfigureAwait(false);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new ApiResponse<IEnumerable<UserGroup>>
                {
                    Data = userGroups?.Select(u => new UserGroup(u))
                }, Util.DefaultSerializerSettings),
                Headers = Util.DefaultHeaders
            };
        }

        public static async Task<APIGatewayProxyResponse> PostUserGroups(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<UserGroupCreateRequest>(request.Body);

                var user = await UsersCollection.AsQueryable().FirstOrDefaultAsync(u => u.Id == ObjectId.Parse(body.UserId));
                if (user == null) throw new Exception("User Not Found");
                
                /*
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
                */

                var mongoUserGroup = new MongoUserGroup(body, user);
                await UserGroupsCollection.InsertOneAsync(mongoUserGroup);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.Created,
                    Body = JsonConvert.SerializeObject(new ApiResponse<UserGroup>
                    {
                        Data = new UserGroup(mongoUserGroup),
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
        /*
        public APIGatewayProxyResponse PutChange(APIGatewayProxyRequest request, ILambdaContext context)
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

        public APIGatewayProxyResponse DeleteChange(APIGatewayProxyRequest request, ILambdaContext context)
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
        */
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

    

    public class SnakeCaseEample
    {
        [JsonProperty("first_name")] public string FirstName { get; set; }

        [JsonProperty("last_name")] public string LastName { get; set; }
    }
}