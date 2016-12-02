using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pwServer.DTO
{
    public class RegisterDTO
    {
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
    }
}
