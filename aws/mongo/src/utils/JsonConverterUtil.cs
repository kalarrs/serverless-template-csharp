using System;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mongo.utils
{
    public static class JsonConverterUtil
    {
        public class ObjectIdConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                var str = token.ToObject<string>();
                if (Nullable.GetUnderlyingType(objectType) != null && str == null) return null;
                return new ObjectId(str);
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(ObjectId).IsAssignableFrom(objectType);
            }
        }
    }
}