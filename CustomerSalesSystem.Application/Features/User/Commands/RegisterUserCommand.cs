namespace CustomerSalesSystem.Application.Features.User.Commands
{
    public class RegisterUserCommand : IRequest<bool>
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
