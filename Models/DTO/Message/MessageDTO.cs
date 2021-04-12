using ChatAPI.Constant;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.Message
{
    public class MessageDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Sender { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string[] UserReaded { get; set; }
        public string[] UserDelete { get; set; }
        public MessageType ContentType { get; set; }
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModify { get; set; }
        public DateTime? DateRecall { get; set; }
    }

    public class MessageReadDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
    }

    public class MessageReadResultDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string[] UserReaded { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }

    public class MessageDeleteResultDTO
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string[] UserDelete { get; set; }
        public string MessageId { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        //[BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }

    public class MessageRecallResultDTO
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        //[BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }

    public class MsgReadResultDTO
    {
        //[BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        //[BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public string[] UserReader { get; set; }

        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
    }

    public class MsgDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public MessageType ContentType { get; set; }
        public string Content { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }
    }

    public class MsgResultDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string GroupId { get; set; }
        public MessageType ContentType { get; set; }
        public string Content { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; }
        public bool ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
