using CustomerSalesSystem.Domain.Interfaces;
using MediatR;

namespace CustomerSalesSystem.Application.Features.Products.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
    {
        private readonly IProductRepository _repository;

        public DeleteProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        //Unit is a special type from MediatR that represents a "void" response in asynchronous operations.
        public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByIdAsync(request.Id);
            if (product == null)
                throw new KeyNotFoundException($"Product with Id = {request.Id} not found.");

            await _repository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
