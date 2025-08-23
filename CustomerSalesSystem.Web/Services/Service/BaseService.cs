using System.Net.Http.Headers;

namespace CustomerSalesSystem.Web.Services.Service
{
    using System.Net.Http.Headers;

    public abstract class BaseService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        protected BaseService(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }


        // Always returns a configured client with JWT (if available)
        protected HttpClient Client => _httpClientFactory.CreateClient("API");

        //protected HttpClient Client
        //{
        //    get
        //    {
        //        var client = _httpClientFactory.CreateClient("API");
        //        var accessToken = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");

        //        if (!string.IsNullOrEmpty(accessToken))
        //        {
        //            client.DefaultRequestHeaders.Authorization =
        //                new AuthenticationHeaderValue("Bearer", accessToken);
        //        }

        //        return client;
        //    }
        //}
    }


}
