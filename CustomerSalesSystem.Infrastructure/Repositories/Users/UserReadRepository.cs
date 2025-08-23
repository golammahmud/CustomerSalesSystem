using Dapper;

namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class UserReadRepository : IUserReadRepository
    {
        private readonly DapperContext _context;
        public UserReadRepository(DapperContext context)
        {
            _context = context;
        }

        public Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var query = @"SELECT Id, Username, PasswordHash, RefreshTokenHash, RefreshTokenExpiryTime
                      FROM Users";
            using var connection = _context.CreateConnection();
            return connection.QueryAsync<UserDTO>(query);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            var query = @"SELECT Id, Username, PasswordHash, RefreshTokenHash, RefreshTokenExpiryTime
                      FROM Users
                      WHERE Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<UserDTO>(query, new { Id = userId });
        }

        public async Task<UserDTO> GetUserByUsernameAsync(string username)
        {
            var query = @"SELECT Id, Username, PasswordHash, RefreshTokenHash, RefreshTokenExpiryTime
                      FROM Users
                      WHERE Username = @Username";
            using var connection = _context.CreateConnection();
            var user = await connection.QuerySingleOrDefaultAsync<UserDTO>(query, new { Username = username });
            if (user == null)
            {
                throw new KeyNotFoundException($"User with username '{username}' not found.");
            }
            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            var query = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
            using var connection = _context.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(query, new { Username = username });
            return count > 0;
        }
        public async Task<UserDTO?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            var query = @"SELECT Id, Username, PasswordHash, RefreshTokenHash, RefreshTokenExpiryTime
                  FROM Users";

            using var connection = _context.CreateConnection();
            var users = await connection.QueryAsync<UserDTO>(query);

            // Compare hashed refresh tokens in memory
            return users.FirstOrDefault(u => BCrypt.Net.BCrypt.Verify(refreshToken, u.RefreshTokenHash));
        }

    }
}
