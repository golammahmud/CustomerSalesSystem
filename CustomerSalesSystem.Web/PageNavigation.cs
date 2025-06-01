namespace CustomerSalesSystem.Web
{
    public static class PageNavigation
    {
        #region search
        public static string GlobalSearch => "/Search/GlobalSearch";
        #endregion search
        #region Customer
        public static string CustomerList => "/Customers/Index";
        public static string EditCustomer(int id) => $"/Customers/Edit?id={id}";
        public static string CreateCustomer => "/Customers/Create";

        #endregion Customer

        #region Sales
        public static string SalesList => "/Sales/Index";
        public static string EditSale(int id) => $"/Sales/Edit?id={id}";
        public static string CreateSale => "/Sales/Create";

        #endregion Sales
    }
}
