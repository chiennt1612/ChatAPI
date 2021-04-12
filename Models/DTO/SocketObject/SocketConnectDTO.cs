using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.Serialization;

namespace ChatAPI.Models.DTO.SocketObject
{
    public class SocketConnectDTO
    {
        public Guid Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        public List<string> GroupId { get; set; }

        [NonSerialized]
        public WebSocket WebSocket;
    }
}
