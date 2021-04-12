using System.Collections.Generic;

namespace ChatAPI.Models.DTO.SystemParam
{
    public class SystemParamListResultDTO
    {
        public List<SystemParamDTO> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Rowcount { get; set; }
    }
}
