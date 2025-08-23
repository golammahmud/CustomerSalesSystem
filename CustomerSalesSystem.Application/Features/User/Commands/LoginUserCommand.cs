namespace CustomerSalesSystem.Application.Features.User.Commands
{
    public record LoginUserCommand(string Username, string Password) : IRequest<LoginResponseDTO>;
}
