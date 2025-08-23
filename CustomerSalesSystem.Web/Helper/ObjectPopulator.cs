using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Helper
{
    public static class ObjectPopulator
    {
        public static void Populate<T>(T target, List<AIFieldFilter> filters)
        {
            var props = typeof(T).GetProperties();

            foreach (var filter in filters)
            {
                var prop = props.FirstOrDefault(p =>
                    string.Equals(p.Name, filter.Field, StringComparison.OrdinalIgnoreCase));

                if (prop == null || !prop.CanWrite) continue;

                try
                {
                    object? value = Convert.ChangeType(filter.Value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                    prop.SetValue(target, value);
                }
                catch
                {
                    // Optionally log or ignore bad conversions
                }
            }
        }

        private static string GetIntent(string query)
        {
            query = query.ToLowerInvariant();

            if (query.Contains("create") || query.Contains("add") || query.Contains("save"))
            {
                if (query.Contains("customer")) return "CreateCustomer";
                if (query.Contains("product")) return "CreateProduct";
                if (query.Contains("sale")) return "CreateSale";
            }

            // Existing Search Intents
            if (query.Contains("customer")) return "SearchCustomer";
            if (query.Contains("product")) return "SearchProduct";
            if (query.Contains("sale")) return "SearchSales";

            return "Unknown";
        }

    }

}
