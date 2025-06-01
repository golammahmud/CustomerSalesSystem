using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Services.IService;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class GlobalSearchAndChatService:IGlobalSearchAndChatService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public GlobalSearchAndChatService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        //        private const string DefaultSystemPrompt = @"
        //You are an assistant that classifies user queries as either structured search or general chat.

        //The system supports three searchable tables: Customers, Products, and Sales.

        //Customer:
        //- Id (int)
        //- Name (string)
        //- Email (string)
        //- Phone (string)

        //Product:
        //- Id (int)
        //- Name (string)
        //- Price (decimal)

        //Sale:
        //- Id (int)
        //- CustomerId (int)
        //- ProductId (int)
        //- Quantity (int)
        //- TotalPrice (decimal)
        //- SaleDate (datetime)

        //Relationships:
        //- A Customer can have many Sales.
        //- A Product can be sold in many Sales.
        //- A Sale links a Customer and a Product.

        //Your job is to output a structured JSON response with this format:

        //{
        //  ""intent"": ""SearchCustomer"" | ""SearchProduct"" | ""SearchSales"" | ""Chat"",
        //  ""entities"": [
        //    { ""field"": ""FieldName"", ""operator"": ""OperatorName"", ""value"": ""FilterValue"" }
        //  ]
        //}

        //Rules:
        //- If the user asks a general question not about Customers, Products, or Sales, set ""intent"": ""Chat"".
        //- If ""intent"": ""Chat"", set a single entity with field=""message"", operator=""Contains"", and value=actual message.
        //- For search intents, determine the correct table and return relevant filters.
        //- Supported operators: Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, etc.
        //- For string fields like Name/Email/Phone, use ""Contains"" unless explicitly stated otherwise.
        //- For numeric or date fields, use comparison operators based on user phrasing.
        //- Respond with raw JSON only. No explanation or extra text.
        //";

        private const string DefaultSystemPrompt = @"
You are an assistant that processes user queries and classifies them into one of several intents: Search, Navigate, Refresh, FillField, or Chat.

Supported Intents:
- ""SearchCustomer"" | ""SearchProduct"" | ""SearchSales""
- ""Navigate""
- ""Refresh""
- ""FillField""
- ""Chat""

The system supports three searchable tables: Customers, Products, and Sales.

Customer:
- Id (int)
- Name (string)
- Email (string)
- Phone (string)

Product:
- Id (int)
- Name (string)
- Price (decimal)

Sale:
- Id (int)
- CustomerId (int)
- ProductId (int)
- Quantity (int)
- TotalPrice (decimal)
- SaleDate (datetime)

Relationships:
- A Customer can have many Sales.
- A Product can be sold in many Sales.
- A Sale links a Customer and a Product.

### Expected Response Format:
Always return JSON in this structure:

For search:
{
  ""intent"": ""SearchCustomer"" | ""SearchProduct"" | ""SearchSales"",
  ""entities"": [
    { ""field"": ""FieldName"", ""operator"": ""OperatorName"", ""value"": ""FilterValue"" }
  ]
}

For navigation:
1. Navigate the user to various pages using target links.
2. Identify navigation intent from phrases like: ""go to"", ""navigate to"", ""open"", ""take me to"", ""show me"", ""edit"", ""create"", ""redirect"", etc.
3. Match user phrases to the closest available page names using synonyms and common patterns.

Available Navigation Targets:
{
  ""Customer List"": ""/Customers/Index"",
  ""Sales List"": ""/Sales/Index"",
  ""Home"": ""/"",
  ""Search"": ""/Search/Index"",
  ""CreateCustomer"": ""/Customers/Create"",
  ""EditCustomer"": ""/Customers/Edit?id={id}"",
  ""CreateSale"": ""/Sales/Create"",
  ""EditSale"": ""/Sales/Edit?id={id}""
}

