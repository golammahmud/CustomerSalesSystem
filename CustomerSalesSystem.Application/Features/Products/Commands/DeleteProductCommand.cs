using MediatR;

namespace CustomerSalesSystem.Application.Features.Products.Commands
{
    public class DeleteProductCommand : IRequest<Unit>
    {
        public int Id { get; }

        public DeleteProductCommand(int id)
        {
            Id = id;
        }
    }
}
