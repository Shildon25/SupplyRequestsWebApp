using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using SupplyManagement.Models;

namespace SupplyManagement.Helpers
{
    public class UserHelper
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public UserHelper(UserManager<IdentityUser> userManager, ILogger logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(string, User)> GetCurrentUserAsync(ClaimsPrincipal currentUser)
        {
            // Check if the current user is null
            if (currentUser == null)
            {
                // Log an error if the current user is null
                this._logger.LogError("Current user is null.");
                // Redirect to an error page or return an appropriate response
                throw new Exception("Current user is null.");
            }

            // Get the user id
            var userId = _userManager.GetUserId(currentUser);

            // Check if the user id is null or empty
            if (string.IsNullOrEmpty(userId))
            {
                // Log an error if the user id is null or empty
                _logger.LogError("User id is null or empty.");
                // Redirect to an error page or return an appropriate response
                throw new Exception("User id is null or empty.");
            }

            // Find the user by user id
            var foundUser = (User)await _userManager.FindByIdAsync(userId);

            return (userId, foundUser);
        }
    }
}
