using CustomerSalesSystem.Domain.Entities;
using CustomerSalesSystem.Domain.Interfaces;
using MediatR;

namespace CustomerSalesSystem.Application.Features.Products.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _repository;

        public CreateProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Price = request.Price,
            };
            

            await _repository.AddAsync(product);
            return product.Id;
        }
    }
}
