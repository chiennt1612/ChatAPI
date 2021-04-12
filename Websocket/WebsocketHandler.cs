using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ChatAPI.Constant;
using ChatAPI.Services;
using ChatAPI.Models.DTO.Groupchat;
using ChatAPI.Models.DTO.Message;
using ChatAPI.Models.DTO.SocketObject;
using MongoDB.Bson;
using ChatAPI.Helpers;

namespace ChatAPI.Websocket
{
    public class WebsocketHandler : IWebsocketHandler
    {
        public List<SocketConnectDTO> websocketConnections = new List<SocketConnectDTO>();
        private ILogger<WebsocketHandler> logger;
        private ITokensJwt jwt;
        private IGroupService _group;
        private IMessageService _Message;
        private ISystemParamService _ParamService;
        private IBlacklistService _Blacklist;
        private IReportUserService _ReportUser;

        public WebsocketHandler(ILogger<WebsocketHandler> _logger, IGroupService group, IMessageService message, ISystemParamService ParamService, IBlacklistService Blacklist, IReportUserService ReportUser, ITokensJwt _jwt)
        {
            logger = _logger;
            logger.LogInformation("Start handle");
            _group = group;
            _Message = message;
            _ParamService = ParamService;
            _Blacklist = Blacklist;
            _ReportUser = ReportUser;
            jwt = _jwt;
            SetupCleanUpTask(); // clear các connect sau 5 giây ko có action
        }

        public async Task Handle(Guid id, WebSocket webSocket)
        {
            var sc = new SocketConnectDTO
            {
                Id = id,
                WebSocket = webSocket
            };
            lock (websocketConnections)
            {
                //logger.LogInformation(String.Format ("Start connect: {0} - State: {1}", id, sc.WebSocket.State ));
                websocketConnections.Add(sc);
            }

            //logger.LogInformation(String.Format("List connection {0}", websocketConnections.ToJson ()));

            //bool Alert = false;
            while (sc.WebSocket.State == WebSocketState.Open)
            {
                try
                {
                    //logger.LogInformation(String.Format("ReceiveMessage: {0} - State: {1}", id, sc.WebSocket.State));
                    await ReceiveMessage(sc);
                }
                catch (Exception ex)
                {
                    logger.LogError(String.Format("ReceiveMessage {0} - State {1} - onerror {2}", id, sc.WebSocket.State, ex.Message ));
                    break; // disconnected most likely
                }
            }

            if (sc.WebSocket.State == WebSocketState.Closed || sc.WebSocket.State == WebSocketState.Aborted || sc.WebSocket.State == WebSocketState.None || sc.WebSocket.State == WebSocketState.CloseReceived) {
                websocketConnections.Remove(sc);
                logger.LogInformation(String.Format("Close {0} - State {1}", sc.Id, sc.WebSocket.State));
                await CloseConnect(sc);
            }
        }

