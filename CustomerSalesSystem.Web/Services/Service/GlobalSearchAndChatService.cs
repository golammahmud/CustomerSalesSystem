using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services.IService;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class GlobalSearchAndChatService : IGlobalSearchAndChatService
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

        private const string CoreSystemPrompt = @"
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

You are an intelligent assistant helping users navigate a web application.

Your job is to return structured JSON output to identify the correct navigation target and optional ID from the user's voice or text query.

---

Common navigation verbs include:
""go to"", ""navigate to"", ""open"", ""take me to"", ""show me"", ""edit"", ""create"", ""add"", ""start"", ""change"", ""update"", ""redirect"", ""move to"", ""switch to"", ""jump to"", ""head to""

Common mishearing corrections:
""cell"" → ""sale""  
""client"" or ""costumer"" → ""customer""  
""pace"" → ""page""  
""sells"" → ""sales""  
""edit sail"" or ""edit cell"" → ""edit sale""  
""crete"" or ""creat"" → ""create""  
""main page"" → ""home""  
""add new"" → ""create""  

---

Match the user request to the following known pages (use **only these exact values** as `value` in the JSON output):

- ""Customer List"" → ""/Customers/Index""
- ""Sales List"" → ""/Sales/Index""
- ""Home"" → ""/""
- ""Search"" → ""/Search/Index""
- ""CreateCustomer"" → ""/Customers/Create""
- ""EditCustomer"" → ""/Customers/Edit?id={id}""
- ""CreateSale"" → ""/Sales/Create""
- ""EditSale"" → ""/Sales/Edit?id={id}""

---

If the user mentions a number with ""edit"" (e.g., ""edit customer 5""), extract it as `""id""`.

---

### OUTPUT RULES:
- Return only **valid pure JSON** (no markdown, no explanation, no extra text).
- If there's no ID, do not include the `""id""` entity.
- Ensure all `value` fields are actual strings, **never empty or placeholder** values like `""...""`.

---

### ✅ Output Examples:

If user says:  
**""Go to the sales page""**  
Return:
json
{
  ""intent"": ""navigate"",
  ""entities"": [
    { ""field"": ""target"", ""operator"": ""Equals"", ""value"": ""Sales List"" }
  ]
}

if user says:
**""Edit customer 12""**  
Return:
{
  ""intent"": ""navigate"",
  ""entities"": [
    { ""field"": ""target"", ""operator"": ""Equals"", ""value"": ""EditCustomer"" },
    { ""field"": ""id"", ""operator"": ""Equals"", ""value"": ""12"" }
  ]
}


If the intent is ""Chat"", classify it into a topic.

Supported Chat Topics:
- ""AppFeatures"" → if the user asks about app functionality, usage, how it works, what it can do
- ""BusinessInfo"" → if the user asks about company, support, contact details, or business-related info
- ""ChitChat"" → if the user is being casual, joking, greeting, or having general conversation
- ""Unknown"" → if none of the above apply

When intent is ""Chat"", respond with:

{
  ""intent"": ""Chat"",
  ""topic"": ""AppFeatures"" | ""BusinessInfo"" | ""ChitChat"" | ""Unknown"",
  ""entities"": []
}




For refresh:
{
  ""intent"": ""Refresh""
}


### Rules:
- Use ""intent"": ""Chat"" only when it's not about navigation, search, refreshing, or filling a form.
- If the user says things like ""go to"", ""open"", ""navigate"", or mentions a page name, return intent ""Navigate"" with a target matching one of the known page names.
- For refresh commands like ""reload page"" or ""refresh this"", use intent ""Refresh"".
- For voice input that sets values in a form, return intent ""FillField"".
- Use exact names from the Navigation Targets list such as ""Customer List"", ""Sales List"", ""CreateCustomer"", ""CreateSale"", etc.
Respond with raw JSON only. No explanation or extra text.
";

        private const string ApplicationHelperPrompt = @"
You are a smart in-app voice assistant.

Your primary role is to help users interact with this web application, which manages Customers, Products, and Sales.

---

🧠 Be Context-Aware:
- If the user's question is **about the application** — like:
  - “What can I do here?”
  - “What is this app for?”
  - “Tell me about this app”
  - “What are the features?”

✅ Then respond like this:
> This web app helps you manage Customers, Products, and Sales efficiently.

> You can:
> - 🔍 Use voice/text search to find customers, products, or sales
> - 🧭 Navigate to different pages using voice (e.g., “Go to Sales page”)
> - 📝 Fill out forms using voice dictation (e.g., “Name is John Doe”)
> - ♻️ Refresh pages or get help on demand

> Try saying: “Search for iPhone”, “Open Customer page”, or “Refresh”.

---

❌ Do NOT say this for every question.

💬 If the user's question is **not about the app**, like:
- “What is a neural network?”
- “How does SQL indexing work?”

✅ Then reply **normally** and **do not** mention the application at all.

---

