using Microsoft.OpenApi.Models;

namespace ChatAPI
{
    internal class ApiKeyScheme : OpenApiSecurityScheme
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string In { get; set; }
        public string Type { get; set; }

        public string Scheme { get; set; }
    }
}