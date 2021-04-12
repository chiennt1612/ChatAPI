using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAPI.Models
{
    public class DBSetting : IDBSetting
    {
        public string CHAT_MONGODB_URL { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDBSetting
    {
        string CHAT_MONGODB_URL { get; set; }
        string DatabaseName { get; set; }
    }
}
