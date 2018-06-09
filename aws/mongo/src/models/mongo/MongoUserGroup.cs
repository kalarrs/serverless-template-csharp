using System;
using System.Collections.Generic;
using mongo.models.requests;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mongo.models.mongo
{
    [BsonIgnoreExtraElements]
    public class MongoUserGroup
    {
        public MongoUserGroup(UserGroupCreateRequest userGroup, MongoUser mongoUser)
        {
            Name = userGroup.Name;
            Users = new List<MongoUserGroupUser>() {new MongoUserGroupUser(mongoUser)};

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public ObjectId Id { get; set; }
        [BsonElement("name")] public string Name { get; set; }
        [BsonElement("users")] public List<MongoUserGroupUser> Users { get; set; }

        [BsonElement("createdAt")] public DateTime? CreatedAt { get; set; }
        [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoUserGroupUser
    {
        public MongoUserGroupUser(MongoUser mongoUser)
        {
            Id = mongoUser.Id;
            Type = UserGroupUserType.Owner;

            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public ObjectId Id { get; set; }
        [BsonElement("type")][BsonRepresentation(BsonType.String)] public UserGroupUserType Type { get; set; }
        
        [BsonElement("score")] public int? Score { get; set; }
        [BsonElement("rank")] public int? Rank { get; set; }

        [BsonElement("createdAt")] public DateTime? CreatedAt { get; set; }
        [BsonElement("updatedAt")] public DateTime? UpdatedAt { get; set; }
    }
}