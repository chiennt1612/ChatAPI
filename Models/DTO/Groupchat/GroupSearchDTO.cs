using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.Groupchat
{
    public class GroupSearchDTO
    {
        public string Id { get; set; }
        public string Keyword { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
