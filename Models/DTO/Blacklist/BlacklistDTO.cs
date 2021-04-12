using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.Blacklist
{
    public class BlacklistDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Owner { get; set; }
        public string Username { get; set; }
        public string Groupname { get; set; }
    }
}
