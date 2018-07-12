using System.Collections.Generic;
using mongo.models.mongo;
using MongoDB.Bson;

namespace mongo.models
{
    public class UserProfile
    {
        public UserProfile(MongoUserProfile userProfile)
        {
            Name = userProfile.Name;
            Gender = userProfile.Gender;
            Location = userProfile.Location;
            Website = userProfile.Website;
            Picture = userProfile.Picture;
        }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public string Picture { get; set; }
    }
    
    public class UsersProjection
    {
        public ObjectId Id { get; set; } 
        public IEnumerable<ObjectId?> UserId { get; set; } 
    }
        
    public class UsersUnwind
    {
        public ObjectId Id { get; set; } 
        public ObjectId? UserId { get; set; } 
    }

    public class UserToUserGroups
    {
        public ObjectId Id { get; set; } 
        public IEnumerable<ObjectId> UserGroups { get; set; } 
    }
}