using CustomerSalesSystem.Web.Services.IService;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class AssistantService : IAssistantService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AssistantService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GetAIResponseTextAsync(List<object> messages, string? model = null, double temperature = 0.3)
        {
            var apiKey = _config["OpenRouter:ApiKey"];
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
            client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

            var selectedModel = model ?? "gpt-3.5-turbo";

            var requestBody = new
            {
                model = selectedModel,
                temperature = temperature,
                messages = messages
            };

            var response = await client.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception("Failed to get response from OpenRouter API. " + error);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);

            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? "No response from assistant.";
        }


        private const string DefaultSystemPrompt = "You are an intelligent assistant that helps with voice search.";
    }

}
