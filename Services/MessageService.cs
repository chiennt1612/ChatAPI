using ChatAPI.Models;
using ChatAPI.Models.DTO.Message;
using ChatAPI.Models.DTO.Groupchat;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using System.Net.Mime;
using ChatAPI.Constant;

namespace ChatAPI.Services
{
    public interface IMessageService
    {
        #region CMS-API
        Task<MessageListResultDTO> GetAsync(int Page, int PageSize);
        Task<MessageListResultDTO> GetAsync(MessageFilterDTO _filter);
        Task<MessageListDTO> GetAsync(string id);
        Task<MessageResultDTO> CreateAsync(MessageDTO book);
        Task<MessageResultDTO> UpdateAsync(string id, MessageDTO bookIn);
        Task<MessageResultDTO> RemoveAsync(MessageDTO bookIn);
        Task<MessageResultDTO> RemoveAsync(string id);
        #endregion

        #region APP-API
        Task<MsgResultDTO> CreateAsync(MsgDTO bookIn, string Token, string Username);
        Task<MsgResultDTO> UpdateAsync(MsgDTO bookIn, string Token, string Username);
        Task<MessageReadResultDTO> MesssageReadAsync(MessageReadDTO _filter, string Token, string Username);
        Task<MessageDeleteResultDTO> MesssageDeleteAsync(MessageReadDTO _filter, string Token, string Username);
        Task<MessageRecallResultDTO> MesssageRecallAsync(MessageReadDTO _filter, string Token, string Username);
        Task<MessageListResultDTO> SearchMesssageAsync(MessageSearchDTO _filter, string Token, string Username);
        Task<MessageListResultDTO> UnreadMesssageAsync(MessageUnreadDTO _filter, string Token, string Username);
        #endregion
    }
    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<MessageDTO> _Message;
        private readonly IMongoCollection<GroupDTO> _Group;
        private readonly ILogger<MessageService> _Log;
        public MessageService(IDBSetting settings, ILogger<MessageService> Log)
        {
            _Log = Log;
            _Log.LogInformation("Start object Message!");
            var client = new MongoClient(settings.CHAT_MONGODB_URL);
            var database = client.GetDatabase(settings.DatabaseName);

            _Message = database.GetCollection<MessageDTO>("message");
            _Group = database.GetCollection<GroupDTO>("group");
        }
        
