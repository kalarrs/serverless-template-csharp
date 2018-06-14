using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using mongo.models.requests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mongo.models.mongo
{
    [BsonIgnoreExtraElements]
    public class MongoUserGroup
    {
        public MongoUserGroup(UserGroupCreateRequest userGroup, MongoUserGroupMemberUser mongoUserGroupMemberUser)
        {
            Name = userGroup.Name;
            Members = new List<MongoUserGroupMember>() {new MongoUserGroupMember(mongoUserGroupMemberUser)};

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public ObjectId Id { get; set; }
        [BsonElement("name")] public string Name { get; set; }
        [BsonElement("users")] public List<MongoUserGroupMember> Members { get; set; }

        [BsonElement("createdAt")] public DateTime? CreatedAt { get; set; }
        [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoUserGroupMember
    {
        public MongoUserGroupMember(MongoUserGroupMemberUser mongoUserGroupMemberUser)
        {
            Id = ObjectId.GenerateNewId();
            Type = UserGroupUserType.Owner;
            User = mongoUserGroupMemberUser.Id;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public ObjectId Id { get; set; }
        [BsonElement("type")][BsonRepresentation(BsonType.String)] public UserGroupUserType Type { get; set; }
        
        [BsonElement("user")] public ObjectId? User { get; set; }
        
        [BsonElement("score")] public int? Score { get; set; }
        [BsonElement("rank")] public int? Rank { get; set; }

        [BsonElement("createdAt")] public DateTime? CreatedAt { get; set; }
        [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoUserGroupMemberUser
    {
        public ObjectId Id { get; set; }
        [BsonElement("email")] public string Email { get; set; }
        [BsonElement("profile")] public MongoUserProfile Profile { get; set; }

        public static readonly Expression<Func<MongoUser, MongoUserGroupMemberUser>> UserProjection = u =>
            new MongoUserGroupMemberUser()
            {
                Id = u.Id,
                Email = u.Email,
                Profile = u.Profile
            };
    }
}