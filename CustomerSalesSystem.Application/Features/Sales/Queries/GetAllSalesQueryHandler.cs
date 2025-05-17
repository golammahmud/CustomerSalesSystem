namespace CustomerSalesSystem.Application.Features.Sales.Queries
{
    public class GetAllSalesQueryHandler:IRequestHandler<GetAllSalesQuery, IEnumerable<SaleDto>>
    {
        private readonly ISaleReadRepository _repository;
        public GetAllSalesQueryHandler(ISaleReadRepository saleReadRepository)
        {
            _repository = saleReadRepository;
        }
        public async Task<IEnumerable<SaleDto>> Handle(GetAllSalesQuery request,CancellationToken cancellationToken)
        {
            return await _repository.GetAllAsync();
        }
    }
}
