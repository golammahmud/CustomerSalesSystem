namespace CustomerSalesSystem.Application.Features.Sales.Commands
{
    public class DeleteSaleCommand : IRequest<Unit>
    {
        public int Id { get; }
        public DeleteSaleCommand(int id)
        {
               Id = id;
        }
    }
}
