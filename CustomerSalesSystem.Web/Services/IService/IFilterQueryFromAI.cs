using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Services
{
    public interface IFilterQueryFromAIService
    {
        Task<AIQueryResult?> GetFilterQueryFromOpenAPI(string userQuery, string? customPrompt = null);
    }
}
