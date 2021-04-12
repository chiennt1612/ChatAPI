using System.Collections.Generic;

namespace ChatAPI.Models.DTO.ReportUser
{
    public class ReportUserListResultDTO
    {
        public List<ReportUserDTO> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Rowcount { get; set; }
    }
}
