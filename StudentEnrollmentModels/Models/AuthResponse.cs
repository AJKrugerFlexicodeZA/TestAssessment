using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class AuthResponse {
        public int? Code { get; set; }
        public int? UserId { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Name { get; set; }
        public string? Role { get; set; }
        public dynamic? Error { get; set; }
    };

}

