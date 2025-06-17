namespace CustomerSalesSystem.Application.DTOs
{
    public class GlobalSearchResultDto
    {
        public string Type { get; set; }        // "Customer", "Sale", etc.
        public int Id { get; set; }             // The actual record ID
        public string Name { get; set; }        // Primary display name
        public string Description { get; set; } // Optional - extra info
        public string TargetPage { get; set; }  // Page to navigate to
    }

}
