using CustomerSalesSystem.Application.Features.Customers.Commands;
using MediatR;

namespace CustomerSalesSystem.Application.Features.Sales.Commands
{
    public class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand, Unit>
    {
        readonly ISalesRepository _repository;
        public DeleteSaleCommandHandler(ISalesRepository salesRepository)
        {
            _repository = salesRepository;
        }
        //Unit is a special type from MediatR that represents a "void" response in asynchronous operations.
        public async Task<Unit> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
        {
            var sale = await _repository.GetByIdAsync(request.Id);
            if (sale == null)
                throw new KeyNotFoundException($"Sale with Id = {request.Id} not found.");

            await _repository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