🎯 Summary Rules:
- Prioritize app help **only** when app-related keywords are detected.
- Otherwise, behave like a general helpful assistant.
- Keep your tone friendly, helpful, and voice-assistant ready.
";
        private const string IntentClassificationPrompt = @"
You are an AI assistant for a web application.

Given the user query below, identify:

- intent: One of [""Search"", ""Navigate"", ""FillField"", ""Chat""]
- topic: Only if intent is ""Chat"", classify into one of:
  - ""AppFeatures"" (if asking about what the app can do, how it works, functionality)
  - ""BusinessInfo"" (asking about company, contact, hours, support)
  - ""ChitChat"" (personal greetings, jokes, general conversation)
  - ""Unknown""

Respond in JSON:
{
  ""intent"": ""..."",
  ""topic"": ""..."",
  ""entities"": [ { ""field"": ""..."", ""value"": ""..."" } ]
}

User Query:
""{userQuery}""
";


        private const string AutoCorrectionPrompt = @"
If the user input contains common mispronunciations, spelling mistakes, or voice-to-text errors, you may correct them automatically while preserving meaning.

Example:
- “fined costomer from daka” ➜ “Find customer from Dhaka”
- “show me prodcut i phone” ➜ “Show me product iPhone”

Keep corrections minimal and helpful. If you're unsure, ask the user to confirm.";
        private const string Fill_field = @"You are a voice assistant that fills form fields on a web page.

Context:
- The page you are helping with is: {PageName} (e.g., ""Customer Create Page"" or ""Sales Edit Page"").
- On this page, input field IDs are usually auto-generated by Razor as: ""{ModelPrefix}_{FieldName}"".
  For example, on the Customer Create page, the field ""Name"" has ID ""Customer_Name"".
- The ModelPrefix for this page is: ""{ModelPrefix}"" (e.g., ""Customer"" or ""Sale"").

Task:
- Extract the field to fill and the value from the user's voice command.
- Return a JSON with the format:
  {
    ""intent"": ""fill_field"",
    ""entities"": [
      { ""field"": ""{FullFieldId}"", ""value"": ""{ValueToFill}"" }
    ]
  }
- If the user says ""set name to Mr. Oggy Man"" on the Customer Create page,
  the field should be returned as ""Customer_Name"".
- Always use the full field ID including prefix.

Rules:
- The ""field"" must match the input field's HTML ID exactly.
- The ""value"" must be the exact value to fill into that input.
- If no explicit ID prefix is mentioned in the command, use the ModelPrefix + ""_"" + FieldName.

Return only raw JSON. No explanation or extra text.
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

               
                if (userQuery.Length > 150 || userQuery.Contains("complex", StringComparison.OrdinalIgnoreCase))
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
                    temperature = 0.3, // lower temp = more deterministic output
                    messages = new[]
                    {
                    new { role = "system", content = CoreSystemPrompt },
                     new { role = "system", content = AutoCorrectionPrompt },
                    new { role = "system", content = Fill_field },
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
        public async Task<string> ChatWithAIAsync(string userMessage, string topic = "Generic", string? customPrompt = null)
        {
            try
            {
                var apiKey = _config["OpenRouter:ApiKey"];
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

                string selectedModel = userMessage.Length > 150 || userMessage.Contains("complex", StringComparison.OrdinalIgnoreCase)
                    ? "gpt-4-turbo"
                    : "gpt-3.5-turbo";


                string prompt = PromptFactory.GetPrompt(topic, userMessage);


                var requestBody = new
                {
                    model = selectedModel,
                    temperature = 0.5, // or make it configurable
                    messages = new[]
                    {
                    new { role = "system", content = prompt },
                    new { role = "system", content = AutoCorrectionPrompt },
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

        public async Task<string> CallOpenRouterLLM(string instruction)
        {
            try
            {
                var apiKey = _config["OpenRouter:ApiKey"];
                var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                client.DefaultRequestHeaders.Add("X-Title", "VoiceSearchApp");

                string selectedModel = "gpt-3.5-turbo";

                var requestBody = new
                {
                    model = selectedModel,
                    temperature = 0.4,
                    messages = new[]
                    {
                new { role = "system", content = "You are a helpful assistant providing concise answers for user FAQs in a polite and friendly tone." },
                new { role = "user", content = instruction }
            }
                };

                var response = await client.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    // Handle fallback if OpenRouter fails (like 402 or 429)
                    var fallback = await ChatWithAIAsync(instruction, "Generic");
                    return $"[Fallback AI] {fallback}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);

                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                // Fallback if content is empty or null
                if (string.IsNullOrWhiteSpace(content))
                {
                    var fallback = await ChatWithAIAsync(instruction, "Generic");
                    return $"[Fallback AI] {fallback}";
                }

                return content;
            }
            catch (Exception ex)
            {
                // Catch any exception and fallback
                var fallback = await ChatWithAIAsync(instruction, "Generic");
                return $"[Fallback AI due to exception: {ex.Message}] {fallback}";
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
