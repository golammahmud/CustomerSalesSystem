using System.Security.Claims;

namespace CustomerSalesSystem.Application.Features.User.Commands
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserReadRepository _userReadRepository;
        private readonly ITokenService _tokenService;
        public LoginUserCommandHandler(IUserRepository userRepository, IUserReadRepository userReadRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _userReadRepository = userReadRepository;
            _tokenService = tokenService;
        }
        public async Task<LoginResponseDTO> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Fetch user by username
            var user = await _userReadRepository.GetUserByUsernameAsync(request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password.Trim(), user.PasswordHash))
                throw new Exception("Invalid username or password");

            // 2. Prepare claims for JWT
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            // 4. Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 5. Save hashed refresh token and expiry to DB
            var userEntity = await _userRepository.GetUserByIdAsync(user.Id);
           if (userEntity == null)
                throw new Exception("User not found");

            // Hash the refresh token before saving
            userEntity.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken);
            userEntity.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry
            await _userRepository.UpdateUserAsync(userEntity);

            // 6. Return DTO
            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

        }
    }
}
