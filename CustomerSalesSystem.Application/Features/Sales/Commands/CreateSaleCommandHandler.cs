namespace CustomerSalesSystem.Application.Features.Sales.Commands
{
    public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, int>
    {
        private readonly ISalesRepository _repository;

        public CreateSaleCommandHandler(ISalesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
        {
            var sale = new Sale()
            {
                CustomerId = command.CustomerId,
                ProductId = command.ProductId,
                Quantity = command.Quantity,
                SaleDate = DateTime.UtcNow,
                TotalPrice = command.TotalPrice
            };
            await _repository.AddAsync(sale);
            return sale.CustomerId;
        }


    }
}
