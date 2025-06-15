using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace CustomerSalesSystem.Web.Pages.Search
{
    [IgnoreAntiforgeryToken]
    public class GlobalSearchModel(IHttpClientFactory httpClientFactory, IGlobalSearchAndChatService filterQueryService) : BasePageModel
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IGlobalSearchAndChatService _filterQueryService = filterQueryService;

        [FromForm(Name = "userQuery")]
        public string Query { get; set; } = string.Empty;
        public List<object> Results { get; set; } = new();


        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public List<CustomerDto>? Customers { get; set; }
        private HttpClient ApiClient => _httpClientFactory.CreateClient("API");


        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;

            var response = await ApiClient.GetFromJsonAsync<PagedResult<CustomerDto>>(
                $"customers?pageNumber={PageNumber}&pageSize={PageSize}");

            if (response is not null)
            {
                Customers = response.Items;
                TotalCount = response.TotalCount;
            }
        }

        public async Task<IActionResult> OnPostVoiceSearchAsync([FromForm(Name = "userQuery")] string query, [FromForm] string pagePath)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Empty query.");

            //make some correction
            string correctedQuery = TextCorrectionHelper.AutoCorrectVoiceQuery(query);

            var aiIntent = await _filterQueryService.GetFilterQueryFromOpenAPI(correctedQuery);

            if (aiIntent == null)
                return StatusCode(500, "AI failed to process the query.");


            if (aiIntent.Intent.Equals("Chat", StringComparison.OrdinalIgnoreCase))
            {
                var normalizedQuery = query?.ToLowerInvariant() ?? "";

                var appUsagePhrases = new[]
                {
        "what is this app", "what can i do", "what does this do",
        "what is this software for", "how does this app work"
    };

                var builtByPhrases = new[]
                {
        "who built this", "who made this app", "who developed this",
        "who created this", "who is the developer"
    };

                // Special case 1: App usage
                if (!string.IsNullOrWhiteSpace(aiIntent.Topic)
                    && aiIntent.Topic.Equals("AppFeatures", StringComparison.OrdinalIgnoreCase)
                    && appUsagePhrases.Any(p => normalizedQuery.Contains(p)))
                {
                    var instruction = """
You are a helpful assistant answering a user's question about what this specific web application is used for.

This app is developed by MySoft Ltd. for Beraten, and it is used to manage customers, sales. It also supports voice-based search, and Sales tracking.

Explain its purpose in a friendly, non-technical, and concise way.
""";

                    var response = await _filterQueryService.CallOpenRouterLLM(instruction);
                    return new JsonResult(new { intent = "Chat", response });
                }

                // Special case 2: Business Info
                if (!string.IsNullOrWhiteSpace(aiIntent.Topic)
                    && aiIntent.Topic.Equals("BusinessInfo", StringComparison.OrdinalIgnoreCase)
                    && builtByPhrases.Any(p => normalizedQuery.Contains(p)))
                {
                    var instruction = "Tell the user that this app was developed by Golam Mahmud from MySoft Ltd. for Beraten.";
                    var response = await _filterQueryService.CallOpenRouterLLM(instruction);
                    return new JsonResult(new { intent = "Chat", response });
                }

                // Fallback general chat
                string userMessage = aiIntent.Entities?.FirstOrDefault()?.Value ?? query;
                string topic = aiIntent.Topic ?? "Generic";

                var reply = await _filterQueryService.ChatWithAIAsync(userMessage, topic);

                return new JsonResult(new
                {
                    intent = "Chat",
                    response = reply
                });
            }


            if (aiIntent.Intent.Equals("navigate", StringComparison.OrdinalIgnoreCase))
            {
                var targetEntity = aiIntent.Entities
                    .FirstOrDefault(e => e.Field.Equals("target", StringComparison.OrdinalIgnoreCase));
                var idEntity = aiIntent.Entities
                    .FirstOrDefault(e => e.Field.Equals("id", StringComparison.OrdinalIgnoreCase));

                var rawTarget = targetEntity?.Value?.Trim() ?? "";

                // 🟢 STEP 1: Use helper to resolve the target

                var resolvedTarget = rawTarget;
                if (!NavigationAliasHelper.NavigationLinks.ContainsKey(resolvedTarget))
                {
                    resolvedTarget = NavigationAliasHelper.ResolveTarget(rawTarget);
                }

                // 🟢 STEP 2: Fallback logic if unresolved
                if (string.IsNullOrWhiteSpace(resolvedTarget) ||
                    resolvedTarget == "..." ||
                    !NavigationAliasHelper.NavigationLinks.TryGetValue(resolvedTarget, out var matchedPath))
                {
                    // Backup from original user text
                    var backupTarget = NavigationAliasHelper.ResolveTarget(correctedQuery);
                    if (!string.IsNullOrWhiteSpace(backupTarget) &&
                        NavigationAliasHelper.NavigationLinks.TryGetValue(backupTarget, out matchedPath))
                    {
                        resolvedTarget = backupTarget;

                        // Rebuild AI intent
                        aiIntent = new AIIntentResult
                        {
                            Intent = "navigate",
                            Entities = new List<AIFieldFilter>
                {
                    new()
                    {
                        Field = "target",
                        Operator = "Equals",
                        Value = resolvedTarget
                    }
                }
                        };
                    }
                    else
                    {
                        Speak("Sorry, I couldn't understand the page you're trying to go to.");
                        return new JsonResult(new { intent = "error", message = "Unknown page" });
                    }
                }

                // 🟢 STEP 3: Insert ID if required
                var navId = idEntity?.Value;
                if (matchedPath.Contains("{id}") && !string.IsNullOrEmpty(navId))
                {
                    matchedPath = matchedPath.Replace("{id}", navId);
                }

                return new JsonResult(new
                {
                    intent = "navigate",
                    target = matchedPath,
                    id = navId
                });
            }



            if (aiIntent.Intent == "fill_field")
            {
                var pageContext = GetContextFromPagePath(pagePath); // Use current path to infer context

                // OPTIONAL: Enrich field names if the AI missed the prefix
                foreach (var entity in aiIntent.Entities)
                {
                    if (!entity.Field.Contains("_") && pageContext.ModelPrefix != null)
                    {
                        entity.Field = $"{pageContext.ModelPrefix}_{entity.Field}";
                    }
                }

                return new JsonResult(new
                {
                    intent = "fill_field",
                    fields = aiIntent.Entities.Select(e => new { fieldId = e.Field, value = e.Value })
                });
            }

            return new JsonResult(new
            {
                intent = aiIntent.Intent,
                filters = aiIntent.Entities
            });
        }


        private static readonly Dictionary<string, string> PageContextMap = new()
        {
            ["/Customers/Create"] = "Page: Create Customer\nModelPrefix: Customer\nFields: Name, Email, Phone",
            ["/Customers/Edit"] = "Page: Edit Customer\nModelPrefix: Customer\nFields: Name, Email, Phone",
            ["/Sales/Create"] = "Page: Create Sale\nModelPrefix: Sale\nFields: ProductId, Quantity, TotalPrice, SaleDate"
        };

        private (string Page, string? ModelPrefix) GetContextFromPagePath(string? path)
        {
            return path switch
            {
                "/Customers/Create" or "/Customers/Edit" => ("Customer Page", "Customer"),
                "/Sales/Create" or "/Sales/Edit" => ("Sales Page", "Sale"),
                _ => ("Unknown", null)
            };
        }

        //public async Task<IActionResult> OnPostVoiceSearchAsync([FromForm(Name = "userQuery")] string query)
        //{
        //    {
        //        if (string.IsNullOrWhiteSpace(Query))
        //            return BadRequest("Empty query.");

        //        var aiIntent = await _filterQueryService.GetFilterQueryFromOpenAPI(Query);

        //        if (aiIntent == null)
        //        {
        //            Speak("AI failed to process the query.");
        //            return StatusCode(500, "AI failed to process the query.");
        //        }

        //        if (aiIntent.Intent.Equals("Chat", StringComparison.OrdinalIgnoreCase))
        //        {
        //            var message = aiIntent.Entities.FirstOrDefault()?.Value ?? "";
        //            var reply = await _filterQueryService.ChatWithAIAsync(message);

        //            return new JsonResult(new
        //            {
        //                intent = "Chat",
        //                response = reply
        //            });
        //        }

        //        return new JsonResult(new
        //        {
        //            intent = aiIntent.Intent,
        //            filters = aiIntent.Entities
        //        });
        //    }

        //    //public async Task<IActionResult> OnPostSearchAsync([FromBody] SearchRequestModel request)
        //    //{
        //    //    if (string.IsNullOrWhiteSpace(request.Query))
        //    //        return BadRequest("Query is required.");

        //    //    var aiResult = await _filterQueryService.GetFilterQueryFromOpenAPI(request.Query);

        //    //    if (aiResult == null)
        //    //        return StatusCode(500, "Failed to get AI response.");

        //    //    // Optionally you can do additional filtering or logic here

        //    //    return new JsonResult(aiResult);
        //    //}


        //}
    }
}