Expected input examples and behavior:
- ""Go to customer list"" → { ""intent"": ""navigate"", ""target"": ""Customer List"" }
- ""Open sales page"" → { ""intent"": ""navigate"", ""target"": ""Sales List"" }
- ""Take me to homepage"" → { ""intent"": ""navigate"", ""target"": ""Home"" }
- ""Create new customer"" → { ""intent"": ""navigate"", ""target"": ""CreateCustomer"" }
- ""Edit sale with ID 3"" → { ""intent"": ""navigate"", ""target"": ""EditSale"", ""id"": 3 }
- ""Navigate to edit customer 12"" → { ""intent"": ""navigate"", ""target"": ""EditCustomer"", ""id"": 12 }

Rules:
- The intent must be `""navigate""` for navigation actions.
- Use the closest matching `""target""` from the list.
- Include `""id""` if user query contains an ID and the target path contains `{id}`.
- Do not return explanations or text. Only output raw JSON in the following format:

{
  ""intent"": ""navigate"",
  ""entities"": [
    { ""field"": ""target"", ""operator"": ""Equals"", ""value"": ""Customer List"" },
    { ""field"": ""id"", ""operator"": ""Equals"", ""value"": ""5"" }
  ]
}





For refresh:
{
  ""intent"": ""Refresh""
}

For filling a field:
{
  ""intent"": ""FillField"",
  ""fieldId"": ""FieldElementId"",
  ""value"": ""User's provided input""
}

For chat/general conversation:
{
  ""intent"": ""Chat"",
  ""entities"": [
    { ""field"": ""message"", ""operator"": ""Contains"", ""value"": ""User's message"" }
  ]
}

### Rules:
- Use ""intent"": ""Chat"" only when it's not about navigation, search, refreshing, or filling a form.
- If the user says things like ""go to"", ""open"", ""navigate"", or mentions a page name, return intent ""Navigate"" with a target matching one of the known page names.
- For refresh commands like ""reload page"" or ""refresh this"", use intent ""Refresh"".
- For voice input that sets values in a form, return intent ""FillField"".
- Use exact names for page targets like ""Customer List"", ""Sales List"", ""Global Search"", ""Create Customer"", etc.

Respond with raw JSON only. No explanation or extra text.
";


        public async Task<AIIntentResult?> GetFilterQueryFromOpenAPI(string userQuery, string? customPrompt = null)
        {
            try
            {
                var apiKey = _config["OpenRouter:ApiKey"];
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

                string selectedModel;

                if (userQuery.Length > 100 || userQuery.Contains("complex", StringComparison.OrdinalIgnoreCase))
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
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // Deserialize JSON filter result for global search
                AIIntentResult? result = null;
                try
                {
                    result = JsonSerializer.Deserialize<AIIntentResult>(content);
                }
                catch (JsonException)
                {
                    // Try parse nested JSON string
                    var unescaped = JsonDocument.Parse(content).RootElement.GetRawText();
                    result = JsonSerializer.Deserialize<AIIntentResult>(unescaped);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred in GetFilterQueryFromOpenAPI: " + ex.Message);
            }
        }

        /// <summary>
        /// Generic Chat method to get AI's conversational response as plain text
        /// </summary>
        public async Task<string> ChatWithAIAsync(string userMessage, string? customPrompt = null)
        {
            try
            {
                var apiKey = _config["OpenRouter:ApiKey"];
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

                string selectedModel = userMessage.Length > 100 || userMessage.Contains("complex", StringComparison.OrdinalIgnoreCase)
                    ? "gpt-4-turbo"
                    : "gpt-3.5-turbo";

                var requestBody = new
                {
                    model = selectedModel,
                    messages = new[]
                    {
                    new { role = "system", content = customPrompt ?? "You are a helpful assistant." },
                    new { role = "user", content = userMessage }
                }
                };

                var response = await client.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return content ?? "Sorry, I didn't get that.";
            }
            catch (Exception ex)
            {
                return "Error in chat service: " + ex.Message;
            }
        }

        public static readonly Dictionary<string, string> NavigationLinks = new()
        {
            ["Customer List"] = "/Customers/Index",
            ["Sales List"] = "/Sales/Index",
            ["Create Customer"] = "/Customers/Create",
            ["Global Search"] = "/Search/GlobalSearch"
        };
    }
}
