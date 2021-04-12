using ChatAPI.Constant;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.ReportUser
{
    public class ReportUserDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Sender { get; set; }
        public string ReportUser { get; set; }
        public Approved Status { get; set; }
        public string Admin { get; set; }
        public DateTime DateRequest { get; set; }
        public DateTime? DateApprove { get; set; }
    }
}
