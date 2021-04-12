using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.Groupchat
{
    public class GroupchatDTO
    {
        public string Name { get; set; }
        public string[] Member { get; set; }
        public long Avatar { get; set; }
    }

    public class Groupchat2DTO
    {
        public string Recieve { get; set; }
    }

    public class GroupJoinDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string[] Member { get; set; }
    }

    public class GroupRemoveDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string Username { get; set; }
    }

    public class GroupLeaveDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
    }

    public class GroupJoinResultDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string[] Member { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
