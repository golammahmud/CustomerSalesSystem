using CustomerSalesSystem.Application.DTOs;
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

        public async Task<IActionResult> OnPostVoiceSearchAsync([FromForm(Name = "userQuery")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Empty query.");

            var aiIntent = await _filterQueryService.GetFilterQueryFromOpenAPI(query);

            if (aiIntent == null)
                return StatusCode(500, "AI failed to process the query.");

            if (aiIntent.Intent.Equals("Chat", StringComparison.OrdinalIgnoreCase))
            {
                var message = aiIntent.Entities.FirstOrDefault()?.Value ?? "";
                var reply = await _filterQueryService.ChatWithAIAsync(message);

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

                var navTarget = targetEntity?.Value ?? "";
                var navId = idEntity?.Value;

                if (!NavigationLinks.TryGetValue(navTarget, out var matchedPath))
                {
                    Speak("Sorry, I couldn't understand the page you're trying to go to.");
                    return new JsonResult(new { intent = "error", message = "Unknown page" });
                }

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

            return new JsonResult(new
            {
                intent = aiIntent.Intent,
                filters = aiIntent.Entities
            });
        }

        private static readonly Dictionary<string, string> NavigationLinks = new()
        {
            ["Customer List"] = "/Customers/Index",
            ["EditCustomer"] = "/Customers/Edit?id={id}",
            ["CreateCustomer"] = "/Customers/Create",
            ["Sales List"] = "/Sales/Index",
            ["EditSale"] = "/Sales/Edit?id={id}",
            ["CreateSale"] = "/Sales/Create",
            ["Global Search"] = "/Search/GlobalSearch"
        };


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
