namespace CustomerSalesSystem.Application.Features.Customers.Queries
{
    public class GetAllFilterCustomersQueryHandler : IRequestHandler<GetAllFilterCustomersQuery, PagedResult<CustomerDto>>
    {
        private readonly ICustomerReadRepository _repository;

        public GetAllFilterCustomersQueryHandler(ICustomerReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<CustomerDto>> Handle(GetAllFilterCustomersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.SearchWithFiltersAsync(request.Filters, request.PageNumber, request.PageSize);
        }
    }

}
