namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface ICustomerReadRepository
    {
        //Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<PagedResult<CustomerDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<CustomerDto?> GetByIdAsync(int customerId);

        /// <summary>
        /// Searches customers using dynamically built filters.
        /// </summary>
        /// <param name="filters">List of filters from AI or user input</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <returns>Paged result of customers matching filters</returns>
        Task<PagedResult<CustomerDto>> SearchWithFiltersAsync(List<AIFieldFilter> filters, int pageNumber, int pageSize);

    }
}
