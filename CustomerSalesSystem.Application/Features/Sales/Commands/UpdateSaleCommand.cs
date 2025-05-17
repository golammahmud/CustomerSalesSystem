namespace CustomerSalesSystem.Application.Features.Sales.Commands
{
    public class UpdateSaleCommand:IRequest<bool>
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime SaleDate { get; set; }
    }
}
