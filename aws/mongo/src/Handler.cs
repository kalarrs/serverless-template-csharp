using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.ScheduledEvents;
using mongo.models;
using mongo.models.mongo;
using mongo.models.requests;
using mongo.utils;
using MongoDB.Bson;
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
        private static readonly ConfiguredTaskAwaitable<string> NameIndex = UserGroupsCollection.Indexes.CreateOneAsync(new IndexKeysDefinitionBuilder<MongoUserGroup>().Descending(ug => ug.Name), new CreateIndexOptions() {Unique = true}).ConfigureAwait(false); 
        
        public static async Task<APIGatewayProxyResponse> GetUserGroups(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var userGroups = await UserGroupsCollection.AsQueryable().ToListAsync().ConfigureAwait(false);
                var userGroupsWithMembers = userGroups.Where(ug => ug.Members != null && ug.Members.Count > 0);
                var userIds = userGroupsWithMembers.SelectMany(g =>
                    g.Members.Where(m => m.User.HasValue).Select(vm => vm.User.Value));
                var users = await UsersCollection.AsQueryable().Select(MongoUserGroupMemberUser.UserProjection)
                    .Where(u => userIds.Contains(u.Id)).ToListAsync().ConfigureAwait(false);
                ;
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
            catch (Exception e)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int) HttpStatusCode.InternalServerError,
                    Body = null,
                    Headers = ApiGatewayUtil.DefaultHeaders
                };
            }
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
        
        public static async Task ReferentialIntegrityUserGroups(ScheduledEvent @event, ILambdaContext context)
        {
            
            Console.WriteLine("Event Object:");
            Console.WriteLine(JsonConvert.SerializeObject(@event));
            
            var userToUserGroupsBson = await UserGroupsCollection.Aggregate()
                .Match(u => u.Members.Count > 0)
                .Project(u => new UsersProjection() {Id = u.Id, UserId = u.Members.Select(m => m.User)})
                .Unwind<UsersProjection, UsersUnwind>(u => u.UserId)
                .Group(i => i.UserId, a => new BsonDocument{{"_id", "$UserId"}, {"userGroupIds", new BsonDocument {{"$addToSet", "$Id"}}}})
                .ToListAsync();

            var userToUserGroups = userToUserGroupsBson.Select(b => new UserToUserGroups() {
                Id = b["_id"].AsObjectId,
                UserGroups = b["userGroupIds"].AsBsonArray.Select(ugi => ugi.AsObjectId)
            }).ToList();

            var userIds = userToUserGroups.Select(u => u.Id);
            var users = await UsersCollection.AsQueryable().Select(u => u.Id).Where(uId => userIds.Contains(uId)).ToListAsync().ConfigureAwait(false);
            var removedUserIds = userIds.Except(users).ToList();
            if (removedUserIds.Count > 0)
            {
                var models = new List<WriteModel<MongoUserGroup>>();
                var userToUserGroupDict = userToUserGroups.ToDictionary(u => u.Id);
                foreach (var removedUserId in removedUserIds)
                {
                    var userGroups = userToUserGroupDict[removedUserId].UserGroups;
                    var filter = Builders<MongoUserGroup>.Filter.Where(m => userGroups.Contains(m.Id));
                    var update = Builders<MongoUserGroup>.Update.PullFilter(ug => ug.Members, m => m.User == removedUserId);
                    models.Add(new UpdateManyModel<MongoUserGroup>(filter, update));
                }

                var bulk = await UserGroupsCollection.BulkWriteAsync(models);
                Console.WriteLine(JsonConvert.SerializeObject(bulk));
            }
            
            Console.WriteLine("Context Object:");
            Console.WriteLine(JsonConvert.SerializeObject(context));
        }

        public class Input
        {
            public string Key1 { get; set; }
            public string Key2 { get; set; }
            public InputStageParams StageParams { get; set; }
        }

        public class InputStageParams
        {
            public string Stage { get; set; }
        }

        public static object Schedule(Input input, ILambdaContext context)
        {
            Console.WriteLine("Event Object:");
            Console.WriteLine(JsonConvert.SerializeObject(input));
            return input;
        }
    }
}