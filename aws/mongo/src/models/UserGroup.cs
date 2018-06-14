using System;
using System.Collections.Generic;
using System.Linq;
using mongo.models.mongo;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace mongo.models
{
    public enum UserGroupUserType
    {
        Owner,
        Member
    }

    public class UserGroup
    {
        public UserGroup(MongoUserGroup mongoUserGroup, IReadOnlyDictionary<ObjectId, MongoUserGroupMemberUser> userDictionary)
        {
            Id = mongoUserGroup.Id;
            Name = mongoUserGroup.Name;
            Members = mongoUserGroup.Members?.Select(u => new UserGroupMember(u, userDictionary));

            CreatedAt = mongoUserGroup.CreatedAt;
            UpdatedAt = mongoUserGroup.UpdatedAt;
        }

        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<UserGroupMember> Members { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserGroupMember
    {
        public UserGroupMember(MongoUserGroupMember mongoUserGroupUser, IReadOnlyDictionary<ObjectId, MongoUserGroupMemberUser> userDictionary)
        {
            Id = mongoUserGroupUser.Id;
            Type = mongoUserGroupUser.Type;
            if (mongoUserGroupUser.User != null && userDictionary.ContainsKey(mongoUserGroupUser.User.Value))
                User = new UserGroupMemberUser(userDictionary[mongoUserGroupUser.User.Value]);
            CreatedAt = mongoUserGroupUser.CreatedAt;
            UpdatedAt = mongoUserGroupUser.UpdatedAt;
        }

        public ObjectId Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public UserGroupUserType? Type { get; set; }

        public UserGroupMemberUser User { get; set; }

        public int? Score { get; set; }
        public int? Rank { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UserGroupMemberUser
    {
        public UserGroupMemberUser(MongoUserGroupMemberUser user)
        {
            Id = user.Id;
            Email = user.Email;
            Profile = user.Profile != null ? new UserProfile(user.Profile) : null;
        }

        public ObjectId Id { get; set; }
        public string Email { get; set; }
        public UserProfile Profile { get; set; }
    }
}