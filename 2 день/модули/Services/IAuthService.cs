using api_work2.DTOs;
using api_work2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace api_work2.Services
{
    public interface IAuthService
    {
        Task<users> Authenticate(string username, string password);
        string GenerateJwtToken(users user);
        Task<users> Register(RegisterRequestDto request);
    }
}