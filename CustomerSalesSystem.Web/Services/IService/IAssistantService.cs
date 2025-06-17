namespace CustomerSalesSystem.Web.Services.IService
{
    public interface IAssistantService
    {
        Task<string> GetAIResponseTextAsync(List<object> messages, string? model = null, double temperature = 0.3);
    }

}
