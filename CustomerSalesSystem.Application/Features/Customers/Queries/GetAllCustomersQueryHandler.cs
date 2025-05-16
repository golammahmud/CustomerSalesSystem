using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Interfaces.Repositories;
using MediatR;

namespace CustomerSalesSystem.Application.Features.Customers.Queries
{
    public class GetAllCustomersQueryHandler : IRequestHandler<GetAllCustomersQuery, IEnumerable<CustomerDto>>
    {
        private readonly ICustomerReadRepository _repository;

        public GetAllCustomersQueryHandler(ICustomerReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CustomerDto>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync();
        }
    }
}
