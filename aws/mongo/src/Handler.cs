using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using mongo.models;
using mongo.models.mongo;
using mongo.models.requests;
using mongo.utils;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;

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
        private static readonly ConfiguredTaskAwaitable<string> NameIndex = UserGroupsCollection.Indexes
            .CreateOneAsync(new IndexKeysDefinitionBuilder<MongoUserGroup>().Descending(ug => ug.Name), new CreateIndexOptions() {Unique = true}).ConfigureAwait(false); 
        
        /// <summary>
        /// A Lambda function to respond to HTTP Get /api/user-groups
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A list of userGroups</returns>
        public static async Task<APIGatewayProxyResponse> GetUserGroups(APIGatewayProxyRequest request, ILambdaContext context)
        {
            var userGroups = await UserGroupsCollection.AsQueryable().ToListAsync().ConfigureAwait(false);
            var userGroupsWithMembers = userGroups.Where(ug => ug.Members != null && ug.Members.Count > 0);
            var userIds = userGroupsWithMembers.SelectMany(g => g.Members.Where(m => m.User.HasValue).Select(vm => vm.User.Value));
            var users = await UsersCollection.AsQueryable().Select(MongoUserGroupMemberUser.UserProjection).Where(u => userIds.Contains(u.Id)).ToListAsync().ConfigureAwait(false);;
            var userDictionary = users.ToDictionary(u => u.Id);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int) HttpStatusCode.OK,
                Body = JsonConvert.SerializeObject(new ApiGatewayUtil.ApiResponse<IEnumerable<UserGroup>>
                {
                    Data = userGroups?.Select(u => new UserGroup(u, userDictionary))
                }, ApiGatewayUtil.DefaultSerializerSettings),
                Headers = ApiGatewayUtil.DefaultHeaders
            };
        }

        public static async Task<APIGatewayProxyResponse> PostUserGroups(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<UserGroupCreateRequest>(request.Body);

                var user = await UsersCollection.AsQueryable().Select(MongoUserGroupMemberUser.UserProjection).FirstOrDefaultAsync(u => u.Id == body.UserId).ConfigureAwait(false);
                if (user == null) throw new Exception("User Not Found");
                // TODO : Check for conflict! IE User is already the owner of a UserGroup! Also check if the group name is already in use. IE make name unique.
                
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
                        }, ApiGatewayUtil.DefaultSerializerSettings),
                        Headers = ApiGatewayUtil.DefaultHeaders
                    };
                */

                var mongoUserGroup = new MongoUserGroup(body, user);
                await UserGroupsCollection.InsertOneAsync(mongoUserGroup).ConfigureAwait(false);
                var userDictionary = new[] {user}.ToDictionary(u => u.Id);

                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.Created,
                    Body = JsonConvert.SerializeObject(new ApiGatewayUtil.ApiResponse<UserGroup>
                    {
                        Data = new UserGroup(mongoUserGroup, userDictionary),
                    }, ApiGatewayUtil.DefaultSerializerSettings),
                    Headers = ApiGatewayUtil.DefaultHeaders
                };
            }
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = e is JsonSerializationException
                        ? (int) HttpStatusCode.BadRequest
                        : (int) HttpStatusCode.InternalServerError,

                    Body = JsonConvert.SerializeObject(new ApiGatewayUtil.ApiResponse<ApiGatewayUtil.ApiError>
                    {
                        Data = new ApiGatewayUtil.ApiError
                        {
                            Type = ApiGatewayUtil.ApiError.ErrorType.ApiError,
                            Message = e.Message
                        }
                    }, ApiGatewayUtil.DefaultSerializerSettings),
                    Headers = ApiGatewayUtil.DefaultHeaders
                };
            }
        }
    }
}