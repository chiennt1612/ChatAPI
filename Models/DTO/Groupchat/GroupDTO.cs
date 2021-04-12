using ChatAPI.Constant;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.Groupchat
{
    public class GroupDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Owner { get; set; }
        public string Name { get; set; }
        public string[] Member { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? LastMessage { get; set; }
        public long Avatar { get; set; }

        public GroupType Type { get; set; }
        public PublishType IsPublish { get; set; }
    }

    public class ObjMember {
        public string Username { get; set; }
        public bool Islogin { get; set; }
    }
}
