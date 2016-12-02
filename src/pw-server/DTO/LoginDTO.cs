using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pwServer.DTO
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
