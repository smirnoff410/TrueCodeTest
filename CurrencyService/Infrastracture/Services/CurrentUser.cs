using CurrencyService.Application.Services;
using System.Security.Claims;

namespace CurrencyService.Infrastracture.Services
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CurrentUser> _logger;

        public CurrentUser(
            IHttpContextAccessor httpContextAccessor,
            ILogger<CurrentUser> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Guid? GetUserId()
        {
            try
            {
                var userIdClaim = GetClaimValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return null;
                }

                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    return userId;
                }

                _logger.LogWarning("Failed to parse user ID from claim: {UserIdClaim}", userIdClaim);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from JWT token");
                return null;
            }
        }

        private string? GetClaimValue(string claimType)
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        }
    }
}
