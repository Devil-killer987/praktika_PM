
using api_work2.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

namespace Api_work.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private AgroControlEntities db = new AgroControlEntities();

        // POST: api/auth/login
        [HttpPost]
        [Route("login")]
        [ResponseType(typeof(object))]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Username and password required");
            }

            var user = db.users.FirstOrDefault(u => u.username == request.Username);

            if (user == null || user.password_hash != request.Password)
            {
                return Unauthorized();
            }

            if (user.is_active == false)
            {
                return BadRequest("User account is disabled");
            }

            // Update last login
            user.last_login = DateTime.Now;
            db.SaveChanges();

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                id = user.id,
                username = user.username,
                full_name = user.full_name,
                role = user.role,
                email = user.email,
                department = user.department,
                token = token,
                expires_in = 86400
            });
        }

        // POST: api/auth/register
        [HttpPost]
        [Route("register")]
        [ResponseType(typeof(users))]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (db.users.Any(u => u.username == request.Username))
            {
                return BadRequest("Username already exists");
            }

            if (db.users.Any(u => u.email == request.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new users
            {
                username = request.Username,
                password_hash = request.Password,
                full_name = request.FullName,
                role = request.Role ?? "observer",
                email = request.Email,
                phone = request.Phone,
                is_active = true,
                created_at = DateTime.Now,
                department = request.Department
            };

            db.users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.id }, user);
        }

        // POST: api/auth/changepassword
        [HttpPost]
        [Route("changepassword")]
        [ResponseType(typeof(string))]
        public IHttpActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = db.users.FirstOrDefault(u => u.id == request.UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.password_hash != request.OldPassword)
            {
                return BadRequest("Old password is incorrect");
            }

            user.password_hash = request.NewPassword;
            db.SaveChanges();

            return Ok("Password changed successfully");
        }

        private string GenerateJwtToken(users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("AgroControlSuperSecretKey2025ForJWTToken12345"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.username),
                new Claim(ClaimTypes.Role, user.role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "AgroControlAPI",
                audience: "AgroControlClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Request DTOs
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
    }

    public class ChangePasswordRequest
    {
        public int UserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}