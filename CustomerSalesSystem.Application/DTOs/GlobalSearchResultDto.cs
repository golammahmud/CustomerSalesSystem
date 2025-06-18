namespace CustomerSalesSystem.Application.DTOs
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; } = default!;       // "Customer", "Sale", etc.
        public int Id { get; set; }             // The actual record ID
        public string Name { get; set; } = default!;        // Primary display name
        public string Description { get; set; } = default!; // Optional - extra info
        public string TargetPage { get; set; } = default!; // Page to navigate to
    } 
}
