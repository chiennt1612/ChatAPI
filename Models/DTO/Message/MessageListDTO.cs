using ChatAPI.Constant;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.Message
{
    public class MessageListDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Sender { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string[] GroupMember { get; set; }
        public string[] UserReaded { get; set; }
        public string[] UserDelete { get; set; }
        public MessageType ContentType { get; set; }
        public string ContentTypeName { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModify { get; set; }
        public DateTime? DateRecall { get; set; }
    }
}
