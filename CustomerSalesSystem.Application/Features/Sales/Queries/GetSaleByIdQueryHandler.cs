using CustomerSalesSystem.Application.Features.Customers.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class GetSaleByIdQueryHandler: IRequestHandler<GetSaleByIdQuery, SaleDto?>
    {
        private readonly ISaleReadRepository _repository;

        public GetSaleByIdQueryHandler(ISaleReadRepository repository)
        {
            _repository = repository;
        }

        public async Task<SaleDto?> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
        {
            return await _repository.GetByIdAsync(request.Id);
        }
    }
}
