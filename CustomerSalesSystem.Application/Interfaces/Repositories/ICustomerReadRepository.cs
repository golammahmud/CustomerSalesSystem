using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Domain.Entities;

namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface ICustomerReadRepository
    {
        Task<IEnumerable<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int customerId);
    }
}
