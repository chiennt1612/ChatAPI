using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.Message
{
    public class MessageResultDTO
    {
        public MessageDTO Data { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
