using ChatAPI.Models;
using ChatAPI.Models.DTO.Blacklist;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAPI.Services
{
    public interface IBlacklistService
    {
        Task<BlacklistListResultDTO> GetAsync(int Page, int PageSize);
        Task<BlacklistListResultDTO> GetAsync(BlacklistFilterDTO _filter);
        Task<BlacklistDTO> GetAsync(string id);
        Task<BlacklistResultDTO> CreateAsync(BlacklistDTO book);
        Task<BlacklistResultDTO> UpdateAsync(string id, BlacklistDTO bookIn);
        Task<BlacklistResultDTO> RemoveAsync(BlacklistDTO bookIn);
        Task<BlacklistResultDTO> RemoveAsync(string id);
    }

    public class BlacklistService : IBlacklistService
    {
        private readonly IMongoCollection<BlacklistDTO> _Blacklist;
        private readonly ILogger<BlacklistService> _Log;
        public BlacklistService(IDBSetting settings, ILogger<BlacklistService> Log)
        {
            _Log = Log;
            _Log.LogInformation("Start object Blacklist!");
            var client = new MongoClient(settings.CHAT_MONGODB_URL);
            var database = client.GetDatabase(settings.DatabaseName);

            _Blacklist = database.GetCollection<BlacklistDTO>("blacklist");
        }

        public async Task<BlacklistListResultDTO> GetAsync(int Page, int PageSize)
        {
            List<BlacklistDTO> a = (await _Blacklist.FindAsync(u => true)).ToList();
            int Rowcount = a.Count;
            return new BlacklistListResultDTO()
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

        public async Task<BlacklistListResultDTO> GetAsync(BlacklistFilterDTO _filter)
        {
            List<BlacklistDTO> a;

            bool kt = false;
            var filters = new List<FilterDefinition<BlacklistDTO>>();

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                var filter = Builders<BlacklistDTO>.Filter.Eq("Id", _filter.Id);
                filters.Add(filter);
                kt = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Owner))
                {
                    var filter = Builders<BlacklistDTO>.Filter.Where(u => u.Owner.Contains(_filter.Owner));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.Username))
                {
                    var filter = Builders<BlacklistDTO>.Filter.Where(u => u.Username.Contains(_filter.Username));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.Groupname))
                {
                    var filter = Builders<BlacklistDTO>.Filter.Where(u => u.Groupname.Contains(_filter.Groupname));
                    filters.Add(filter);
                    kt = true;
                }
            }

            var complexFilter = Builders<BlacklistDTO>.Filter.And(filters);
            if (kt)
            {
                a = (await _Blacklist.FindAsync(complexFilter)).ToList();
            }
            else
            {
                a = (await _Blacklist.FindAsync(book => true)).ToList();
            }
            int Rowcount = a.Count;
            return new BlacklistListResultDTO()
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

        public async Task<BlacklistDTO> GetAsync(string id)
        {
            var a = await _Blacklist.FindAsync<BlacklistDTO>(book => book.Id == id);
            return a.FirstOrDefault();
        }
        public async Task<BlacklistResultDTO> CreateAsync(BlacklistDTO book)
        {
            var filters = new List<FilterDefinition<BlacklistDTO>>();

            if (String.IsNullOrEmpty(book.Owner))
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = book.Id,
                        Owner = book.Owner,
                        Username = book.Username,
                        Groupname = book.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{OwnerIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(book.Username) && String.IsNullOrEmpty(book.Groupname))
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = book.Id,
                        Owner = book.Owner,
                        Username = book.Username,
                        Groupname = book.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{UsernameOrGroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Blacklist!"
                };
            }

            // Check ton tai
            var filter = Builders<BlacklistDTO>.Filter.Where(u => u.Owner.Contains(book.Owner));
            filters.Add(filter);
            filter = Builders<BlacklistDTO>.Filter.Where(u => u.Username.Contains(book.Username));
            filters.Add(filter);
            filter = Builders<BlacklistDTO>.Filter.Where(u => u.Groupname.Contains(book.Groupname));
            filters.Add(filter);
            var complexFilter = Builders<BlacklistDTO>.Filter.And(filters);
            var a = (await _Blacklist.FindAsync(complexFilter)).ToList();
            if (a.Count > 0)
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = book.Id,
                        Owner = book.Owner,
                        Username = book.Username,
                        Groupname = book.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{UsernameOrGroupchatIsExists}" //"Tài khoản hoặc nhóm chat bạn chọn đã có trong Blacklist!"
                };
            }

            await _Blacklist.InsertOneAsync(book);
            return new BlacklistResultDTO()
            {
                Data = new BlacklistDTO()
                {
                    Id = book.Id,
                    Owner = book.Owner,
                    Username = book.Username,
                    Groupname = book.Groupname
                },
                
                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<BlacklistResultDTO> UpdateAsync(string id, BlacklistDTO bookIn)
        {
            var a = (await _Blacklist.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Username = bookIn.Username,
                        Groupname = bookIn.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Owner))
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Username = bookIn.Username,
                        Groupname = bookIn.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{OwnerIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Username) && String.IsNullOrEmpty(bookIn.Groupname))
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Username = bookIn.Username,
                        Groupname = bookIn.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{UsernameOrGroupchatIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Blacklist!"
                };
            }

            // Check ton tai
            var filters = new List<FilterDefinition<BlacklistDTO>>();
            var filter = Builders<BlacklistDTO>.Filter.Where(u => u.Owner.Contains(bookIn.Owner));
            filters.Add(filter);
            filter = Builders<BlacklistDTO>.Filter.Where(u => u.Username.Contains(bookIn.Username));
            filters.Add(filter);
            filter = Builders<BlacklistDTO>.Filter.Where(u => u.Groupname.Contains(bookIn.Groupname));
            filters.Add(filter);
            filter = Builders<BlacklistDTO>.Filter.Where(u => u.Id != bookIn.Id);
            filters.Add(filter);
            var complexFilter = Builders<BlacklistDTO>.Filter.And(filters);
            a = (await _Blacklist.FindAsync(complexFilter)).ToList();
            if (a.Count > 0)
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Username = bookIn.Username,
                        Groupname = bookIn.Groupname
                    },
                    ResponseStatus = -600,
                    Message = "{UsernameOrGroupchatIsExists}" //"Tài khoản hoặc nhóm chat bạn chọn đã có trong Blacklist!"
                };
            }

            await _Blacklist.ReplaceOneAsync(book => book.Id == id, bookIn);
            return new BlacklistResultDTO()
            {
                Data = new BlacklistDTO()
                {
                    Id = bookIn.Id,
                    Owner = bookIn.Owner,
                    Username = bookIn.Username,
                    Groupname = bookIn.Groupname
                },
                
                ResponseStatus = 1,
                Message = "{UpdateIsSuccess}" //"Cập nhật Blacklist thành công!"
            };
        }

        public async Task<BlacklistResultDTO> RemoveAsync(BlacklistDTO bookIn)
        {
            var a = (await _Blacklist.FindAsync(book => book.Id == bookIn.Id)).ToList();
            if (a.Count < 1)
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = bookIn.Id,
                        Owner = bookIn.Owner,
                        Username = bookIn.Username,
                        Groupname = bookIn.Groupname
                    },
                    
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _Blacklist.DeleteOneAsync(book => book.Id == bookIn.Id);
            return new BlacklistResultDTO()
            {
                Data = new BlacklistDTO()
                {
                    Id = bookIn.Id,
                    Owner = bookIn.Owner,
                    Username = bookIn.Username,
                    Groupname = bookIn.Groupname
                },
                
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Blacklist thành công!"
            };
        }

        public async Task<BlacklistResultDTO> RemoveAsync(string id)
        {
            var a = (await _Blacklist.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new BlacklistResultDTO()
                {
                    Data = new BlacklistDTO()
                    {
                        Id = id,
                        Owner = "",
                        Username = "",
                        Groupname = ""
                    },                    
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            await _Blacklist.DeleteOneAsync(book => book.Id == id);
            return new BlacklistResultDTO()
            {
                Data = new BlacklistDTO()
                {
                    Id = id,
                    Owner = a[0].Owner,
                    Username = a[0].Username,
                    Groupname = a[0].Groupname
                },               
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Blacklist thành công!"
            };
        }
    }
}
