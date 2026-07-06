using EMI.EmployeeManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMI.EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IAuthBLL _authBLL;

        public AuthController(
            IConfiguration config,
            IAuthBLL authBLL)
        {
            _config = config;
            _authBLL = authBLL;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin request)
        {
            // Validate user credentials
            var user = await _authBLL.AuthenticateAsync(request.Username, request.Password);

            // Return Unauthorized if authentication fails
            if (user == null)
                return Unauthorized("Invalid credentials");

            // Create JWT claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
            };

            // Add user roles to the token
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Read JWT settings from configuration
            var jwtSettings = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            // Generate JWT token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: credentials);

            // Serialize JWT token
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            // Return generated token
            return Ok(new
            {
                Token = jwt
            });
        }
    }

    public class UserLogin
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}