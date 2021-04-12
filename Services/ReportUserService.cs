using ChatAPI.Models;
using ChatAPI.Models.DTO.ReportUser;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using ChatAPI.Constant;

namespace ChatAPI.Services
{
    public interface IReportUserService
    {
        Task<ReportUserListResultDTO> GetAsync(int Page, int PageSize);
        Task<ReportUserListResultDTO> GetAsync(ReportUserFilterDTO _filter);
        Task<ReportUserDTO> GetAsync(string id);
        Task<ReportUserResultDTO> CreateAsync(ReportUserDTO book);
        Task<ReportUserResultDTO> UpdateAsync(string id, ReportUserDTO bookIn);
        Task<ReportUserResultDTO> RemoveAsync(ReportUserDTO bookIn);
        Task<ReportUserResultDTO> RemoveAsync(string id);
    }
    public class ReportUserService : IReportUserService
    {
        private readonly IMongoCollection<ReportUserDTO> _Group;
        private readonly ILogger<ReportUserService> _Log;
        public ReportUserService(IDBSetting settings, ILogger<ReportUserService> Log)
        {
            _Log = Log;
            _Log.LogInformation("Start object Group!");
            var client = new MongoClient(settings.CHAT_MONGODB_URL);
            var database = client.GetDatabase(settings.DatabaseName);

            _Group = database.GetCollection<ReportUserDTO>("reportuser");
        }

        public async Task<ReportUserListResultDTO> GetAsync(int Page, int PageSize)
        {
            List<ReportUserDTO> a = (await _Group.FindAsync(u => true)).ToList();
            int Rowcount = a.Count;
            return new ReportUserListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateRequest)
                    .Skip(PageSize * (Page - 1))
                    .Take(PageSize)
                    .ToList(),
                Page = Page,
                PageSize = PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<ReportUserListResultDTO> GetAsync(ReportUserFilterDTO _filter)
        {
            List<ReportUserDTO> a;

            bool kt = false;
            var filters = new List<FilterDefinition<ReportUserDTO>>();

            if (!String.IsNullOrEmpty(_filter.Id))
            {
                var filter = Builders<ReportUserDTO>.Filter.Eq("Id", _filter.Id);
                filters.Add(filter);
                kt = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(_filter.Sender))
                {
                    var filter = Builders<ReportUserDTO>.Filter.Where(u => u.Sender.Contains(_filter.Sender));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.ReportUser))
                {
                    var filter = Builders<ReportUserDTO>.Filter.Where(u => u.ReportUser.Contains(_filter.ReportUser));
                    filters.Add(filter);
                    kt = true;
                }

                if (!String.IsNullOrEmpty(_filter.Admin))
                {
                    var filter = Builders<ReportUserDTO>.Filter.Where(u => u.Admin.Contains(_filter.Admin));
                    filters.Add(filter);
                    kt = true;
                }

                if (!(_filter.Status == Approved.Approved || _filter.Status == Approved.Reject || _filter.Status == Approved.CreateNew))
                {
                    var filter = Builders<ReportUserDTO>.Filter.Eq("Status", _filter.Status);
                    filters.Add(filter);
                    kt = true;
                }
            }

