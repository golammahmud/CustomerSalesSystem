namespace CustomerSalesSystem.Application.DTOs
{
    public class CustomerPurchaseReportDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPurchaseAmount { get; set; }
    }
}
