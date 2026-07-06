using EMI.EmployeeManagement.BLL.Interfaces;
using EMI.EmployeeManagement.DAL.Interfaces;
using EMI.EmployeeManagement.Entities.Dto;
using Microsoft.Extensions.Logging;

namespace EMI.EmployeeManagement.BLL
{
    public class AuthBLL : IAuthBLL
    {
        private readonly IAuthDAL _authDAL;
        private readonly ILogger<AuthBLL> _logger;

        public AuthBLL(IAuthDAL authDAL, ILogger<AuthBLL> logger)
        {
            _authDAL = authDAL;
            _logger = logger;
        }

        public async Task<AuthUserDto?> AuthenticateAsync(string username, string password)
        {
            _logger.LogInformation("Authenticating user {Username}", username);

            var user = await _authDAL.AuthenticateAsync(username, password);

            if (user == null)
            {
                _logger.LogWarning("Authentication failed for user {Username}", username);
                return null;
            }

            _logger.LogInformation("User {Username} authenticated successfully", username);

            return user;
        }
    }
}