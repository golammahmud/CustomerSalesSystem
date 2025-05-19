using Dapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class CustomerReadRepository : ICustomerReadRepository
    {
        private readonly DapperContext _context;
        private readonly AppDbContext _appDbContext;

        public CustomerReadRepository(DapperContext context, AppDbContext appDbContext)
        {
            _context = context;
            _appDbContext = appDbContext;
        }

        public async Task<PagedResult<CustomerDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();

            var sql = @"
                        SELECT COUNT(*) FROM Customers;

                        SELECT Id, Name, Email, Phone
                        FROM Customers
                        ORDER BY Id
                        OFFSET @Offset ROWS
                        FETCH NEXT @PageSize ROWS ONLY;";

            var parameters = new
            {
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var totalCount = await multi.ReadSingleAsync<int>();
            var items = (await multi.ReadAsync<CustomerDto>()).ToList();

            return new PagedResult<CustomerDto>
            {
                Items = items,
                TotalCount = totalCount
            };
        }


        public async Task<CustomerDto?> GetByIdAsync(int customerId)
        {
            using var connection = _context.CreateConnection();
            var sql = "SELECT Id, Name, Email, Phone FROM Customers WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<CustomerDto>(sql, new { Id = customerId });
        }
    }
}
