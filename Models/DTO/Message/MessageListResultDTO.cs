using System.Collections.Generic;

namespace ChatAPI.Models.DTO.Message
{
    public class MessageListResultDTO
    {
        public List<MessageListDTO> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Rowcount { get; set; }
    }
}
