using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIDTIER.Models
{
    public class Register
    {
        [Required(ErrorMessage = "Name Is Required.")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Email Is Required."), EmailAddress]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password Is Required."), DataType(DataType.Password)]
        public string? Password { get; set; }
        public Roles? Role { get; set; }
    };
}
