using CustomerSalesSystem.Domain.Entities;

namespace CustomerSalesSystem.Domain.Interfaces
{
    public interface ICustomerRepository
    {
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task DeleteAsync(int customerId);
        Task<Customer?> GetByIdAsync(int customerId);
    }
}
