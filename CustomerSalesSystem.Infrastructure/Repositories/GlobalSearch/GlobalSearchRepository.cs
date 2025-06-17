using Dapper;
using System.Data;
using System.Text;

namespace CustomerSalesSystem.Infrastructure.Repositories
{
    public class GlobalSearchRepository : IGlobalSearchRepository
    {
        private readonly DapperContext _context;

        public GlobalSearchRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<GlobalSearchResultDto>> SearchAsync(AIIntentResult request)
        {
            using var connection = _context.CreateConnection();

            var sqlBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            var table = "";
            var navTarget = "";
            var descriptionSql = "";

            // Step 1: Set table-specific metadata
            var allowedFields = new List<string>();
            switch (request.Intent)
            {
                case "SearchCustomer":
                    table = "Customers";
                    navTarget = "EditCustomer";
                    allowedFields = new() { "Name", "Email", "Phone" };
                    descriptionSql = "Email + ' | ' + Phone";
                    break;

                case "SearchProduct":
                    table = "Products";
                    navTarget = "EditProduct";
                    allowedFields = new() { "Name", "Price" };
                    descriptionSql = "'Price: $' + CAST(Price AS VARCHAR)";
                    break;

                case "SearchSales":
                    table = "Sales";
                    navTarget = "EditSale";
                    allowedFields = new() { "Id", "Amount", "Notes" };
                    descriptionSql = "'Amount: $' + CAST(Amount AS VARCHAR)";
                    break;

                default:
                    return new(); // or throw
            }

            // Step 2: Start SQL
            sqlBuilder.Append($@"
                SELECT 
                    '{request.Intent.Replace("Search", "")}' AS [Table],
                    Id,
                    {(request.Intent == "SearchSales" ? "'Sale #' + CAST(Id AS VARCHAR)" : "Name")} AS Title,
                    {descriptionSql} AS Description,
                    '{navTarget}' AS NavigationTarget
                FROM {table}
                WHERE 1=1
            ");

            // Step 3: Apply filters
            int index = 0;
            foreach (var filter in request.Entities)
            {
                if (!allowedFields.Contains(filter.Field, StringComparer.OrdinalIgnoreCase))
                    continue; // Skip or throw for unknown fields

                string paramName = $"@param{index++}";
                string sqlCondition = GetSqlCondition(filter, paramName);

                if (bool.TryParse(filter.Value?.ToString(), out var boolVal))
                    parameters.Add(paramName, boolVal ? 1 : 0);
                else
                    parameters.Add(paramName, filter.Value);

                sqlBuilder.Append(" AND " + sqlCondition);
            }


            sqlBuilder.Append(" ORDER BY [Table]");

            var sql = sqlBuilder.ToString();
            var results = await connection.QueryAsync<GlobalSearchResultDto>(sql, parameters);
            return results.ToList();
        }


        private string GetSqlCondition(AIFieldFilter filter, string paramName)
        {
            string fieldExpr = filter.Operator.ToLower() switch
            {
                "contains" or "startswith" or "endswith" => $"CAST({filter.Field} AS VARCHAR)",
                _ => filter.Field
            };

            return filter.Operator.ToLower() switch
            {
                "equals" => $"{filter.Field} = {paramName}",
                "notequals" => $"{filter.Field} <> {paramName}",
                "contains" => $"{fieldExpr} LIKE '%' + {paramName} + '%'",
                "startswith" => $"{fieldExpr} LIKE {paramName} + '%'",
                "endswith" => $"{fieldExpr} LIKE '%' + {paramName}",
                "greaterthan" => $"{filter.Field} > {paramName}",
                "greaterthanorequal" => $"{filter.Field} >= {paramName}",
                "lessthan" => $"{filter.Field} < {paramName}",
                "lessthanorequal" => $"{filter.Field} <= {paramName}",
                _ => throw new NotSupportedException($"Unsupported operator: {filter.Operator}")
            };
        }


    }
}
