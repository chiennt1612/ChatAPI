using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAPI.Models.DTO.SystemParam
{
    public class SystemParamResultDTO
    {
        public SystemParamDTO Data { get; set; }

        public int ResponseStatus { get; set; }
        public string Message { get; set; }
    }
}
