using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Products.Commands;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class ProductService: BaseService
    {
        public ProductService(IHttpContextAccessor accessor, IHttpClientFactory factory)
        : base(accessor, factory) { }

        public async Task<List<ProductDto>> GetAllAsync() =>
            await Client.GetFromJsonAsync<List<ProductDto>>("api/products");

        public async Task<ProductDto?> GetByIdAsync(int id) =>
            await Client.GetFromJsonAsync<ProductDto>($"api/products/{id}");

        public async Task CreateAsync(CreateProductCommand product) =>
            await Client.PostAsJsonAsync("api/products", product);

        public async Task UpdateAsync(UpdateProductCommand product) =>
            await Client.PutAsJsonAsync($"api/products/{product.Id}", product);

        public async Task DeleteAsync(int id) =>
            await Client.DeleteAsync($"api/products/{id}");
    }

}
