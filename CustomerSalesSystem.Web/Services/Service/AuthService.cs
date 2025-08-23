using CustomerSalesSystem.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class AuthService : BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CookieOptions _accessTokenCookieOptions;
        private readonly CookieOptions _refreshTokenCookieOptions;

        public AuthService(IHttpContextAccessor httpContextAccessor, IHttpClientFactory factory)
            : base(httpContextAccessor, factory)
        {
            _httpContextAccessor = httpContextAccessor;

            // Configure cookie options
            _accessTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,// Prevent CSRF attacks
                Expires = DateTime.UtcNow.AddMinutes(15) // Match access token expiry
            };

            _refreshTokenCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,// Prevent CSRF attacks
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/user/refresh" // Only sent to refresh endpoint
            };
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await Client.PostAsJsonAsync("user/login", request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedAccessException("Invalid credentials");
                    }
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                if (result != null)
                {
                    SetAuthCookies(result.AccessToken, result.RefreshToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                throw; // Re-throw for controller handling
            }
        }

        public async Task<LoginResponseDTO?> RegisterAsync(RegisterRequestDTO request)
        {
            try
            {
                var response = await Client.PostAsJsonAsync("user/register", request);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.Conflict)
                    {
                        throw new InvalidOperationException("Username already exists");
                    }
                    return null;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
                if (result != null)
                {
                    SetAuthCookies(result.AccessToken, result.RefreshToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                throw;
            }
        }

        public void Logout()
        {
            ClearAuthCookies();
        }

        private void SetAuthCookies(string accessToken, string refreshToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Append("access_token", accessToken, _accessTokenCookieOptions);
            httpContext.Response.Cookies.Append("refresh_token", refreshToken, _refreshTokenCookieOptions);
        }

        private void ClearAuthCookies()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            httpContext.Response.Cookies.Delete("access_token");
            httpContext.Response.Cookies.Delete("refresh_token");
        }

        public string? GetAccessToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        }

        public string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        }
    }
}