using CustomerSalesSystem.Application.DTOs;
using Polly;
using Polly.Retry;
using System.Net.Http.Headers;

namespace CustomerSalesSystem.Web.Helper
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly SemaphoreSlim _refreshLock = new(1, 1);
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public TokenRefreshHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                .RetryAsync(1);
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Attach current access token from cookie
            await AttachAccessTokenFromCookie(request);

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await _refreshLock.WaitAsync(cancellationToken);
                    try
                    {
                        // Check if token was refreshed by another request
                        var currentAccessToken = GetAccessTokenFromCookie();
                        if (!string.IsNullOrEmpty(currentAccessToken) &&
                            request.Headers.Authorization?.Parameter != currentAccessToken)
                        {
                            response.Dispose();
                            AttachTokenToRequest(request, currentAccessToken);
                            return await base.SendAsync(request, cancellationToken);
                        }

                        // Attempt refresh
                        if (await RefreshTokenAsync())
                        {
                            var newAccessToken = GetAccessTokenFromCookie();
                            response.Dispose();
                            AttachTokenToRequest(request, newAccessToken);
                            return await base.SendAsync(request, cancellationToken);
                        }
                    }
                    finally
                    {
                        _refreshLock.Release();
                    }
                }

                return response;
            });
        }

        private async Task AttachAccessTokenFromCookie(HttpRequestMessage request)
        {
            var accessToken = GetAccessTokenFromCookie();
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        private string GetAccessTokenFromCookie()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["access_token"];
        }

        private string GetRefreshTokenFromCookie()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["refresh_token"];
        }

        private void AttachTokenToRequest(HttpRequestMessage request, string token)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var refreshToken = GetRefreshTokenFromCookie();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var client = _httpClientFactory.CreateClient("AuthClient");
            var response = await client.PostAsJsonAsync("api/auth/refresh", new { refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            var tokenResult = await response.Content.ReadFromJsonAsync<LoginResponseDTO>();
            if (tokenResult == null) return false;

            // Set new tokens as HTTP-only cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(15) // Match token expiry
            };

            _httpContextAccessor.HttpContext?.Response.Cookies.Append("access_token", tokenResult.AccessToken, cookieOptions);

            // Refresh token typically has longer expiry
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/api/auth" // Limit to refresh endpoint
            };
            _httpContextAccessor.HttpContext?.Response.Cookies.Append("refresh_token", tokenResult.RefreshToken, refreshCookieOptions);

            return true;
        }
    }
}