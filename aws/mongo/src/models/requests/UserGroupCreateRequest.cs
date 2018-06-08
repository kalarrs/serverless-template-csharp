using Newtonsoft.Json;

namespace mongo.models.requests
{
    public class UserGroupCreateRequest
    {
        [JsonProperty(Required = Required.Always)]
        public string UserId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
    }
}