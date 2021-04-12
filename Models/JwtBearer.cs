using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAPI.Models
{
    public class JwtBearer : IJwtBearer
    {
        public string SecurityKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }

    public interface IJwtBearer
    {
        string SecurityKey { get; set; }
        string Issuer { get; set; }
        string Audience { get; set; }
    }
}
