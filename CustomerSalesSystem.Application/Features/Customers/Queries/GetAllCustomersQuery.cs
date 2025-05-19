namespace CustomerSalesSystem.Application.Features.Customers.Queries
{
    public class GetAllCustomersQuery : IRequest<PagedResult<CustomerDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
