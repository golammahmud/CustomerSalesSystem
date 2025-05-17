namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class GetSaleByIdQuery : IRequest<SaleDto>
    {
        public int Id { get; set; }
    }
}
