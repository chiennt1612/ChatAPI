using ChatAPI.Constant;
using ChatAPI.Models;
using ChatAPI.Models.DTO.Groupchat;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAPI.Services
{
    public interface IGroupService
    {
        #region CMS-API
        Task<GroupListResultDTO> GetAsync(int Page, int PageSize);
        Task<GroupListResultDTO> GetAsync(GroupFilterDTO _filter);
        Task<GroupDTO> GetAsync(string id);
        Task<GroupResultDTO> CreateAsync(GroupDTO book);
        Task<GroupResultDTO> UpdateAsync(string id, GroupDTO bookIn);
        Task<GroupResultDTO> RemoveAsync(GroupDTO bookIn);
        Task<GroupResultDTO> RemoveAsync(string id);
        #endregion

        #region APP-API
        Task<GroupResultDTO> CreateChat(Groupchat2DTO bookIn, string Token, string Username);
        Task<GroupResultDTO> CreateGroupchat(GroupchatDTO bookIn, string Token, string Username);
        Task<GroupListResultDTO> SearchGroupAsync(GroupSearchDTO _filter, string Token, string Username);
        Task<GroupJoinResultDTO> JoinGroupAsync(GroupJoinDTO bookIn, string Token, string Username);
        Task<GroupJoinResultDTO> RemoveGroupAsync(GroupRemoveDTO bookIn, string Token, string Username);
        Task<GroupJoinResultDTO> LeaveGroupAsync(GroupLeaveDTO bookIn, string Token, string Username);
        #endregion
    }

    public class GroupService : IGroupService
    {
        private readonly IMongoCollection<GroupDTO> _Group;
        private readonly ILogger<GroupService> _Log;
        public GroupService(IDBSetting settings, ILogger<GroupService> Log)
        {
            _Log = Log;
            _Log.LogInformation("Start object Group!");
            var client = new MongoClient(settings.CHAT_MONGODB_URL);
            var database = client.GetDatabase(settings.DatabaseName);

            _Group = database.GetCollection<GroupDTO>("group");
        }

        #region CMS-API
        public async Task<GroupListResultDTO> GetAsync(int Page, int PageSize)
        {
            List<GroupDTO> a;
            a = (await _Group.FindAsync(u => true)).ToList();
            int Rowcount = a.Count;
            return new GroupListResultDTO()
            {
                Data = a.OrderBy(u => u.Owner)
                    .Skip(PageSize * (Page - 1))
                    .Take(PageSize)
                    .ToList(),
                Page = Page,
                PageSize = PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<GroupListResultDTO> GetAsync(GroupFilterDTO _filter)
        {
            List<GroupDTO> a;

            bool kt = false;
            var filters = new List<FilterDefinition<GroupDTO>>();

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                var filter = Builders<GroupDTO>.Filter.Eq("Id", _filter.Id);
                filters.Add(filter);
                kt = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Owner))
                {
                    var filter = Builders<GroupDTO>.Filter.Where(u => u.Owner.Contains(_filter.Owner));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.Name))
                {
                    var filter = Builders<GroupDTO>.Filter.Where(u => u.Name.Contains(_filter.Name));
                    filters.Add(filter);
                    kt = true;
                }
            }

            var complexFilter = Builders<GroupDTO>.Filter.And(filters);
            if (kt)
            {
                a = (await _Group.FindAsync(complexFilter))
                    .ToList()
                    ;
            }
            else
            {
                a = (await _Group.FindAsync(book => true))
                    .ToList()
                    ;
            }

            int Rowcount = a.Count;
            return new GroupListResultDTO()
            {
                Data = a.OrderBy(u => u.Owner)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<GroupDTO> GetAsync(string id)
        {
            var a = await _Group.FindAsync<GroupDTO>(book => book.Id == id);
            return a.FirstOrDefault();
        }
        public async Task<GroupResultDTO> CreateAsync(GroupDTO book)
        {
            var filters = new List<FilterDefinition<GroupDTO>>();
            book.CreateDate = DateTime.Now;
            book.LastMessage = null;
            if (String.IsNullOrEmpty(book.Owner))
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = book.Id,
                        Owner = book.Owner,
                        Name = book.Name,
                        Member = book.Member,
                        Type = book.Type ,
                        IsPublish = book.IsPublish,
                        CreateDate = book.CreateDate,
                        LastMessage = book.LastMessage,
                        Avatar = book.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{OwnerIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(book.Name))
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = book.Id,
                        Owner = book.Owner,
                        Name = book.Name,
                        Member = book.Member,
                        Type = book.Type,
                        IsPublish = book.IsPublish,
                        CreateDate = book.CreateDate,
                        LastMessage = book.LastMessage,
                        Avatar = book.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{UsernameOrGroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _Group.InsertOneAsync(book);
            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = book.Id,
                    Owner = book.Owner,
                    Name = book.Name,
                    Member = book.Member,
                    Type = book.Type,
                    IsPublish = book.IsPublish,
                    CreateDate = book.CreateDate,
                    LastMessage = book.LastMessage,
                    Avatar = book.Avatar
                },
                
                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<GroupResultDTO> UpdateAsync(string id, GroupDTO bookIn)
        {
            var a = (await _Group.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Name = bookIn.Name,
                        Member = bookIn.Member,
                        Type = bookIn.Type,
                        IsPublish = bookIn.IsPublish,
                        CreateDate = bookIn.CreateDate,
                        LastMessage = bookIn.LastMessage,
                        Avatar = bookIn.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Owner))
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Name = bookIn.Name,
                        Member = bookIn.Member,
                        Type = bookIn.Type,
                        IsPublish = bookIn.IsPublish,
                        CreateDate = bookIn.CreateDate,
                        LastMessage = bookIn.LastMessage,
                        Avatar = bookIn.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{OwnerIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Name))
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Name = bookIn.Name,
                        Member = bookIn.Member,
                        Type = bookIn.Type,
                        IsPublish = bookIn.IsPublish,
                        CreateDate = bookIn.CreateDate,
                        LastMessage = bookIn.LastMessage,
                        Avatar = bookIn.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{GroupnameIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _Group.ReplaceOneAsync(book => book.Id == id, bookIn);
            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = bookIn.Id,
                    Owner = bookIn.Owner,
                    Name = bookIn.Name,
                    Member = bookIn.Member,
                    Type = bookIn.Type,
                    IsPublish = bookIn.IsPublish,
                    CreateDate = bookIn.CreateDate,
                    LastMessage = bookIn.LastMessage,
                    Avatar = bookIn.Avatar
                },
                ResponseStatus = 1,
                Message = "{UpdateIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<GroupResultDTO> RemoveAsync(GroupDTO bookIn)
        {
            var a = (await _Group.FindAsync(book => book.Id == bookIn.Id)).ToList();
            if (a.Count < 1)
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Name = bookIn.Name,
                        Member = bookIn.Member,
                        Type = bookIn.Type,
                        IsPublish = bookIn.IsPublish,
                        CreateDate = bookIn.CreateDate,
                        LastMessage = bookIn.LastMessage,
                        Avatar = bookIn.Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _Group.DeleteOneAsync(book => book.Id == bookIn.Id);
            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = bookIn.Id,
                    Owner = bookIn.Owner,
                    Name = bookIn.Name,
                    Member = bookIn.Member,
                    Type = bookIn.Type,
                    IsPublish = bookIn.IsPublish,
                    CreateDate = bookIn.CreateDate,
                    LastMessage = bookIn.LastMessage,
                    Avatar = bookIn.Avatar
                },
                
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<GroupResultDTO> RemoveAsync(string id)
        {
            var a = (await _Group.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = id,
                        Owner = "",
                        Name = "",
                        Member = null,
                        Type = GroupType.Single,
                        IsPublish = PublishType.Private,
                        CreateDate = DateTime.Now,
                        LastMessage = DateTime.Now,
                        Avatar = 0
                    },
                    
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            await _Group.DeleteOneAsync(book => book.Id == id);
            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = id,
                    Owner = a[0].Owner,
                    Name = a[0].Name,
                    Member = a[0].Member,
                    Type = a[0].Type,
                    IsPublish = a[0].IsPublish,
                    CreateDate = a[0].CreateDate,
                    LastMessage = a[0].LastMessage,
                    Avatar = a[0].Avatar
                },
                
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }
        #endregion

        #region APP-API
        public async Task<GroupResultDTO> CreateChat(Groupchat2DTO bookIn, string Token, string Username)
        {
            var a = (await _Group.FindAsync(book => book.Type == GroupType.Single && book.Member.Contains(Username) && book.Member.Contains(bookIn.Recieve))).ToList();
            if (a.Count > 0)
            {
                return new GroupResultDTO()
                {
                    Data = new GroupDTO()
                    {
                        Id = a[0].Id,
                        Owner = a[0].Owner,
                        Name = a[0].Name,
                        Member = a[0].Member,
                        Type = a[0].Type,
                        IsPublish = a[0].IsPublish,
                        CreateDate = a[0].CreateDate,
                        LastMessage = a[0].LastMessage,
                        Avatar = a[0].Avatar
                    },
                    ResponseStatus = -600,
                    Message = "{GroupIsExists}" //"Bạn ghi không tồn tại!"
                };
            }
            
            GroupDTO book = new GroupDTO() {
                Id = "",
                Owner = Username,
                Name = Username,
                Member = new string [2] { Username, bookIn.Recieve },
                Type = GroupType.Single, // chat doi
                IsPublish= PublishType.Private,
                CreateDate = DateTime.Now ,
                LastMessage = null,
                Avatar = 0
            };
            await _Group.InsertOneAsync(book);
            
            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = book.Id,
                    Owner = book.Owner,
                    Name = book.Name,
                    Member = book.Member,
                    Type = book.Type,
                    IsPublish = book.IsPublish,
                    CreateDate = book.CreateDate,
                    LastMessage = book.LastMessage,
                    Avatar = book.Avatar
                },

                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }
        
        public async Task<GroupResultDTO> CreateGroupchat(GroupchatDTO bookIn, string Token, string Username)
        {
            if(Array.IndexOf (bookIn.Member, Username) < 0)
            {
                string[] a = bookIn.Member;
                Array.Resize(ref a, bookIn.Member.Length + 1);
                a[bookIn.Member.Length] = Username;
                bookIn.Member = a;
            }
           GroupDTO book = new GroupDTO()
            {
                Id = "",
                Owner = Username,
                Name = bookIn.Name,
                Member = bookIn.Member,
                Type = GroupType.Group, // chat group
                CreateDate = DateTime.Now,
                LastMessage = null,
                Avatar = bookIn.Avatar
            };
            await _Group.InsertOneAsync(book);

            return new GroupResultDTO()
            {
                Data = new GroupDTO()
                {
                    Id = book.Id,
                    Owner = book.Owner,
                    Name = book.Name,
                    Member = book.Member,
                    Type = book.Type,
                    CreateDate = book.CreateDate,
                    LastMessage = book.LastMessage,
                    Avatar = book.Avatar
                },

                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }
        
        public async Task<GroupListResultDTO> SearchGroupAsync(GroupSearchDTO _filter, string Token, string Username)
        {
            List<GroupDTO> a;
            var filters = new List<FilterDefinition<GroupDTO>>();
            var /*filter = Builders<GroupDTO>.Filter.Where(u => u.Type == 1);
            filters.Add(filter);*/
            filter = Builders<GroupDTO>.Filter.Where(u => (u.Owner.Equals(Username) || u.Member.Contains(Username)));
            filters.Add(filter);

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                filter = Builders<GroupDTO>.Filter.Eq("Id", _filter.Id);
                filters.Add(filter);
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Keyword))
                {
                    filter = Builders<GroupDTO>.Filter.Where(u => (u.Name.Contains(_filter.Keyword) || u.Owner.Contains(_filter.Keyword) || u.Member.Contains(_filter.Keyword)));
                    filters.Add(filter);
                }
            }
            var complexFilter = Builders<GroupDTO>.Filter.And(filters);
            a = (await _Group.FindAsync(complexFilter)).ToList();

            int Rowcount = a.Count;
            return new GroupListResultDTO()
            {
                Data = a.OrderBy(u => u.Type).ThenByDescending(x => x.CreateDate).ThenByDescending(x => x.LastMessage)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }
        
        public async Task<GroupJoinResultDTO> JoinGroupAsync(GroupJoinDTO bookIn, string Token, string Username)
        {
            var a = (await _Group.FindAsync(book => book.Id == bookIn.GroupId)).ToList();
            if (a.Count < 1)
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = bookIn.Member,
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            // Add user to group || Join chi duoc phep voi nhom Publish
            if (!((Username == a[0].Owner) || // Nhom rieng owner //  && a[0].IsPublish == PublishType.Private
                    (a[0].IsPublish == PublishType.Publish) || // Nhom Publish
                    (Array.IndexOf(a[0].Member, Username) > -1 && a[0].IsPublish == PublishType.MemberAdd))) // Member co the them
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = bookIn.Member,
                    ResponseStatus = -600,
                    Message = "{YouAreNotPermission}" //"Bạn ghi không tồn tại!"
                };
            }

            string[] a1 = bookIn.Member;
            foreach (string item in bookIn.Member)
            {
                if (item != Username)
                {                    
                    Array.Resize(ref a1, bookIn.Member.Length + 1);
                    a1[a1.Length - 1] = item;
                }
            }
            bookIn.Member = a1;

            GroupDTO book = new GroupDTO()
            {
                Id = a[0].Id,
                Owner = a[0].Owner,
                Name = a[0].Name,
                Member = a1,
                Type = a[0].Type, // chat group
                CreateDate = a[0].CreateDate,
                LastMessage = a[0].LastMessage,
                Avatar = a[0].Avatar
            };
            await _Group.ReplaceOneAsync(b => b.Id == bookIn.GroupId, book);

            return new GroupJoinResultDTO()
            {
                GroupId = bookIn.GroupId,
                Member = bookIn.Member,

                ResponseStatus = 1,
                Message = "{JoinIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<GroupJoinResultDTO> RemoveGroupAsync(GroupRemoveDTO bookIn, string Token, string Username)
        {
            var a = (await _Group.FindAsync(book => book.Id == bookIn.GroupId)).ToList();
            if (a.Count < 1)
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = new string []{ bookIn.Username},
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            
            // Add user to group || Join chi duoc phep voi nhom Publish
            if (Username != a[0].Owner)
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = new string[1] { bookIn.Username },
                    ResponseStatus = -600,
                    Message = "{YouAreNotPermission}" //"Bạn ghi không tồn tại!"
                };
            }

            string[] a1 = new string[a[0].Member.Length - 1];
            if (Array.IndexOf(a[0].Member, bookIn.Username) > -1)
            {
                int j = 0;
                foreach (string item in a[0].Member)
                {
                    if (item!= bookIn.Username)
                    {
                        a1[j] = item;j = j + 1;
                    }
                }
                a[0].Member = a1;
            }
            else
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = new string[1] { bookIn.Username },
                    ResponseStatus = -600,
                    Message = "{MemberIsNotFound}" //"Bạn ghi không tồn tại!"
                };
            }

            GroupDTO book = new GroupDTO()
            {
                Id = a[0].Id,
                Owner = a[0].Owner,
                Name = a[0].Name,
                Member = a1,
                Type = a[0].Type, // chat group
                CreateDate = a[0].CreateDate,
                LastMessage = a[0].LastMessage,
                Avatar = a[0].Avatar
            };
            await _Group.ReplaceOneAsync(b => b.Id == bookIn.GroupId, book);

            return new GroupJoinResultDTO()
            {
                GroupId = bookIn.GroupId,
                Member = new string[1] { bookIn.Username },

                ResponseStatus = 1,
                Message = "{RemoveIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<GroupJoinResultDTO> LeaveGroupAsync(GroupLeaveDTO bookIn, string Token, string Username)
        {
            var a = (await _Group.FindAsync(book => book.Id == bookIn.GroupId)).ToList();
            if (a.Count < 1)
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = new string[1] { Username },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            string[] a1 = new string[(a[0].Member.Length>0? a[0].Member.Length: 1) - 1];
            if (Array.IndexOf(a[0].Member, Username) > -1)
            {
                int j = 0;
                foreach (string item in a[0].Member)
                {
                    if (item != Username)
                    {
                        a1[j] = item; j = j + 1;
                    }
                }
                a[0].Member = a1;
            }
            else
            {
                return new GroupJoinResultDTO()
                {
                    GroupId = bookIn.GroupId,
                    Member = new string[1] { Username },
                    ResponseStatus = -600,
                    Message = "{MemberIsNotFound}" //"Bạn ghi không tồn tại!"
                };
            }

            GroupDTO book = new GroupDTO()
            {
                Id = a[0].Id,
                Owner = a[0].Owner,
                Name = a[0].Name,
                Member = a1,
                Type = a[0].Type, // chat group
                CreateDate = a[0].CreateDate,
                LastMessage = a[0].LastMessage,
                Avatar = a[0].Avatar
            };
            await _Group.ReplaceOneAsync(b => b.Id == bookIn.GroupId, book);

            return new GroupJoinResultDTO()
            {
                GroupId = bookIn.GroupId,
                Member = new string[1] { Username },

                ResponseStatus = 1,
                Message = "{JoinIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }
#endregion
    }
}
