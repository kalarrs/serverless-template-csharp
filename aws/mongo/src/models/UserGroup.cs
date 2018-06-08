using System;
using System.Collections.Generic;
using System.Linq;
using mongo.models.mongo;
using MongoDB.Bson;

namespace mongo.models
{
    public enum UserGroupUserType
    {
        Owner,
        Member
    }

    public class UserGroup
    {
        public UserGroup(MongoUserGroup mongoUserGroup)
        {
            Id = mongoUserGroup.Id;
            Name = mongoUserGroup.Name;
            Users = mongoUserGroup.Users?.Select(u => new UserGroupUser(u));

            CreatedAt = mongoUserGroup.CreatedAt;
            UpdatedAt = mongoUserGroup.UpdatedAt;
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<UserGroupUser> Users { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserGroupUser
    {
        public UserGroupUser(MongoUserGroupUser mongoUserGroupUser)
        {
            Id = mongoUserGroupUser.Id;
            Type = mongoUserGroupUser.Type;
            CreatedAt = mongoUserGroupUser.CreatedAt;
            UpdatedAt = mongoUserGroupUser.UpdatedAt;
        }

        public ObjectId Id { get; set; }
        public UserGroupUserType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}