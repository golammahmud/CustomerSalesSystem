namespace CustomerSalesSystem.Web.Helper
{
    public static class NavigationAliasHelper
    {
        public static readonly Dictionary<string, string> TargetMap = new(StringComparer.OrdinalIgnoreCase)
{
    // Specific phrases for list pages
    { "customer list", "Customer List" },
    { "customers page", "Customer List" },
    { "customers list page", "Customer List" },
    { "go to customer", "Customer List" },
    { "view customers", "Customer List" },
    { "customers", "Customer List" },
    { "customer", "Customer List" },

    { "sales list", "Sales List" },
    { "sales page", "Sales List" },
    { "go to sales", "Sales List" },
    { "view sales", "Sales List" },
    { "sales", "Sales List" },
    { "sale", "Sales List" },

    // Common navigation pages
    { "search page", "Search" },
    { "search", "Search" },

    { "main page", "Home" },
    { "dashboard", "Home" },
    { "main", "Home" },
    { "home", "Home" },

    // Create customer
    { "add new customer", "CreateCustomer" },
    { "create new customer", "CreateCustomer" },
    { "start customer entry", "CreateCustomer" },
    { "register customer", "CreateCustomer" },
    { "add customer", "CreateCustomer" },
    { "create customer", "CreateCustomer" },
    { "new customer", "CreateCustomer" },

    // Create sale
    { "add new sale", "CreateSale" },
    { "create new sale", "CreateSale" },
    { "start sale", "CreateSale" },
    { "record sale", "CreateSale" },
    { "add sale", "CreateSale" },
    { "create sale", "CreateSale" },
    { "new sale", "CreateSale" },

    // Edit customer
    { "edit customer", "EditCustomer" },
    { "change customer", "EditCustomer" },
    { "update customer", "EditCustomer" },
    { "modify customer", "EditCustomer" },

    // Edit sale
    { "edit sale", "EditSale" },
    { "change sale", "EditSale" },
    { "update sale", "EditSale" },
    { "modify sale", "EditSale" }
};

        public static readonly Dictionary<string, string> NavigationLinks = new()
        {
            ["Customer List"] = "/Customers/Index",
            ["EditCustomer"] = "/Customers/Edit?id={id}",
            ["CreateCustomer"] = "/Customers/Create",
            ["Sales List"] = "/Sales/Index",
            ["EditSale"] = "/Sales/Edit?id={id}",
            ["CreateSale"] = "/Sales/Create",
            ["Global Search"] = "/Search/GlobalSearch"
        };

        public static string? ResolveTarget(string userQuery)
        {
            // Normalize the query
            var cleaned = userQuery.ToLowerInvariant().Trim();

            // Try exact match
            if (TargetMap.TryGetValue(cleaned, out var exactMatch))
                return exactMatch;

            // ❗ Avoid fuzzy if already looks like a valid value
            if (NavigationLinks.ContainsKey(userQuery))
                return userQuery;

            // Try fuzzy/contains match
            foreach (var key in TargetMap.Keys.OrderByDescending(k => k.Length)) // Prefer longer matches
            {
                if (cleaned.Contains(key))
                    return TargetMap[key];
            }

            return null;
        }


    }

}