            var complexFilter = Builders<ReportUserDTO>.Filter.And(filters);
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
            return new ReportUserListResultDTO()
            {
                Data = a.OrderByDescending(u => u.DateRequest)
                    .Skip(_filter.PageSize * (_filter.Page - 1))
                    .Take(_filter.PageSize)
                    .ToList(),
                Page = _filter.Page,
                PageSize = _filter.PageSize,
                Rowcount = Rowcount
            };
        }

        public async Task<ReportUserDTO> GetAsync(string id)
        {
            var a = await _Group.FindAsync<ReportUserDTO>(book => book.Id == id);
            return a.FirstOrDefault();
        }
        public async Task<ReportUserResultDTO> CreateAsync(ReportUserDTO book)
        {
            var filters = new List<FilterDefinition<ReportUserDTO>>();
            book.DateRequest = DateTime.Now;
            book.DateApprove = null;
            if (String.IsNullOrEmpty(book.Sender))
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = book.Id,
                        Sender = book.Sender,
                        ReportUser = book.ReportUser,
                        Status = book.Status,
                        Admin = book.Admin,
                        DateRequest = book.DateRequest,
                        DateApprove = book.DateApprove,
                    },
                    ResponseStatus = -600,
                    Message = "{OwnerIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(book.Sender))
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = book.Id,
                        Sender = book.Sender,
                        ReportUser = book.ReportUser,
                        Status = book.Status,
                        Admin = book.Admin,
                        DateRequest = book.DateRequest,
                        DateApprove = book.DateApprove,
                    },
                    ResponseStatus = -600,
                    Message = "{SenderIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _Group.InsertOneAsync(book);
            return new ReportUserResultDTO()
            {
                Data = new ReportUserDTO()
                {
                    Id = book.Id,
                    Sender = book.Sender,
                    ReportUser = book.ReportUser,
                    Status = book.Status,
                    Admin = book.Admin,
                    DateRequest = book.DateRequest,
                    DateApprove = book.DateApprove,
                },
                
                ResponseStatus = 1,
                Message = "{AddnewIsSuccess}" //"Bạn đã thêm Backlist thành công"
            };
        }

        public async Task<ReportUserResultDTO> UpdateAsync(string id, ReportUserDTO bookIn)
        {
            bookIn.DateApprove = DateTime.Now;
            var a = (await _Group.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        ReportUser = bookIn.ReportUser,
                        Status = bookIn.Status,
                        Admin = bookIn.Admin,
                        DateRequest = bookIn.DateRequest,
                        DateApprove = bookIn.DateApprove
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.Sender))
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        ReportUser = bookIn.ReportUser,
                        Status = bookIn.Status,
                        Admin = bookIn.Admin,
                        DateRequest = bookIn.DateRequest,
                        DateApprove = bookIn.DateApprove
                    },
                    ResponseStatus = -600,
                    Message = "{SenderIsNull}" //"Bạn chưa nhập tài khoản yêu cầu!"
                };
            }

            if (String.IsNullOrEmpty(bookIn.ReportUser))
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        ReportUser = bookIn.ReportUser,
                        Status = bookIn.Status,
                        Admin = bookIn.Admin,
                        DateRequest = bookIn.DateRequest,
                        DateApprove = bookIn.DateApprove
                    },
                    ResponseStatus = -600,
                    Message = "{ReportUserIsNull}" //"Bạn cần chọn tài khoản hoặc nhóm chat cần đưa vào Group!"
                };
            }

            await _Group.ReplaceOneAsync(book => book.Id == id, bookIn);
            return new ReportUserResultDTO()
            {
                Data = new ReportUserDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    ReportUser = bookIn.ReportUser,
                    Status = bookIn.Status,
                    Admin = bookIn.Admin,
                    DateRequest = bookIn.DateRequest,
                    DateApprove = bookIn.DateApprove
                },
                ResponseStatus = 1,
                Message = "{UpdateIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<ReportUserResultDTO> RemoveAsync(ReportUserDTO bookIn)
        {
            var a = (await _Group.FindAsync(book => book.Id == bookIn.Id)).ToList();
            if (a.Count < 1)
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = bookIn.Id,
                        Sender = bookIn.Sender,
                        ReportUser = bookIn.ReportUser,
                        Status = bookIn.Status,
                        Admin = bookIn.Admin,
                        DateRequest = bookIn.DateRequest,
                        DateApprove = bookIn.DateApprove
                    },
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }
            await _Group.DeleteOneAsync(book => book.Id == bookIn.Id);
            return new ReportUserResultDTO()
            {
                Data = new ReportUserDTO()
                {
                    Id = bookIn.Id,
                    Sender = bookIn.Sender,
                    ReportUser = bookIn.ReportUser,
                    Status = bookIn.Status,
                    Admin = bookIn.Admin,
                    DateRequest = bookIn.DateRequest,
                    DateApprove = bookIn.DateApprove
                },
                
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }

        public async Task<ReportUserResultDTO> RemoveAsync(string id)
        {
            var a = (await _Group.FindAsync(book => book.Id == id)).ToList();
            if (a.Count < 1)
            {
                return new ReportUserResultDTO()
                {
                    Data = new ReportUserDTO()
                    {
                        Id = id,
                        Sender = "",
                        ReportUser = "",
                        Status = Approved.CreateNew,
                        Admin = "",
                        DateRequest = DateTime.Now,
                        DateApprove = null
                    },
                    
                    ResponseStatus = -600,
                    Message = "{RecordIsNotfound}" //"Bạn ghi không tồn tại!"
                };
            }

            await _Group.DeleteOneAsync(book => book.Id == id);
            return new ReportUserResultDTO()
            {
                Data = new ReportUserDTO()
                {
                    Id = a[0].Id,
                    Sender = a[0].Sender,
                    ReportUser = a[0].ReportUser,
                    Status = a[0].Status,
                    Admin = a[0].Admin,
                    DateRequest = a[0].DateRequest,
                    DateApprove = a[0].DateApprove
                },
                
                ResponseStatus = 1,
                Message = "{DeleteIsSuccess}" //"Cập nhật Group thành công!"
            };
        }
    }
}