        #region CMS-API
        public async Task<MessageListResultDTO> GetAsync(int Page, int PageSize)
        {
            List<MessageListDTO> a;
            IMongoQueryable<MessageListDTO> query = from m in _Message.AsQueryable()
                                                    join g in _Group.AsQueryable() on m.GroupId equals g.Id
                                                    select new MessageListDTO()
                                                    {
                                                        Id = m.Id,
                                                        Sender = m.Sender,
                                                        GroupId = m.GroupId,
                                                        GroupName = g.Name,
                                                        GroupMember = g.Member,
                                                        UserReaded = m.UserReaded,
                                                        UserDelete = m.UserDelete,
                                                        ContentType = m.ContentType,
                                                        ContentTypeName = (m.ContentType == MessageType.Text ? "{MsgText}" : (m.ContentType == MessageType.Image ? "{MsgImage}" : (m.ContentType == MessageType.Video ? "{MsgVideo}" : (m.ContentType == MessageType.Streaming ? "{MsgStreaming}" : (m.ContentType == MessageType.Excel ? "{MsgExcel}" : (m.ContentType == MessageType.Word ? "{MsgWord}" : (m.ContentType == MessageType.Pdf ? "{MsgPdf}" : "{MsgFile}"))))))),
                                                        Content = m.Content,
                                                        DateCreated = m.DateCreated,
                                                        DateModify = m.DateModify,
                                                        DateRecall = m.DateRecall
                                                    };
            a = (await query.ToListAsync());
            int Rowcount = a.Count;
            return new MessageListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateCreated)
                    .Skip(PageSize * (Page - 1))
                    .Take(PageSize)
                    .ToList(),
                Page = Page,
                PageSize = PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<MessageListResultDTO> GetAsync(MessageFilterDTO _filter)
        {
            List<MessageListDTO> a;
            IMongoQueryable<MessageListDTO> query = from m in _Message.AsQueryable()
                                                    join g in _Group.AsQueryable() on m.GroupId equals g.Id
                                                    select new MessageListDTO()
                                                    {
                                                        Id = m.Id,
                                                        Sender = m.Sender,
                                                        GroupId = m.GroupId,
                                                        GroupName = g.Name,
                                                        GroupMember = g.Member,
                                                        UserReaded = m.UserReaded,
                                                        UserDelete = m.UserDelete,
                                                        ContentType = m.ContentType,
                                                        ContentTypeName = (m.ContentType == MessageType.Text ? "{MsgText}" : (m.ContentType == MessageType.Image ? "{MsgImage}" : (m.ContentType == MessageType.Video ? "{MsgVideo}" : (m.ContentType == MessageType.Streaming ? "{MsgStreaming}" : (m.ContentType == MessageType.Excel ? "{MsgExcel}" : (m.ContentType == MessageType.Word ? "{MsgWord}" : (m.ContentType == MessageType.Pdf ? "{MsgPdf}" : "{MsgFile}"))))))),
                                                        Content = m.Content,
                                                        DateCreated = m.DateCreated,
                                                        DateModify = m.DateModify,
                                                        DateRecall = m.DateRecall
                                                    };

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                query = query.Where(u => u.Id.Equals(_filter.Id));
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Sender))
                {
                    query = query.Where(u => u.Sender.Contains(_filter.Sender));
                }

                if (!String.IsNullOrEmpty(_filter.GroupId))
                {
                    query = query.Where(u => u.GroupId.Equals(_filter.GroupId));
                }

                if (!String.IsNullOrEmpty(_filter.GroupName))
                {
                    query = query.Where(u => u.GroupName.Contains(_filter.GroupName));
                }

                if (_filter.ContentType > 0)
                {
                    query = query.Where(u => u.ContentType.Equals(_filter.ContentType));
                }

                if (!String.IsNullOrEmpty(_filter.ContentTypeName))
                {
                    query = query.Where(u => u.ContentTypeName.Contains(_filter.ContentTypeName));
                }

                if (!String.IsNullOrEmpty(_filter.Content))
                {
                    query = query.Where(u => u.Content.Contains(_filter.Content));
                }
            }

            a = (await query.ToListAsync());
            int Rowcount = a.Count;
            return new MessageListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateCreated)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<MessageListDTO> GetAsync(string id)
        {
            List<MessageListDTO> a;
            IMongoQueryable<MessageListDTO> query = from m in _Message.AsQueryable()
                                                    join g in _Group.AsQueryable() on m.GroupId equals g.Id
                                                    select new MessageListDTO()
                                                    {
                                                        Id = m.Id,
                                                        Sender = m.Sender,
                                                        GroupId = m.GroupId,
                                                        GroupName = g.Name,
                                                        GroupMember = g.Member,
                                                        UserReaded = m.UserReaded,
                                                        UserDelete = m.UserDelete,
                                                        ContentType = m.ContentType,
                                                        ContentTypeName = (m.ContentType == MessageType.Text ? "{MsgText}" : (m.ContentType == MessageType.Image ? "{MsgImage}" : (m.ContentType == MessageType.Video ? "{MsgVideo}" : (m.ContentType == MessageType.Streaming ? "{MsgStreaming}" : (m.ContentType == MessageType.Excel ? "{MsgExcel}" : (m.ContentType == MessageType.Word ? "{MsgWord}" : (m.ContentType == MessageType.Pdf ? "{MsgPdf}" : "{MsgFile}"))))))),
                                                        Content = m.Content,
                                                        DateCreated = m.DateCreated,
                                                        DateModify = m.DateModify,
                                                        DateRecall = m.DateRecall
                                                    };
            a = (await query.Where(u => u.Id.Equals(id)).ToListAsync());
            return a.FirstOrDefault();
        }
        public async Task<MessageResultDTO> CreateAsync(MessageDTO book)
        {
            var filters = new List<FilterDefinition<MessageDTO>>();
            book.DateCreated = DateTime.Now;
            book.DateModify = null;
            book.DateRecall = null;
            if (String.IsNullOrEmpty(book.Sender))
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = book.Id,
                        Sender = book.Sender,
                        GroupId = book.GroupId,
                        UserReaded = book.UserReaded,
                        UserDelete = book.UserDelete,
                        ContentType = book.ContentType,
                        Content = book.Content,
                        DateCreated = book.DateCreated,
                        DateModify = book.DateModify,
                        DateRecall = book.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{SenderIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(book.GroupId))
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = book.Id,
                        Sender = book.Sender,
                        GroupId = book.GroupId,
                        UserReaded = book.UserReaded,
                        UserDelete = book.UserDelete,
                        ContentType = book.ContentType,
                        Content = book.Content,
                        DateCreated = book.DateCreated,
                        DateModify = book.DateModify,
                        DateRecall = book.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{GroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _Message.InsertOneAsync(book);
            return new MessageResultDTO()
            {
                Data = new MessageDTO()
                {
                    Id = book.Id,
                    Sender = book.Sender,
                    GroupId = book.GroupId,
                    UserReaded = book.UserReaded,
                    UserDelete = book.UserDelete,
                    ContentType = book.ContentType,
                    Content = book.Content,
                    DateCreated = book.DateCreated,
                    DateModify = book.DateModify,
                    DateRecall = book.DateRecall
                },

                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<MessageResultDTO> UpdateAsync(string id, MessageDTO bookIn)
        {
            var a = (await _Message.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        GroupId = bookIn.GroupId,
                        UserReaded = bookIn.UserReaded,
                        UserDelete = bookIn.UserDelete,
                        ContentType = bookIn.ContentType,
                        Content = bookIn.Content,
                        DateCreated = bookIn.DateCreated,
                        DateModify = bookIn.DateModify,
                        DateRecall = bookIn.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Sender))
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        GroupId = bookIn.GroupId,
                        UserReaded = bookIn.UserReaded,
                        UserDelete = bookIn.UserDelete,
                        ContentType = bookIn.ContentType,
                        Content = bookIn.Content,
                        DateCreated = bookIn.DateCreated,
                        DateModify = bookIn.DateModify,
                        DateRecall = bookIn.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{SenderIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.GroupId))
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        GroupId = bookIn.GroupId,
                        UserReaded = bookIn.UserReaded,
                        UserDelete = bookIn.UserDelete,
                        ContentType = bookIn.ContentType,
                        Content = bookIn.Content,
                        DateCreated = bookIn.DateCreated,
                        DateModify = bookIn.DateModify,
                        DateRecall = bookIn.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{GroupnameIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            bookIn.DateModify = DateTime.Now;
            await _Message.ReplaceOneAsync(book => book.Id == id, bookIn);
            return new MessageResultDTO()
            {
                Data = new MessageDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    GroupId = bookIn.GroupId,
                    UserReaded = bookIn.UserReaded,
                    UserDelete = bookIn.UserDelete,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    DateCreated = bookIn.DateCreated,
                    DateModify = bookIn.DateModify,
                    DateRecall = bookIn.DateRecall
                },
                ResponseStatus = 1,
                Message = "{UpdateIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<MessageResultDTO> RemoveAsync(MessageDTO bookIn)
        {
            var a = (await _Message.FindAsync(book => book.Id == bookIn.Id)).ToList();
            if (a.Count < 1)
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        GroupId = bookIn.GroupId,
                        UserReaded = bookIn.UserReaded,
                        UserDelete = bookIn.UserDelete,
                        ContentType = bookIn.ContentType,
                        Content = bookIn.Content,
                        DateCreated = bookIn.DateCreated,
                        DateModify = bookIn.DateModify,
                        DateRecall = bookIn.DateRecall
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _Message.DeleteOneAsync(book => book.Id == bookIn.Id);
            return new MessageResultDTO()
            {
                Data = new MessageDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    GroupId = bookIn.GroupId,
                    UserReaded = bookIn.UserReaded,
                    UserDelete = bookIn.UserDelete,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    DateCreated = bookIn.DateCreated,
                    DateModify = bookIn.DateModify,
                    DateRecall = bookIn.DateRecall
                },

                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<MessageResultDTO> RemoveAsync(string id)
        {
            var a = (await _Message.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new MessageResultDTO()
                {
                    Data = new MessageDTO()
                    {
                        Id = id,
                        Sender = "",
                        GroupId = "",
                        UserReaded = null,
                        UserDelete = null,
                        ContentType = MessageType.Text,
                        Content = "",
                        DateCreated = DateTime.Now,
                        DateModify = null,
                        DateRecall = null
                    },

                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            await _Message.DeleteOneAsync(book => book.Id == id);
            return new MessageResultDTO()
            {
                Data = new MessageDTO()
                {
                    Id = id,
                    Sender = a[0].Sender,
                    GroupId = a[0].GroupId,
                    UserReaded = a[0].UserReaded,
                    UserDelete = a[0].UserDelete,
                    ContentType = a[0].ContentType,
                    Content = a[0].Content,
                    DateCreated = a[0].DateCreated,
                    DateModify = a[0].DateModify,
                    DateRecall = a[0].DateRecall
                },

                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }
        #endregion

        #region APP-API
        public async Task<MsgResultDTO> CreateAsync(MsgDTO bookIn, string Token, string Username)
        {
            var filters = new List<FilterDefinition<MessageDTO>>();
            if (String.IsNullOrEmpty(bookIn.Sender))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    ResponseStatus = false,
                    Message = "{SenderIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.GroupId))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    ResponseStatus = false,
                    Message = "{GroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            if (!(bookIn.ContentType == MessageType.Text || 
                bookIn.ContentType == MessageType.Image || 
                bookIn.ContentType == MessageType.Video || 
                bookIn.ContentType == MessageType.Streaming || 
                bookIn.ContentType == MessageType.Excel || 
                bookIn.ContentType == MessageType.Word || 
                bookIn.ContentType == MessageType.Pdf || 
                bookIn.ContentType == MessageType.File))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    ResponseStatus = false,
                    Message = "{MessageTypeIsWrong}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Content))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    ResponseStatus = false,
                    Message = "{MessageIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            MessageDTO book = new MessageDTO() {
                Id = "",
                Sender = bookIn.Sender,
                GroupId = bookIn.GroupId,
                UserReaded = new string[1] { Username },
                UserDelete = new string[0],
                ContentType = bookIn.ContentType,
                Content = bookIn.Content,
                DateCreated = DateTime.Now,
                DateModify = null,
                DateRecall = null
            };
            await _Message.InsertOneAsync(book);
            return new MsgResultDTO()
            {
                Id = bookIn.Id,
                Sender = book.Sender,
                Token = bookIn.Token,
                GroupId = book.GroupId,
                ContentType = book.ContentType,
                Content = book.Content,
                MessageId = book.Id,
                ResponseStatus = true,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<MsgResultDTO> UpdateAsync(MsgDTO bookIn, string Token, string Username)
        {
            var a = (await _Message.FindAsync(book => book.Id == bookIn.MessageId)).ToList();
            if (a.Count < 1)
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    MessageId = bookIn.MessageId,
                    ResponseStatus = false,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            var filters = new List<FilterDefinition<MessageDTO>>();
            if (String.IsNullOrEmpty(bookIn.Sender))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    MessageId = bookIn.MessageId,
                    ResponseStatus = false,
                    Message = "{SenderIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.GroupId))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    MessageId = bookIn.MessageId,
                    ResponseStatus = false,
                    Message = "{GroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            if (!(bookIn.ContentType == MessageType.Text ||
                bookIn.ContentType == MessageType.Image ||
                bookIn.ContentType == MessageType.Video ||
                bookIn.ContentType == MessageType.Streaming ||
                bookIn.ContentType == MessageType.Excel ||
                bookIn.ContentType == MessageType.Word ||
                bookIn.ContentType == MessageType.Pdf ||
                bookIn.ContentType == MessageType.File))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    MessageId = bookIn.MessageId,
                    ResponseStatus = false,
                    Message = "{MessageTypeIsWrong}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Content))
            {
                return new MsgResultDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    Token = bookIn.Token,
                    GroupId = bookIn.GroupId,
                    ContentType = bookIn.ContentType,
                    Content = bookIn.Content,
                    MessageId = bookIn.MessageId,
                    ResponseStatus = false,
                    Message = "{MessageIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            MessageDTO book = new MessageDTO()
            {
                Id = bookIn.MessageId,
                Sender = bookIn.Sender,
                GroupId = bookIn.GroupId,
                UserReaded = new string[1] { Username },
                UserDelete = new string[0],
                ContentType = bookIn.ContentType,
                Content = bookIn.Content,
                DateCreated = DateTime.Now,
                DateModify = null,
                DateRecall = null
            };
            await _Message.ReplaceOneAsync(b => b.Id == bookIn.MessageId, book);
            return new MsgResultDTO()
            {
                Id = bookIn.Id,
                Sender = bookIn.Sender,
                Token = bookIn.Token,
                GroupId = bookIn.GroupId,
                ContentType = bookIn.ContentType,
                Content = bookIn.Content,
                MessageId = book.Id,
                ResponseStatus = true,
                Message = "{UpdateIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }
        public async Task<MessageReadResultDTO> MesssageReadAsync(MessageReadDTO _filter, string Token, string Username)
        {
            var a = (await _Message.FindAsync(book => book.Id == _filter.Id)).ToList();
            if (a.Count < 1)
            {
                return new MessageReadResultDTO()
                {
                    Id = _filter.Id,
                    UserReaded = new string[0],
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            string[] a1 = new string[a[0].UserReaded.Length - 1];
            if (Array.IndexOf(a[0].UserReaded, Username) > -1)
            {
                int j = 0;
                foreach (string item in a[0].UserReaded)
                {
                    if (item != Username)
                    {
                        a1[j] = item; j = j + 1;
                    }
                }
                a[0].UserReaded = a1;
            }
            a[0].DateModify = DateTime.Now;
            var b = a[0];
            await _Message.ReplaceOneAsync(book => book.Id == _filter.Id, b);
            return new MessageReadResultDTO()
            {
                Id = _filter.Id,
                UserReaded = b.UserReaded,
                ResponseStatus = 1,
                Message = "{ReadIsSuccess}" //"Bạn ghi không tồn tại!"
            };
        }

        public async Task<MessageDeleteResultDTO> MesssageDeleteAsync(MessageReadDTO _filter, string Token, string Username)
        {
            var a = (await _Message.FindAsync(book => book.Id == _filter.Id)).ToList();
            if (a.Count < 1)
            {
                return new MessageDeleteResultDTO()
                {
                    Id = _filter.Id,
                    UserDelete = new string[0],
                    MessageId = _filter.Id,
                    Sender = Username,
                    Token = Token,
                    GroupId = "",
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            string[] a1 = new string[a[0].UserDelete.Length - 1];
            if (Array.IndexOf(a[0].UserDelete, Username) > -1)
            {
                int j = 0;
                foreach (string item in a[0].UserDelete)
                {
                    if (item!= Username)
                    {
                        a1[j] = item; j = j + 1;
                    }
                }
                a[0].UserDelete = a1;
            }
            a[0].DateModify = DateTime.Now;
            var b = a[0];
            await _Message.ReplaceOneAsync(book => book.Id == _filter.Id, b);
            return new MessageDeleteResultDTO()
            {
                Id = _filter.Id,
                UserDelete = b.UserDelete,
                MessageId = _filter.Id,
                Sender = Username,
                Token = Token,
                GroupId = b.GroupId,
                ResponseStatus = 1,
                Message = "{ReadIsSuccess}" //"Bạn ghi không tồn tại!"
            };
        }

        public async Task<MessageRecallResultDTO> MesssageRecallAsync(MessageReadDTO _filter, string Token, string Username)
        {
            var a = (await _Message.FindAsync(book => book.Id == _filter.Id)).ToList();
            if (a.Count < 1)
            {
                return new MessageRecallResultDTO()
                {
                    Id = _filter.Id,
                    MessageId = _filter.Id,
                    Sender = Username,
                    Token = Token,
                    GroupId = "",
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _Message.DeleteOneAsync(book => book.Id == _filter.Id);
            return new MessageRecallResultDTO()
            {
                Id = _filter.Id,
                MessageId = _filter.Id,
                Sender = Username,
                Token = Token,
                GroupId = a[0].GroupId,
                ResponseStatus = 1,
                Message = "{ReadIsSuccess}" //"Bạn ghi không tồn tại!"
            };
        }

        public async Task<MessageListResultDTO> SearchMesssageAsync(MessageSearchDTO _filter, string Token, string Username)
        {
            List<MessageListDTO> a;
            if (String.IsNullOrEmpty(_filter.GroupId) && String.IsNullOrEmpty(_filter.Keyword))
            {
                return new MessageListResultDTO()
                {
                    Data = new List<MessageListDTO>(),
                    Page = _filter.Page,
                    PageSize = _filter.PageSize,
                    Rowcount = 0
                };
            }

            IMongoQueryable<MessageListDTO> query = from g in _Group.AsQueryable().Where(u => u.Member.Contains(Username))
                                                    join m in _Message.AsQueryable() on g.Id equals m.GroupId
                                                    select new MessageListDTO()
                                                    {
                                                        Id = m.Id,
                                                        Sender = m.Sender,
                                                        GroupId = m.GroupId,
                                                        GroupName = g.Name,
                                                        GroupMember = g.Member,
                                                        UserReaded = m.UserReaded,
                                                        UserDelete = m.UserDelete,
                                                        ContentType = m.ContentType,
                                                        ContentTypeName = (m.ContentType == MessageType.Text ? "{MsgText}" : (m.ContentType == MessageType.Image ? "{MsgImage}" : (m.ContentType == MessageType.Video ? "{MsgVideo}" : (m.ContentType == MessageType.Streaming ? "{MsgStreaming}" : (m.ContentType == MessageType.Excel ? "{MsgExcel}" : (m.ContentType == MessageType.Word ? "{MsgWord}" : (m.ContentType == MessageType.Pdf ? "{MsgPdf}" : "{MsgFile}"))))))),
                                                        Content = m.Content,
                                                        DateCreated = m.DateCreated,
                                                        DateModify = m.DateModify,
                                                        DateRecall = m.DateRecall
                                                    };

            if (!String.IsNullOrEmpty(_filter.GroupId))
            {
                query = query.Where(u => u.GroupId.Equals(_filter.GroupId));
            }

            if (!String.IsNullOrEmpty(_filter.Keyword))
            {
                query = query.Where(u => (u.GroupName.Contains (_filter.Keyword) || u.Sender.Contains(_filter.Keyword) || u.Content.Contains(_filter.Keyword)));
            }

            a = (await query.ToListAsync());
            int Rowcount = a.Count;
            return new MessageListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateCreated)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<MessageListResultDTO> UnreadMesssageAsync(MessageUnreadDTO _filter, string Token, string Username)
        {
            List<MessageListDTO> a;
            if (String.IsNullOrEmpty(_filter.GroupId))
            {
                return new MessageListResultDTO()
                {
                    Data = new List<MessageListDTO>(),
                    Page = _filter.Page,
                    PageSize = _filter.PageSize,
                    Rowcount = 0
                };
            }

            IMongoQueryable<MessageListDTO> query = from g in _Group.AsQueryable().Where(u => (u.Member.Contains(Username) && u.Id.Equals(_filter.GroupId)))
                                                    join m in _Message.AsQueryable() on g.Id equals m.GroupId
                                                    select new MessageListDTO()
                                                    {
                                                        Id = m.Id,
                                                        Sender = m.Sender,
                                                        GroupId = m.GroupId,
                                                        GroupName = g.Name,
                                                        GroupMember = g.Member,
                                                        UserReaded = m.UserReaded,
                                                        UserDelete = m.UserDelete,
                                                        ContentType = m.ContentType,
                                                        ContentTypeName = (m.ContentType == MessageType.Text ? "{MsgText}" : (m.ContentType == MessageType.Image ? "{MsgImage}" : (m.ContentType == MessageType.Video ? "{MsgVideo}" : (m.ContentType == MessageType.Streaming ? "{MsgStreaming}" : (m.ContentType == MessageType.Excel ? "{MsgExcel}" : (m.ContentType == MessageType.Word ? "{MsgWord}" : (m.ContentType == MessageType.Pdf ? "{MsgPdf}" : "{MsgFile}"))))))),
                                                        Content = m.Content,
                                                        DateCreated = m.DateCreated,
                                                        DateModify = m.DateModify,
                                                        DateRecall = m.DateRecall
                                                    };

            a = (await query.ToListAsync());
            int Rowcount = a.Count;
            return new MessageListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateCreated)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }
        #endregion

        private string MessageTypeName(MessageType type)
        {
            switch (type)
            {
                case MessageType.Text: return "{MsgText}";
                case MessageType.Image: return "{MsgImage}";
                case MessageType.Video: return "{MsgVideo}";
                case MessageType.Streaming: return "{MsgStreaming}";
                case MessageType.Excel: return "{MsgExcel}";
                case MessageType.Word: return "{MsgWord}";
                case MessageType.Pdf: return "{MsgPdf}";
                case MessageType.File: return "{MsgFile}";
            }
            return "N/A";
        }
    }
}
