namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface ISaleReadRepository
    {
        Task<IEnumerable<SaleDto>> GetAllAsync();
        Task<SaleDto?> GetByIdAsync(int id);
        Task<IEnumerable<SaleDto>> FilterAsync(int? customerId, DateTime? date);
    }
}
