using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.Groupchat
{
    public class GroupResultDTO
    {
        public GroupDTO Data { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
