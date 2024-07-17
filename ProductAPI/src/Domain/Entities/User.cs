using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordHash { get; set; }
        public bool IsInWhiteList { get; set; }
        public string? ConfirmationCode { get; set; }
    }
}
