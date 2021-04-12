using System.Collections.Generic;

namespace ChatAPI.Models.DTO.Blacklist
{
    public class BlacklistListResultDTO
    {
        public List<BlacklistDTO> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Rowcount { get; set; }
    }
}
