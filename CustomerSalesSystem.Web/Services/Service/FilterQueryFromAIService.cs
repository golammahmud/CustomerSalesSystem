using CustomerSalesSystem.Application.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CustomerSalesSystem.Web.Services
{
    public class FilterQueryFromAIService : IFilterQueryFromAIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public FilterQueryFromAIService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        private const string DefaultSystemPrompt = @"
You are Sensa, a kind, helpful, and intelligent female assistant designed to help users queries to search customers in a database. You always stay polite, concise, and focused on the task.

You are an assistant that converts natural language search queries into structured JSON filters for querying a database.

The database has three main tables: Customers, Products, and Sales.

Table: Customer
- Id (int)
- Name (string)
- Email (string)
- Phone (string)

Table: Product
- Id (int)
- Name (string)
- Price (decimal)

Table: Sale
- Id (int)
- CustomerId (int, FK to Customer)
- ProductId (int, FK to Product)
- Quantity (int)
- TotalPrice (decimal)
- SaleDate (datetime)

Relationships:
- A Customer can have many Sales.
- A Product can be sold in many Sales.
- A Sale links a Customer and a Product.

Your job is to convert the user's query into a JSON object with filters in the following format:

{
  ""filters"": [
    { ""field"": ""FieldName"", ""operator"": ""OperatorName"", ""value"": ""FilterValue"" }
  ]
}

Supported operators include:
- Equals
- NotEquals
- Contains
- StartsWith
- EndsWith
- GreaterThan
- GreaterThanOrEqual
- LessThan
- LessThanOrEqual

Rules:
- Always use the above JSON structure.
- The 'filters' array must contain one or more objects with 'field', 'operator', and 'value'.
- For string fields like Name, Email, or Phone:
  - **Default to 'Contains'** for user queries like 'name is Jerry', 'email is gmail.com', or 'phone is 123'.
  - Only use 'Equals' if the user explicitly says: 'equals', 'equal to', 'exactly matches', or similar wording.
- For numeric or date fields, use comparison operators when user intent suggests it (e.g., greater than, less than).
- Do not include any explanation. Respond with JSON only.
";


        public async Task<AIQueryResult?> GetFilterQueryFromOpenAPI(string userQuery, string? customPrompt = null)
        {
            try
            {
                var apiKey = _config["OpenRouter:ApiKey"];
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

                string selectedModel;

                if (userQuery.Length > 500 || userQuery.Contains("complex", StringComparison.OrdinalIgnoreCase))
                {
                    selectedModel = "gpt-4-turbo";
                }
                else
                {
                    selectedModel = "gpt-3.5-turbo";
                }


                var requestBody = new
                {
                    model = selectedModel,
                    messages = new[]
                    {
                    new { role = "system", content = customPrompt ?? DefaultSystemPrompt },
                    new { role = "user", content = userQuery }
                }
                };

                var response = await client.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    //_logger.LogError("API Error: {Error}", error);
                    throw new Exception("Failed to get response from OpenRouter API.");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // If content is a JSON string inside a string, you need to parse it again:
                AIQueryResult? result = null;
                try
                {
                    result = JsonSerializer.Deserialize<AIQueryResult>(content);
                }
                catch (JsonException)
                {
                    // Try to parse it as a nested JSON string
                    var unescaped = JsonDocument.Parse(content).RootElement.GetRawText();
                    result = JsonSerializer.Deserialize<AIQueryResult>(unescaped);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred in GetFilterQueryFromOpenAPI" + ex.Message);
            }
        }
    }

    //public class AIQueryResult
    //{
    //    public string Table { get; set; } = string.Empty;
    //    public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
    //}

    //public class AIQueryResult
    //{
    //    [JsonPropertyName("filters")]
    //    public List<AIFieldFilter> Filters { get; set; } = new();
    //}


}
