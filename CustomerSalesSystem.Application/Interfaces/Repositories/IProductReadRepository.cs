namespace CustomerSalesSystem.Application.Interfaces.Repositories
{
    public interface IProductReadRepository
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
    }
}
