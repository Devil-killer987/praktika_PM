using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorMD.Models
{
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public int id { get; set; }
        public string username { get; set; }
        public string full_name { get; set; }
        public string role { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }
}
