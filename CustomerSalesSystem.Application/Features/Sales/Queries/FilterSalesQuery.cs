namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class FilterSalesQuery : IRequest<IEnumerable<SaleDto>>
    {
        public int? CustomerId { get; set; }
        public DateTime? Date { get; set; }
    }
}
