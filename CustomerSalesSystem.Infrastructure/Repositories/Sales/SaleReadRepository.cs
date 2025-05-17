using Dapper;

namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class SaleReadRepository : ISaleReadRepository
    {
        private readonly DapperContext _context;
        public SaleReadRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<SaleDto?> GetByIdAsync(int id)
        {
            var sql = @"
    SELECT s.Id, s.CustomerId, c.Name AS CustomerName, s.ProductId, p.Name AS ProductName,
           s.Quantity, (s.Quantity * p.Price) AS TotalPrice, s.SaleDate
    FROM Sales s
    JOIN Customers c ON s.CustomerId = c.Id
    JOIN Products p ON s.ProductId = p.Id
    WHERE s.Id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<SaleDto>(sql, new { Id = id });
        }



        public async Task<IEnumerable<SaleDto>> GetAllAsync()
        {
            var sql = @"
    SELECT s.Id, s.CustomerId, c.Name AS CustomerName, s.ProductId, p.Name AS ProductName,
           s.Quantity, (s.Quantity * p.Price) AS TotalPrice, s.SaleDate
    FROM Sales s
    JOIN Customers c ON s.CustomerId = c.Id
    JOIN Products p ON s.ProductId = p.Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<SaleDto>(sql);
        }

        public async Task<IEnumerable<SaleDto>> FilterAsync(int? customerId, DateTime? date)
        {
            var sql = @"
    SELECT s.Id, s.CustomerId, c.Name AS CustomerName, s.ProductId, p.Name AS ProductName,
           s.Quantity, (s.Quantity * p.Price) AS TotalPrice, s.SaleDate
    FROM Sales s
    JOIN Customers c ON s.CustomerId = c.Id
    JOIN Products p ON s.ProductId = p.Id
    WHERE (@CustomerId IS NULL OR s.CustomerId = @CustomerId)
      AND (@SaleDate IS NULL OR CAST(s.SaleDate AS DATE) = CAST(@SaleDate AS DATE))
    ORDER BY s.SaleDate DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<SaleDto>(sql, new { CustomerId = customerId, SaleDate = date });
        }


    }
}
