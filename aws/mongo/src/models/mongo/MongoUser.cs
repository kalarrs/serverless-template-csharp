using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mongo.models.mongo
{
    [BsonIgnoreExtraElements]
    public class MongoUser
    {
        public ObjectId Id { get; set; }
        
        [BsonElement("email")]
        public string Email { get; set; }
        
        [BsonElement("password")]
        public string Password { get; set; }
        
        [BsonElement("passwordResetToken")]
        public string PasswordResetToken { get; set; }
        
        [BsonElement("passwordResetExpires")]
        public string PasswordResetExpires { get; set; }
        
        
        [BsonElement("facebook")]
        public string Facebook { get; set; }
        
        [BsonElement("tokens")]
        public IEnumerable<MongoAuthTokens> Tokens { get; set; }
        
        
        [BsonElement("profile")]        
        public MongoUserProfile Profile { get; set; }
        
        
        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
        
        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
    
    public class MongoAuthTokens
    {
        [BsonElement("accessToken")]
        public string AccessToken { get; set; }
        
        [BsonElement("kind")]
        public string Kind { get; set; }
    }

    public class MongoUserProfile
    {
        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("gender")]
        public string Gender { get; set; }
        
        [BsonElement("location")]
        public string location { get; set; }

        [BsonElement("website")]
        public string website { get; set; }

        [BsonElement("picture")]
        public string picture { get; set; }
    }
}