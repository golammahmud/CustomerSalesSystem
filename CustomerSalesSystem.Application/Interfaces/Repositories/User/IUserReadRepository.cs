using CustomerSalesSystem.Domain;

namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface IUserReadRepository
    {
        Task<bool> UserExistsAsync(string username);
        Task<UserDTO> GetUserByUsernameAsync(string username);
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO?> GetUserByRefreshTokenAsync(string refreshToken);

    }
}