        #region ReceiveMessage
        private async Task ReceiveMessage(SocketConnectDTO sc) //Guid id, WebSocket webSocket
        {
            var arraySegment = new ArraySegment<byte>(new byte[4096]);
            var receivedMessage = await sc.WebSocket.ReceiveAsync(arraySegment, CancellationToken.None);

            switch (receivedMessage.MessageType)
            {
                case WebSocketMessageType.Text:
                    var message = Encoding.Default.GetString(arraySegment).TrimEnd('\0');
                    logger.LogInformation(String.Format("Receive Message string {0} - State {1} - Message {2}", sc.Id, sc.WebSocket.State, message));
                    dynamic d;
                    try
                    {
                        d = JObject.Parse(message);
                        if (SocketTask.TryParse(d.SocketTask.ToString(), out SocketTask iTask))
                        {
                            switch (iTask)
                            {
                                case SocketTask.Login:
                                    LoginMessageDTO loginMsg = new LoginMessageDTO() { Sender = d.Sender.ToString(), Token = d.Token.ToString() };
                                    //var login = await LoginAsync(loginMsg);
                                    var login = await LoginAsync(loginMsg);
                                    logger.LogInformation(String.Format("Loging: Sender {0} - Token {1} - ResponseStatus {2} - Message {3}", login.Sender, login.Token, login.ResponseStatus, login.Message));
                                    await LoginProcess(login, sc, loginMsg);
                                    break;
                                case SocketTask.CreateGroup:
                                    GroupMessageDTO r = new GroupMessageDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        Token = d.Token.ToString(), 
                                        GroupName = d.GroupName.ToString(), 
                                        Member = d.Member.ToObject<string[]>(), 
                                        Avatar = d.Avatar 
                                    };
                                    var _grpDto = await _group.CreateGroupchat(new GroupchatDTO() { Name = r.GroupName, Member = r.Member, Avatar = r.Avatar }, r.Token, r.Sender);
                                    r.ResponseStatus = (_grpDto.ResponseStatus > 0);
                                    r.Message = _grpDto.Message;
                                    await CreateGroupProcess(r, _grpDto, sc);
                                    break;
                                case SocketTask.CreateChat:
                                    r = new GroupMessageDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        Token = d.Token.ToString(), 
                                        GroupName = d.Recieve.ToString(), 
                                        Member = new string[2] { d.Sender.ToString(), d.Recieve.ToString() }, 
                                        Avatar = 0 
                                    };
                                    _grpDto = await _group.CreateChat(new Groupchat2DTO() { Recieve = r.GroupName }, r.Token, r.Sender);
                                    r.ResponseStatus = (_grpDto.ResponseStatus > 0);
                                    r.Message = _grpDto.Message;
                                    await CreateGroupProcess(r, _grpDto, sc);
                                    break;
                                case SocketTask.JoinToGroup:
                                case SocketTask.AddToGroup:
                                    JoinGroupMessageDTO r1 = new JoinGroupMessageDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        Token = d.Token.ToString(), 
                                        Member = d.Member.ToObject<string[]>(), 
                                        GroupId = d.GroupId.ToString() 
                                    };
                                    GroupJoinResultDTO _JoinGrpDto = await _group.JoinGroupAsync(new GroupJoinDTO() { GroupId = r1.GroupId, Member = r1.Member }, r1.Token, r1.Sender);
                                    r1.ResponseStatus = (_JoinGrpDto.ResponseStatus > 0);
                                    r1.Message = _JoinGrpDto.Message;
                                    await JoinGroupProcess(r1, _JoinGrpDto, sc);
                                    break;
                                case SocketTask.RemoveFromGroup:
                                    RemoveGroupMessageDTO r2 = new RemoveGroupMessageDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        Token = d.Token.ToString(), 
                                        Username = d.Username.ToString(), 
                                        GroupId = d.GroupId.ToString() 
                                    };
                                    _JoinGrpDto = await _group.RemoveGroupAsync(new GroupRemoveDTO() { GroupId = r2.GroupId, Username = r2.Username }, r2.Token, r2.Sender);
                                    r2.ResponseStatus = (_JoinGrpDto.ResponseStatus > 0);
                                    r2.Message = _JoinGrpDto.Message;
                                    await RemoveGroupProcess(r2, _JoinGrpDto, sc);
                                    break;
                                case SocketTask.LeaveFromGroup:
                                    r2 = new RemoveGroupMessageDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        Token = d.Token.ToString(), 
                                        GroupId = d.GroupId.ToString() 
                                    };
                                    _JoinGrpDto = await _group.LeaveGroupAsync(new GroupLeaveDTO() { GroupId = r2.GroupId }, r2.Token, r2.Sender);
                                    r2.ResponseStatus = (_JoinGrpDto.ResponseStatus > 0);
                                    r2.Message = _JoinGrpDto.Message;
                                    await LeaveGroupProcess(r2, _JoinGrpDto, sc);
                                    break;
                                //case SocketTask.SearchingGroup:
                                //    break;
                                //case SocketTask.AddReportUser:
                                //    break;
                                //case SocketTask.AddBlacklistUser:
                                //    break;
                                //case SocketTask.RemoveBlacklistUser:
                                //    break;
                                case SocketTask.MessageSend:
                                    MsgDTO r3 = new MsgDTO() { 
                                        Id = d.Id.ToString(), 
                                        Sender = d.Sender.ToString(), 
                                        GroupId = d.GroupId.ToString(),
                                        Token = d.Token.ToString(),
                                        ContentType = d.ContentType, 
                                        Content = d.Content.ToString(),
                                        MessageId = ""
                                    };
                                    var r4 = await _Message.CreateAsync(r3, r3.Token, r3.Sender);
                                    if (r4.ResponseStatus) await MessageSendProcess(r4, sc);
                                    break;
                                case SocketTask.MessageEdit:
                                    r3 = new MsgDTO()
                                    {
                                        Id = d.Id.ToString(),
                                        Sender = d.Sender.ToString(),
                                        GroupId = d.GroupId.ToString(),
                                        Token = d.Token.ToString(),
                                        ContentType = d.ContentType,
                                        Content = d.Content.ToString(),
                                        MessageId = d.MessageId.ToString()
                                    };
                                    r4 = await _Message.UpdateAsync(r3, r3.Token, r3.Sender);
                                    if(r4.ResponseStatus) await MessageEditProcess(r4, r3, sc);
                                    break;
                                case SocketTask.MessageRead:
                                    var r5 = new MsgReadResultDTO() {
                                        Id = d.Id.ToString(),
                                        Sender = d.Sender.ToString(),
                                        GroupId = d.GroupId.ToString(),
                                        Token = d.Token.ToString(),
                                        MessageId = d.MessageId.ToString()
                                    };
                                    var r6 = await _Message.MesssageReadAsync(new MessageReadDTO() { Id=r5.MessageId }, r5.Token, r5.Sender);
                                    r5.UserReader = r6.UserReaded;
                                    r5.ResponseStatus = (r6.ResponseStatus > 0);
                                    r5.Message = r6.Message;
                                    if(r5.ResponseStatus)
                                        await SendMessageToSockets(new SendMessageDTO()
                                        {
                                            Id = r5.MessageId,
                                            Sender = r5.Sender,
                                            GroupId = r5.GroupId,
                                            MsgType = MessageType.Text,
                                            TaskType = SocketTask.MessageRead,
                                            Msg = r5.ToJson(),
                                            ListTag = new List<string>()
                                        });
                                    break;
                                case SocketTask.MessageRecall:
                                    MessageRecallResultDTO r7 = await _Message.MesssageRecallAsync(new MessageReadDTO() { Id = d.MessageId.ToString() }, d.Token.ToString(), d.Sender.ToString());
                                    if (r7.ResponseStatus > 0) await MesssageRecallProcess(r7, sc);
                                    break;
                                case SocketTask.MessageDelete:
                                    MessageDeleteResultDTO r8 = await _Message.MesssageDeleteAsync(new MessageReadDTO() { Id = d.MessageId.ToString() }, d.Token.ToString(), d.Sender.ToString());
                                    if (r8.ResponseStatus > 0) await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                                    {
                                        Id = r8.MessageId,
                                        Sender = r8.Sender,
                                        GroupId = r8.GroupId,
                                        MsgType = MessageType.Text,
                                        TaskType = SocketTask.MessageRecall,
                                        Msg = r8.ToJson(),
                                        ListTag = new List<string>()
                                    }).ToJson());
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(String.Format("ReceiveMessage: {0} - State: {1} - Message {2} - Exception {3}", sc.Id.ToString(), sc.WebSocket.State, message, ex.Message));
                    }
                    break;
            }
        }
        #endregion

        #region SendMessageToSockets
        private async Task SendMessageToSockets (WebSocket webSocket, string Msg)
        {
            var bytes = Encoding.Default.GetBytes(Msg);
            var arraySegment = new ArraySegment<byte>(bytes);
            if (webSocket.State == WebSocketState.Open) await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task SendMessageToSockets(SendMessageDTO sendMsg)
        {
            string msg = sendMsg.ToJson();
            logger.LogInformation(String.Format("Sending {0} - Message {1}", sendMsg.GroupId, msg));
            IEnumerable<SocketConnectDTO> toSentTo;

            lock (websocketConnections)
            {
                toSentTo = websocketConnections.Where (u => (u.GroupId.FindIndex(r => r.Equals(sendMsg.GroupId)) > -1) && (u.WebSocket.State == WebSocketState.Open) && (u.Sender != sendMsg.Sender )).ToList();
            }

            var tasks = toSentTo.Select(async websocketConnection =>
            {
                logger.LogInformation(String.Format("Sending {0} - State {1}", sendMsg.GroupId, websocketConnection.WebSocket.State));
                await SendMessageToSockets(websocketConnection.WebSocket, msg);
            });
            await Task.WhenAll(tasks);
        }

        private async Task SendMessageToSockets(string [] Member, SendMessageDTO sendMsg)
        {
            string msg = sendMsg.ToJson();
            logger.LogInformation(String.Format("Sending {0} - Message {1}", sendMsg.GroupId, msg));
            IEnumerable<SocketConnectDTO> toSentTo;

            lock (websocketConnections)
            {
                toSentTo = websocketConnections.Where(u => (Array.IndexOf(Member, u.Sender) > -1) && (u.WebSocket.State == WebSocketState.Open)).ToList();
            }

            var tasks = toSentTo.Select(async websocketConnection =>
            {
                logger.LogInformation(String.Format("Sending {0} - State {1}", sendMsg.GroupId, websocketConnection.WebSocket.State));
                await SendMessageToSockets(websocketConnection.WebSocket, msg);
            });
            await Task.WhenAll(tasks);
        }
        #endregion

        #region CloseConnect
        private void SetupCleanUpTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    IEnumerable<SocketConnectDTO> openSockets;
                    IEnumerable<SocketConnectDTO> closedSockets;

                    lock (websocketConnections)
                    {
                        openSockets = websocketConnections.Where(x => x.WebSocket.State == WebSocketState.Open || x.WebSocket.State == WebSocketState.Connecting);
                        closedSockets = websocketConnections.Where(x => x.WebSocket.State != WebSocketState.Open && x.WebSocket.State != WebSocketState.Connecting);

                        websocketConnections = openSockets.ToList();
                    }

                    foreach (var closedWebsocketConnection in closedSockets)
                    {
                        await CloseConnect(closedWebsocketConnection);
                    }

                    await Task.Delay(5000); // 5 giây dọn dẹp 1 lần các connect sleep
                }
            });
        }

        private async Task CloseConnect(SocketConnectDTO sc)
        {
            //logger.LogInformation(String.Format("Closing: {0} - State: {1}", sc.Id, sc.WebSocket.State));

            // sending to connect online
            if(sc.GroupId != null)
            {
                var tasks = sc.GroupId.Select(async r => {
                    await SendMessageToSockets(new SendMessageDTO() { Id = sc.Id.ToString(), GroupId = r, Sender = sc.Sender, ListTag = null, Msg = $"User {sc.Sender} with id <b>{sc.Id}</b> has left the groupchat {r}", MsgType = MessageType.Text });
                });
                await Task.WhenAll(tasks);
            }
            try
            {
                websocketConnections = websocketConnections.Where(a => !(a.Id.Equals(sc.Id))).ToList();
                await sc.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket is closed", CancellationToken.None);
                logger.LogInformation(String.Format("Closed {0} - State {1}", sc.Id, sc.WebSocket.State));
            }
            catch (Exception ex)
            {
                logger.LogError(String.Format("Closing {0} - State {1} - Error {2}", sc.Id, sc.WebSocket.State, ex.Message));
            }
                                 
        }
        #endregion

        #region Process
        private async Task UpdateConnect(string[] Member, string Token, string Username)
        {
            //lock (websocketConnections)
            //{
            var tasks = websocketConnections
                .Where(u => u.WebSocket.State == WebSocketState.Open || u.WebSocket.State == WebSocketState.Connecting)
                .Where(u => Array.IndexOf(Member, u.Sender) > -1)
            .Select(async t => {
                t.GroupId = (from d in (await _group.SearchGroupAsync(new GroupSearchDTO() { Id = "", Keyword = "", Page = 1, PageSize = 100 }, Token, Username)).Data
                             select d.Id)
                            .ToList();
            });
            await Task.WhenAll(tasks);
            //}            
        }
        private async Task UpdateConnect(string Id, string Token, string Username)
        {
            //lock (websocketConnections)
            //{
            var tasks = websocketConnections
                .Where(u => u.WebSocket.State == WebSocketState.Open || u.WebSocket.State == WebSocketState.Connecting)
                .Where(u => u.Id.Equals(Id))
            .Select(async t => {
                t.GroupId = (from d in (await _group.SearchGroupAsync(new GroupSearchDTO() { Id = "", Keyword = "", Page = 1, PageSize = 100 }, Token, Username)).Data
                             select d.Id)
                            .ToList();
            });
            await Task.WhenAll(tasks);
            //}            
        }
        private async Task UpdateConnect(string Token, string Username)
        {
            //lock (websocketConnections)
            //{
                var tasks = websocketConnections.Where(u => (u.WebSocket.State == WebSocketState.Open || u.WebSocket.State == WebSocketState.Connecting))
                .Select(async t => {
                    t.GroupId = (from d in (await _group.SearchGroupAsync(new GroupSearchDTO() { Id = "", Keyword = "", Page = 1, PageSize = 100 }, Token, Username)).Data
                                 select d.Id)
                                .ToList();
                });
                await Task.WhenAll(tasks);
            //}            
        }
        #region Login
        private async Task<LoginResultDTO> LoginAsync(LoginMessageDTO a)
        {
            return await jwt.ValidateIdentityServerToken(a);
        }

        private async Task LoginProcess(LoginResultDTO login, SocketConnectDTO sc, LoginMessageDTO loginMsg)
        {
            if (login.ResponseStatus)
            {
                // set lai socket object
                sc.Sender = loginMsg.Sender; sc.Token = loginMsg.Token;
                // lay danh sach nhom cho User
                if (sc.GroupId == null) sc.GroupId = new List<string>();
                var tasks = (await _group.SearchGroupAsync(new GroupSearchDTO() { Keyword = "", Page = 1, PageSize = 100 }, sc.Token, sc.Sender)).Data.Select(async a =>
                {
                    sc.GroupId.Add(a.Id);
                    await SendMessageToSockets(new SendMessageDTO()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Sender = login.Sender,
                        GroupId = a.Id,
                        MsgType = MessageType.Text,
                        TaskType = SocketTask.MessageSend,
                        Msg = $"User {sc.Sender} with id <b>{sc.Id}</b> has joined the groupchat {a.Id}",
                        ListTag = new List<string>()
                    }); ;
                });
                await Task.WhenAll(tasks);

                login.GroupId = sc.GroupId;
                login.Id = sc.Id.ToString();

                logger.LogInformation(String.Format("Object connect GroupId {0}; Token {1}; Username {2}; Guid {3}", sc.GroupId.ToJson(), sc.Token, sc.Sender, sc.Id));

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = login.Sender,
                    GroupId = "",
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.Login,
                    Msg = login.ToJson(),
                    ListTag = new List<string>()
                }).ToJson());
            }
            else
            {
                await CloseConnect(sc);
            }
        }
        #endregion

        #region Group Chat
        private async Task CreateGroupProcess(GroupMessageDTO r, GroupResultDTO _grpDto, SocketConnectDTO sc) 
        {
            if (r.ResponseStatus)
            {
                for (var i = 0; i < websocketConnections.Count; i++)
                {
                    if (Array.IndexOf(r.Member, websocketConnections[i].Sender) > -1)
                    {
                        websocketConnections[i].GroupId.Add(_grpDto.Data.Id);
                    }
                }
                r.GroupId = _grpDto.Data.Id;

                //// update group
                //await UpdateConnect(r.Member, r.Token, r.Sender);

                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.CreateGroup,
                    Msg = $"User {sc.Sender} with id <b>{sc.Id}</b> has creared the groupchat {r.GroupId}",
                    ListTag = new List<string>()
                });

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.CreateGroup,
                    Msg = $"You has creared the groupchat {r.GroupId}",
                    ListTag = new List<string>()
                }).ToJson());
            }
            else
            {
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"You has not creared the groupchat {r.GroupId}. Error: {r.Message}",
                    ListTag = new List<string>()
                }).ToJson());
            }
        }
        
        private async Task JoinGroupProcess(JoinGroupMessageDTO r, GroupJoinResultDTO _grpDto, SocketConnectDTO sc)
        {
            if (r.ResponseStatus)
            {
                for (var i = 0; i < websocketConnections.Count; i++)
                {
                    if (Array.IndexOf(r.Member, websocketConnections[i].Sender) > -1)
                    {
                        websocketConnections[i].GroupId.Add(r.GroupId);
                    }
                }
                //// update group
                //await UpdateConnect(r.Member, r.Token, r.Sender);

                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.JoinToGroup,
                    Msg = $"User {sc.Sender} with id <b>{sc.Id}</b> has added {r.Member.ToJson()} to the groupchat {r.GroupId}",
                    ListTag = new List<string>()
                });

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"List member {r.Member.ToJson()} has joined to group {r.GroupId}",
                    ListTag = new List<string>()
                }).ToJson());
            }
            else
            {
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"List member {r.Member.ToJson()} has not joined to group {r.GroupId}. Error: {r.Message}",
                    ListTag = new List<string>()
                }).ToJson());
            }
        }

        private async Task RemoveGroupProcess(RemoveGroupMessageDTO r, GroupJoinResultDTO _grpDto, SocketConnectDTO sc)
        {
            if (r.ResponseStatus)
            {
                for (var i = 0; i < websocketConnections.Count; i++)
                {
                    if (r.Username== websocketConnections[i].Sender)
                    {
                        websocketConnections[i].GroupId.Remove(r.GroupId);
                    }
                }
                //// update group
                //await UpdateConnect(r.Token, r.Sender);

                // danh sach theo nhom
                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.RemoveFromGroup,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                });

                // danh sach bi remove
                await SendMessageToSockets(_grpDto.Member, new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.RemoveFromGroup,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                });

                // owner
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"Member {r.Username} has removed from group {r.GroupId}",
                    ListTag = new List<string>()
                }).ToJson());
            }
            else
            {
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"Member {r.Username} has not removed from group {r.GroupId}. Error: {r.Message}",
                    ListTag = new List<string>()
                }).ToJson());
            }
        }

        private async Task LeaveGroupProcess(RemoveGroupMessageDTO r, GroupJoinResultDTO _grpDto, SocketConnectDTO sc)
        {
            if (r.ResponseStatus)
            {
                for (var i = 0; i < websocketConnections.Count; i++)
                {
                    if (r.Sender == websocketConnections[i].Sender)
                    {
                        websocketConnections[i].GroupId.Remove(r.GroupId);
                    }
                }
                //// update group
                //await UpdateConnect(r.Id, r.Token, r.Sender);

                // danh sách theo nhóm
                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"User {sc.Sender} with id <b>{sc.Id}</b> has leaved from the groupchat {r.GroupId}",
                    ListTag = new List<string>()
                });

                // owner
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.LeaveFromGroup,
                    Msg = $"You has leaved from group {r.GroupId}",
                    ListTag = new List<string>()
                }).ToJson());
            }
            else
            {
                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = Guid.NewGuid().ToString(),
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = $"You has not leaved from group {r.GroupId}. Error: {r.Message}",
                    ListTag = new List<string>()
                }).ToJson());
            }
        }
        #endregion

        #region Chat message
        private async Task MesssageRecallProcess(MessageRecallResultDTO r, SocketConnectDTO sc)
        {
            if (r.ResponseStatus > 0)
            {
                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageRecall,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                });

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageRecall,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                }).ToJson());
            }
        }
        private async Task MessageSendProcess(MsgResultDTO r, SocketConnectDTO sc)
        {
            if (r.ResponseStatus)
            {
                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                });

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageSend,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                }).ToJson());
            }
        }

        private async Task MessageEditProcess(MsgResultDTO r, MsgDTO _grpDto, SocketConnectDTO sc)
        {
            if (r.ResponseStatus)
            {
                await SendMessageToSockets(new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageEdit,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                });

                await SendMessageToSockets(sc.WebSocket, (new SendMessageDTO()
                {
                    Id = r.MessageId,
                    Sender = r.Sender,
                    GroupId = r.GroupId,
                    MsgType = MessageType.Text,
                    TaskType = SocketTask.MessageEdit,
                    Msg = r.ToJson(),
                    ListTag = new List<string>()
                }).ToJson());
            }
        }
        #endregion
        #endregion
    }
}
