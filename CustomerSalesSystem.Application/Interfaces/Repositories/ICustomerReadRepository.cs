namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface ICustomerReadRepository
    {
        //Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<PagedResult<CustomerDto>> GetAllAsync(int pageNumber, int pageSize);
        Task<CustomerDto?> GetByIdAsync(int customerId);
    }
}
