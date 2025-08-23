namespace CustomerSalesSystem.Application.Features.User.Commands
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserReadRepository _userReadRepository;
        private readonly ITokenService _tokenService;
        public RegisterUserCommandHandler(IUserRepository userRepository, IUserReadRepository userReadRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _userReadRepository = userReadRepository;
            _tokenService = tokenService;
        }

        public async Task<bool> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Username and Password cannot be empty");
            }
            // 1. Check if user already exists
            var exists = await _userReadRepository.UserExistsAsync(request.Username);
            if (exists)
                throw new Exception("Username already taken");

            // 2. Create new user entity
            var newUser = new CustomerSalesSystem.Domain.Entities.User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password.Trim(), workFactor: 12),
                RefreshTokenHash = string.Empty,
                RefreshTokenExpiryTime = DateTime.MinValue
            };

            // 3. Save the user to the repository
            var result = await _userRepository.CreateUserAsync(newUser);
            if (result <= 0)
            {
                throw new Exception("Failed to create user");
            }

            // 4. Return success
            return true;
        }
    }
}
