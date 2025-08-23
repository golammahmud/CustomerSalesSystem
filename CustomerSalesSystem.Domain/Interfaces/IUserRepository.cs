using CustomerSalesSystem.Domain.Entities;

namespace CustomerSalesSystem.Domain.Interfaces
{
    public interface IUserRepository
    {
    
        Task<int> CreateUserAsync(User user);
        Task<User?> GetUserByUsernameAsync(string username);
        Task UpdateUserAsync(User user, bool isPasswordChanged = false);

        Task<User?> GetUserByIdAsync(int userId);
    }
}
