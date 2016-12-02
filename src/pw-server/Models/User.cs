using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pwServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public DateTime RegisterTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public double Balance { get; set; }
    }
}
