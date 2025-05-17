using CustomerSalesSystem.Domain.Entities;

namespace CustomerSalesSystem.Domain.Interfaces
{
    public interface ISalesRepository
    {
        Task<Sale?> GetByIdAsync(int id);
        Task AddAsync(Sale sale);
        Task UpdateAsync(Sale sale);
        Task DeleteAsync(int id);
    }
}
