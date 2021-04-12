using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.Blacklist
{
    public class BlacklistResultDTO
    {
        public BlacklistDTO Data { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
