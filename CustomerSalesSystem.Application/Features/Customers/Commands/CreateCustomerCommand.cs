using MediatR;

namespace CustomerSalesSystem.Application.Features.Customers.Commands
{
    public class CreateCustomerCommand : IRequest<int>
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
    }
}
