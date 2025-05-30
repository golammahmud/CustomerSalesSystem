namespace CustomerSalesSystem.Application.DTOs
{
    public class CustomerSearchRequest
    {
        public List<AIFieldFilter> Filters { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

}
