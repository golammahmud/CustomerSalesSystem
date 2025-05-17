namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class FilterSalesQueryHandler : IRequestHandler<FilterSalesQuery, IEnumerable<SaleDto>>
    {
        private readonly ISaleReadRepository _repository;
        public FilterSalesQueryHandler(ISaleReadRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<SaleDto>> Handle(FilterSalesQuery request, CancellationToken cancellationToken)
        {
            return await _repository.FilterAsync(request.CustomerId, request.Date);
        }

    }
}
