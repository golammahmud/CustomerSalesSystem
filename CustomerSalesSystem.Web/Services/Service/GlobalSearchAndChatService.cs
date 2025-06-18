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
        private readonly IAssistantService _assistantService;
        private readonly IConfiguration _config;

        public GlobalSearchAndChatService(IHttpClientFactory httpClientFactory, IConfiguration config, IAssistantService assistantService)
        {
            _httpClientFactory = httpClientFactory;
            _assistantService = assistantService;
            _config = config;
        }

        
        private const string CoreSystemPrompt = @"
You are Sensa, a friendly, patient, and intelligent female assistant designed to help users with questions, search, and navigation.  
You communicate clearly and warmly, making users feel comfortable and supported, while always staying polite, concise, and focused on the task.


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


Your task is to extract intent and filters from user queries. Analyze the query and return the result in a clean JSON format only — no extra explanation.

Respond ONLY in this JSON format:

{
  ""intent"": ""SearchCustomer"" | ""SearchProduct"" | ""SearchSales"",
  ""entities"": [
    { ""field"": ""FieldName"", ""operator"": ""Equals"", ""value"": ""FilterValue"" }
  ]
}

Guidelines:
- Do not wrap the JSON in quotes or encode it as a string.
- Return only valid JSON, no markdown, no text, no explanation.
- Ensure the `intent` starts with ""Search"" followed by the target entity.
- If the query does not match any known entity or field, return:
{
  ""intent"": ""Unknown"",
  ""entities"": []
}

Supported fields:
- Customer: Name, Email, Phone
- Product: Name, Price
- Sales: Id, Quantity, TotalPrice, SaleDate, CustomerId, ProductId, CustomerName, ProductName



You can understand flexible user queries related to Customers, Products, and Sales.

For search-related queries, always return intent like:
- ""SearchCustomer""
- ""SearchProduct""
- ""SearchSales""

Supported field filters:
- Customer: Name, Email, Phone
- Product: Name, Price
- Sales: Id, Quantity, TotalPrice, SaleDate, CustomerId, ProductId, CustomerName, ProductName



Supported operators:
- Equals → exact match
- Contains → partial match
- StartsWith → prefix match
- EndsWith → suffix match
- GreaterThan, LessThan → numeric comparison (e.g., Price > 1000)
- Between → for ranges (if user says ""between X and Y"")

Examples:

User Input: ""Find customers named Tom""
Expected Output:
{
  ""intent"": ""SearchCustomer"",
  ""entities"": [
    { ""field"": ""Name"", ""operator"": ""Equals"", ""value"": ""Tom"" }
  ]
}
1. User says: ""Search for a customer named Jemmy""
Return:
{
  ""intent"": ""SearchCustomer"",
  ""entities"": [
    { ""field"": ""Name"", ""operator"": ""Equals"", ""value"": ""Jemmy"" }
  ]
}
2.User says: ""Show all customers""
Return:
{
  ""intent"": ""SearchCustomer"",
  ""entities"": []
}
3.User says: ""Find products under 500""
Return:
{
  ""intent"": ""SearchProduct"",
  ""entities"": [
    { ""field"": ""Price"", ""operator"": ""LessThan"", ""value"": ""500"" }
  ]
}
4.User says: ""List sales where notes contain urgent""
Return:
{
  ""intent"": ""SearchSales"",
  ""entities"": [
    { ""field"": ""Notes"", ""operator"": ""Contains"", ""value"": ""urgent"" }
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
               
                var messages = new List<object>
                {
                     new { role = "system", content = PromptFactory.GetPromptforSensa },
                     new { role = "system", content = CoreSystemPrompt },
                     new { role = "system", content = AutoCorrectionPrompt },
                     new { role = "system", content = Fill_field },
                     new { role = "user", content = userQuery }
                };

                var content = await _assistantService.GetAIResponseTextAsync(messages, temperature: 0.3); // low randomness

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
               

                string prompt = PromptFactory.GetPrompt(topic, userMessage);


                //var requestBody = new
                //{
                //    model = selectedModel,
                //    temperature = 0.5, // or make it configurable
                //    messages = new[]
                //    {
                //    new { role = "system", content = prompt },
                //    new { role = "system", content = AutoCorrectionPrompt },
                //    new { role = "user", content = userMessage }
                //}
                //};

                var messages = new List<object>
                                {
                                    new { role = "system", content = PromptFactory.GetPromptforSensa },
                                    new { role = "system", content = prompt },
                                    new { role = "system", content = AutoCorrectionPrompt },
                                    new { role = "user", content = userMessage }
                                };

                var result = await _assistantService.GetAIResponseTextAsync(messages, temperature: 0.5); // low randomness

                return result ?? "Sorry, I didn't get that.";
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
               

                var messages = new List<object>
                                {
                                     new { role = "system", content = PromptFactory.GetPromptforSensa },
                                     new { role = "system", content = "You are a helpful assistant providing concise answers for user FAQs in a polite and friendly tone." },
                                     new { role = "user", content = instruction }
                                };

                var result = await _assistantService.GetAIResponseTextAsync(messages, model: "gpt-3.5-turbo", temperature: 0.4);

                // Fallback if content is empty or null
                if (string.IsNullOrWhiteSpace(result))
                {
                    var fallback = await ChatWithAIAsync(instruction, "Generic");
                    return $"[Fallback AI] {fallback}";
                }

                return result;
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
