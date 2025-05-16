namespace CustomerSalesSystem.Application.Features.Customers.Commands
{
    public class UpdateCustomerCommand:IRequest<bool>
    {
        public int Id { get; set; }
        public string Name { get; set; } 
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
