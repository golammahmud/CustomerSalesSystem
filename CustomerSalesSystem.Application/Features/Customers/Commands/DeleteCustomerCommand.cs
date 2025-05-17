namespace CustomerSalesSystem.Application.Features.Customers.Commands
{
    public class DeleteCustomerCommand : IRequest<Unit>
    {
        public int Id { get; }

        public DeleteCustomerCommand(int id)
        {
            Id = id;
        }
    }
}
