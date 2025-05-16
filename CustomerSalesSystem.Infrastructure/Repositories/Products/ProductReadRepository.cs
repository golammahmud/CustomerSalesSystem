using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Interfaces.Repositories;
using Dapper;

namespace CustomerSalesSystem.Infrastructure.Repositories.Products
{
    public class ProductReadRepository : IProductReadRepository
    {
        private readonly DapperContext _context;

        public ProductReadRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT Id, Name, Price FROM Products";
            return await connection.QueryAsync<ProductDto>(sql);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT Id, Name, Price FROM Products WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<ProductDto>(sql, new { Id = id });
        }

    }
}