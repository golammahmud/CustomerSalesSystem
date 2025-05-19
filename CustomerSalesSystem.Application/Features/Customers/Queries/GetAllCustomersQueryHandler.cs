namespace CustomerSalesSystem.Application.Features.Customers.Queries
{
    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, PagedResult<CustomerDto>>
    {
        private readonly ICustomerReadRepository _repository;

        public GetAllCustomersQueryHandler(ICustomerReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync(request.PageNumber, request.PageSize);
        }
    }
}
