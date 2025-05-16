using CustomerSalesSystem.Domain.Interfaces;
using MediatR;

namespace CustomerSalesSystem.Application.Features.Customers.Commands
{
    public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, Unit>
    {
        private readonly ICustomerRepository _repository;

        public DeleteCustomerCommandHandler(ICustomerRepository repository)
        {
            _repository = repository;
        }

        //Unit is a special type from MediatR that represents a "void" response in asynchronous operations.
        public async Task<Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _repository.GetByIdAsync(request.Id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with Id = {request.Id} not found.");

            await _repository.DeleteAsync(request.Id);
            return Unit.Value;
        }
    }
}
