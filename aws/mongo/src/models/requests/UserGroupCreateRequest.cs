using mongo.utils;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace mongo.models.requests
{
    public class UserGroupCreateRequest
    {
        [JsonRequired]
        [JsonConverter(typeof(JsonConverterUtil.ObjectIdConverter))]
        public ObjectId UserId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
    }
}