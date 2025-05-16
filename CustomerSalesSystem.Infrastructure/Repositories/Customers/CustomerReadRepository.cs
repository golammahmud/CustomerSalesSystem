using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Interfaces.Repositories;
using Dapper;

namespace CustomerSalesSystem.Infrastructure.Repositories.Customers
{
    public class CustomerReadRepository : ICustomerReadRepository
    {
        private readonly DapperContext _context;

        public CustomerReadRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CustomerDto>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT Id, Name, Email, Phone FROM Customers";
            return await connection.QueryAsync<CustomerDto>(sql);
        }

        public async Task<CustomerDto?> GetByIdAsync(int customerId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT Id, Name, Email, Phone FROM Customers WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<CustomerDto>(sql, new { Id = customerId });
        }
    }
}
