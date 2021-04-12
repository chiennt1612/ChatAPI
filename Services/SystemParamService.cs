using ChatAPI.Models;
using ChatAPI.Models.DTO.SystemParam;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAPI.Services
{
    public interface ISystemParamService
    {
        Task<SystemParamListResultDTO> GetAsync(int Page, int PageSize);
        Task<SystemParamListResultDTO> GetAsync(SystemParamFilterDTO _filter);
        Task<SystemParamDTO> GetAsync(string id);
        Task<SystemParamResultDTO> CreateAsync(SystemParamDTO book);
        Task<SystemParamResultDTO> UpdateAsync(string id, SystemParamDTO bookIn);
        Task<SystemParamResultDTO> RemoveAsync(SystemParamDTO bookIn);
        Task<SystemParamResultDTO> RemoveAsync(string id);
    }
    public class SystemParamService : ISystemParamService
    {
        private readonly IMongoCollection<SystemParamDTO> _SystemParam;
        private readonly ILogger<SystemParamService> _Log;
        public SystemParamService(IDBSetting settings, ILogger<SystemParamService> Log)
        {
            _Log = Log;
            _Log.LogInformation("Start object SystemParam!");
            var client = new MongoClient(settings.CHAT_MONGODB_URL);
            var database = client.GetDatabase(settings.DatabaseName);

            _SystemParam = database.GetCollection<SystemParamDTO>("systemparam");
        }

        public async Task<SystemParamListResultDTO> GetAsync(int Page, int PageSize)
        {
            List<SystemParamDTO> a= (await _SystemParam.FindAsync(u => true)).ToList();
            int Rowcount = a.Count;
            return new SystemParamListResultDTO()
            {
                Data = a.OrderBy(u => u.Code)
                    .Skip(PageSize * (Page - 1))
                    .Take(PageSize)
                    .ToList(),
                Page = Page,
                PageSize = PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<SystemParamListResultDTO> GetAsync(SystemParamFilterDTO _filter) {
            List<SystemParamDTO> a;

            bool kt = false;
            var filters = new List<FilterDefinition<SystemParamDTO>>();

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                var filter = Builders<SystemParamDTO>.Filter.Eq("Id", _filter.Id);
                filters.Add(filter);
                kt = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Code))
                {
                    var filter = Builders<SystemParamDTO>.Filter.Where(u => u.Code.Contains(_filter.Code));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.Value))
                {
                    var filter = Builders<SystemParamDTO>.Filter.Where(u => u.Value.Contains(_filter.Value));
                    filters.Add(filter);
                    kt = true;
                }
            }
            
            var complexFilter = Builders<SystemParamDTO>.Filter.And(filters);
            if (kt)
            {
                a = (await _SystemParam.FindAsync(complexFilter))
                    .ToList()
                    ;
            }
            else
            {
                a = (await _SystemParam.FindAsync(book => true))
                    .ToList()
                    ;
            }

            int Rowcount = a.Count;
            return new SystemParamListResultDTO()
            {
                Data = a.OrderBy(u => u.Code)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<SystemParamDTO> GetAsync(string id)
        {
            var a = await _SystemParam.FindAsync<SystemParamDTO>(book => book.Id == id);
            return a.FirstOrDefault();
        }
        public async Task<SystemParamResultDTO> CreateAsync(SystemParamDTO book)
        {
            if (String.IsNullOrEmpty(book.Code))
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = book.Id,
                        Code = book.Code,
                        Value = book.Value
                    },
                    ResponseStatus = -600,
                    Message = "{CodeIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(book.Value))
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = book.Id,
                        Code = book.Code,
                        Value = book.Value
                    },
                    ResponseStatus = -600,
                    Message = "{ValueIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _SystemParam.InsertOneAsync(book);
            return new SystemParamResultDTO()
            {
                Data = new SystemParamDTO()
                {
                    Id = book.Id,
                    Code = book.Code,
                    Value = book.Value
                },
                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
            }; 
        }

        public async Task<SystemParamResultDTO> UpdateAsync(string id, SystemParamDTO bookIn)
        {
            var a = (await _SystemParam.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = bookIn.Id,
                        Code = bookIn.Code,
                        Value = bookIn.Value
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            if (String.IsNullOrEmpty(bookIn.Code))
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = bookIn.Id,
                        Code = bookIn.Code,
                        Value = bookIn.Value
                    },
                    ResponseStatus = -600,
                    Message = "{CodeIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Value))
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = bookIn.Id,
                        Code = bookIn.Code,
                        Value = bookIn.Value
                    },
                    ResponseStatus = -600,
                    Message = "{ValueIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }
            await _SystemParam.ReplaceOneAsync(book => book.Id == id, bookIn);
            return new SystemParamResultDTO()
            {
                Data = new SystemParamDTO()
                {
                    Id = bookIn.Id,
                    Code = bookIn.Code,
                    Value = bookIn.Value
                },
                ResponseStatus = 1,
                Message = "{UpdateIsSuccess}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
            };
        }

        public async Task<SystemParamResultDTO> RemoveAsync(SystemParamDTO bookIn)
        {
            var a = (await _SystemParam.FindAsync(book => book.Id == bookIn.Id)).ToList();
            if (a.Count < 1)
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = bookIn.Id,
                        Code = bookIn.Code,
                        Value = bookIn.Value
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _SystemParam.DeleteOneAsync(book => book.Id == bookIn.Id);
            return new SystemParamResultDTO()
            {
                Data = new SystemParamDTO()
                {
                    Id = bookIn.Id,
                    Code = bookIn.Code,
                    Value = bookIn.Value
                },
                ResponseStatus = -600,
                Message = "{Delete}" //"Bạn ghi không tồn tại!"
            };
        }

        public async Task<SystemParamResultDTO> RemoveAsync(string id)
        {
            var a = (await _SystemParam.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new SystemParamResultDTO()
                {
                    Data = new SystemParamDTO()
                    {
                        Id = id,
                        Code = "",
                        Value = ""
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _SystemParam.DeleteOneAsync(book => book.Id == id);
            return new SystemParamResultDTO()
            {
                Data = new SystemParamDTO()
                {
                    Id = a[0].Id,
                    Code = a[0].Code,
                    Value = a[0].Value
                },
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
            };
        }
    }
}
