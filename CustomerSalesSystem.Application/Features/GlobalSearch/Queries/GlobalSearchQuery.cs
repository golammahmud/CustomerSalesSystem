namespace CustomerSalesSystem.Application.Features
{
    public class GlobalSearchQuery : IRequest<List<GlobalSearchResultDto>>
    {
        public string Intent { get; set; }           // Example: "SearchCustomer", "SearchProduct"
        public List<AIFieldFilter> Entities { get; set; } = new();  // Structured filters
    }

}
