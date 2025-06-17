using Dapper;

namespace CustomerSalesSystem.Infrastructure
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

        public async Task<PagedResult<CustomerDto>> SearchWithFiltersAsync(List<AIFieldFilter> filters, int pageNumber, int pageSize)
        {
            using var connection = _context.CreateConnection();
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            var allowedFields = new[] { "Name", "Email", "Phone" };

            int index = 0;

            foreach (var filter in filters)
            {
                if (!allowedFields.Contains(filter.Field, StringComparer.OrdinalIgnoreCase))
                    throw new Exception($"Invalid field: {filter.Field}");

                var paramName = $"@param{index}";

                string condition = filter.Operator.ToLower() switch
                {
                    "equals" => $"{filter.Field} = {paramName}",
                    "notequals" => $"{filter.Field} <> {paramName}",
                    "contains" => $"{filter.Field} LIKE '%' + {paramName} + '%'",
                    "startswith" => $"{filter.Field} LIKE {paramName} + '%'",
                    "endswith" => $"{filter.Field} LIKE '%' + {paramName}",
                    "greaterthan" => $"{filter.Field} > {paramName}",
                    "greaterthanorequal" => $"{filter.Field} >= {paramName}",
                    "lessthan" => $"{filter.Field} < {paramName}",
                    "lessthanorequal" => $"{filter.Field} <= {paramName}",
                    _ => throw new NotSupportedException($"Unsupported operator: {filter.Operator}")
                };

                object paramValue = filter.Operator.ToLower() switch
                {
                    "contains" => $"%{filter.Value}%",
                    _ => filter.Value!
                };

                whereClauses.Add(condition);
                parameters.Add(paramName, paramValue);
                index++;
            }

            string whereSql = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";

            string sql = $@"
        SELECT COUNT(*) FROM Customers {whereSql};

        SELECT * FROM Customers
        {whereSql}
        ORDER BY Id
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    ";

            parameters.Add("@Offset", (pageNumber - 1) * pageSize);
            parameters.Add("@PageSize", pageSize);

            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var totalCount = await multi.ReadFirstAsync<int>();
            var customers = (await multi.ReadAsync<CustomerDto>()).ToList();

            return new PagedResult<CustomerDto>
            {
                Items = customers,
                TotalCount = totalCount
            };
        }

    }
}
