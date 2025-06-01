using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Services.IService
{
    public interface IGlobalSearchAndChatService
    {
        Task<AIIntentResult?> GetFilterQueryFromOpenAPI(string userQuery, string? customPrompt = null);
        Task<string> ChatWithAIAsync(string userMessage, string? customPrompt = null);
    }
}
