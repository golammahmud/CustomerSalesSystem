using System.Security.Claims;

namespace CustomerSalesSystem.Application.Features.User.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<LoginResponseDTO>;
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponseDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserReadRepository _userReadRepository;
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(IUserRepository userRepository, IUserReadRepository userReadRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _userReadRepository = userReadRepository;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDTO> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new ArgumentException("Refresh token cannot be null or empty.", nameof(request.RefreshToken));

            // 1. Fetch user by refresh token
            var user = await _userReadRepository.GetUserByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
                throw new Exception("Invalid refresh token");
            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new Exception("Refresh token has expired");
            // 2. Verify hashed refresh token
            if (!BCrypt.Net.BCrypt.Verify(request.RefreshToken, user.RefreshTokenHash))
                throw new Exception("Invalid refresh token");
            // 3. Prepare claims for JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };
            // 4. Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            // 5. Save new hashed refresh token and expiry to DB
            var userEntity = await _userRepository.GetUserByIdAsync(user.Id);
            if (userEntity == null)
                throw new Exception("User not found");
            // Hash the new refresh token before saving
            userEntity.RefreshTokenHash = BCrypt.Net.BCrypt.HashPassword(newRefreshToken);
            userEntity.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry
            await _userRepository.UpdateUserAsync(userEntity);
            // 6. Return DTO
            return new LoginResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
