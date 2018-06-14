using mongo.models.mongo;

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
}