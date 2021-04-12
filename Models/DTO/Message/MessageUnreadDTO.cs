using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.Message
{
    public class MessageUnreadDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
