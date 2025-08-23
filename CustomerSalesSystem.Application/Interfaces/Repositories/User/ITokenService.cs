using System.Security.Claims;

namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
    }

}
