namespace CustomerSalesSystem.Application.Features.Products.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
