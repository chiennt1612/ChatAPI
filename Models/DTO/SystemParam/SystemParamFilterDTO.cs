using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.SystemParam
{
    public class SystemParamFilterDTO
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Code { get; set; }
        public string Value { get; set; }

        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
