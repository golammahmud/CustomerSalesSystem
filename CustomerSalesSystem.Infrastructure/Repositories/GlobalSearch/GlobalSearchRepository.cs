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
            var targetPage = "";
            var allowedFields = new List<string>();
            var descriptionSql = "";
            var nameSql = "";
            var joinClause = "";

            // Step 1: Set metadata based on intent
            switch (request.Intent)
            {
                case "SearchCustomer":
                    table = "Customers";
                    targetPage = "EditCustomer";
                    allowedFields = new() { "Name", "Email", "Phone" };
                    nameSql = "ISNULL(Customers.Name, '')";
                    descriptionSql = "ISNULL(Customers.Email, '') + ' | ' + ISNULL(Customers.Phone, '')";
                    break;

                case "SearchProduct":
                    table = "Products";
                    targetPage = "EditProduct";
                    allowedFields = new() { "Name", "Price" };
                    nameSql = "ISNULL(Products.Name, '')";
                    descriptionSql = "'Price: $' + CAST(ISNULL(Products.Price, 0) AS VARCHAR)";
                    break;

                case "SearchSales":
                    table = "Sales";
                    targetPage = "EditSale";
                    allowedFields = new()
                            {
                                "Id", "Quantity", "TotalPrice", "SaleDate", "CustomerId", "ProductId",
                                "CustomerName", "ProductName"
                            };
                    nameSql = "'Sale #' + CAST(Sales.Id AS VARCHAR)";
                    descriptionSql = "ISNULL(Customers.Name, '') + ' | ' + ISNULL(Products.Name, '') + ' | $' + CAST(ISNULL(TotalPrice * Quantity, 0) AS VARCHAR)";
                    joinClause = @"
                LEFT JOIN Customers ON Sales.CustomerId = Customers.Id
                LEFT JOIN Products ON Sales.ProductId = Products.Id";
                    break;

                default:
                    return new(); // Unknown intent
            }

            // Step 2: Build SQL
                        sqlBuilder.Append($@"
            SELECT 
                '{request.Intent.Replace("Search", "")}' AS [Type],
                {table}.Id,
                {nameSql} AS [Name],
                {descriptionSql} AS [Description],
                '{targetPage}' AS [TargetPage]
            FROM {table}
            {joinClause}
            WHERE 1=1
            ");

            // Step 3: Apply filters
            int index = 0;
            foreach (var filter in request.Entities)
            {
                if (!allowedFields.Contains(filter.Field, StringComparer.OrdinalIgnoreCase))
                    continue;

                string paramName = $"@param{index++}";
                string condition = GetSqlCondition(filter, paramName);

                if (bool.TryParse(filter.Value?.ToString(), out var boolVal))
                    parameters.Add(paramName, boolVal ? 1 : 0);
                else
                    parameters.Add(paramName, filter.Value);

                sqlBuilder.Append(" AND " + condition);
            }

            sqlBuilder.Append(" ORDER BY " + table + ".Id DESC");

            // Step 4: Execute SQL
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
