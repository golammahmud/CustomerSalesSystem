using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Products.Commands;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class ProductService : BaseService
    {
        public ProductService(IHttpContextAccessor accessor, IHttpClientFactory factory)
            : base(accessor, factory) { }

        public async Task<List<ProductDto>> GetAllAsync()
        {
            var response = await Client.GetAsync("products");
            if (!response.IsSuccessStatusCode)
                return new List<ProductDto>(); // gracefully return empty list instead of null

            var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
            return products ?? new List<ProductDto>();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var response = await Client.GetAsync($"products/{id}");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task<bool> CreateAsync(CreateProductCommand product)
        {
            var response = await Client.PostAsJsonAsync("products", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(UpdateProductCommand product)
        {
            var response = await Client.PutAsJsonAsync($"products/{product.Id}", product);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await Client.DeleteAsync($"products/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}
