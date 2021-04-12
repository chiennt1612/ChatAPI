using ChatAPI.Constant;
using System;
using System.Collections.Generic;

namespace ChatAPI.Models.DTO.SocketObject
{
    #region Sending MSG
    public class SendMessageDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string GroupId { get; set; }
        public MessageType MsgType { get; set; }// Text = 1,
        public SocketTask TaskType { get; set; }
        public string Msg { get; set; }
        public List<string> ListTag { get; set; }
    }
#endregion

    #region Login
    public class LoginMessageDTO
    {
        public string Sender { get; set; }
        public string Token { get; set; }
    }

    public class LoginResultDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        public List<string> GroupId { get; set; }


        public string Message { get; set; }
        public bool ResponseStatus { get; set; }
    }
    #endregion

    #region Create Group
    public class GroupMessageDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }

        public string GroupName { get; set; }
        public string[] Member { get; set; }
        public long Avatar { get; set; }

        public string GroupId { get; set; }
        public string Message { get; set; }
        public bool ResponseStatus { get; set; }
    }

    public class JoinGroupMessageDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        public string[] Member { get; set; }
        public string GroupId { get; set; }

        public string Message { get; set; }
        public bool ResponseStatus { get; set; }
    }

    public class RemoveGroupMessageDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public string GroupId { get; set; }

        public string Message { get; set; }
        public bool ResponseStatus { get; set; }
    }

    public class LeaveGroupMessageDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Token { get; set; }
        public string GroupId { get; set; }

        public string Message { get; set; }
        public bool ResponseStatus { get; set; }
    }
    #endregion
}
