using CustomerSalesSystem.Domain;

namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            return await _context.Users
                                 .AsNoTracking() // no need to track for read-only queries
                                 .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateUserAsync(User user, bool isPasswordChanged = false)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException($"User with ID {user.Id} not found.");

            existingUser.Username = user.Username;

            if (isPasswordChanged && !string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                // Only rehash if password is updated
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            existingUser.RefreshTokenHash = user.RefreshTokenHash;
            existingUser.RefreshTokenExpiryTime = user.RefreshTokenExpiryTime;

            await _context.SaveChangesAsync(); // EF automatically tracks changes, no need for Update()
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than zero.");
            return await _context.Users
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
