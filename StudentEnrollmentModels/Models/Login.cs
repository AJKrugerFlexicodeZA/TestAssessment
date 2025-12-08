using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class Login {
        [Required(ErrorMessage ="Email Is Required."), EmailAddress]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password Is Required."), DataType(DataType.Password)]
        public string? Password {  get; set; }
    };

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Message { get; set; }
    }
}
