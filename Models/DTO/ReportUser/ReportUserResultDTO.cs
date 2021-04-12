using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ChatAPI.Models.DTO.ReportUser
{
    public class ReportUserResultDTO
    {
        public ReportUserDTO Data { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
