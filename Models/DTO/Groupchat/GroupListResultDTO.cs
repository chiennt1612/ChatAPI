using System.Collections.Generic;

namespace ChatAPI.Models.DTO.Groupchat
{
    public class GroupListResultDTO
    {
        public List<GroupDTO> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Rowcount { get; set; }
    }
}
