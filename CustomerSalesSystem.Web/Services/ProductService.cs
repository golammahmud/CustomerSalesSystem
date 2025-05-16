using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Products.Commands;

namespace CustomerSalesSystem.Web.Services
{
    public class ProductService
    {
        private readonly HttpClient _httpClient;

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductDto>> GetAllAsync() =>
            await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/products");

        public async Task<ProductDto?> GetByIdAsync(int id) =>
            await _httpClient.GetFromJsonAsync<ProductDto>($"api/products/{id}");

        public async Task CreateAsync(CreateProductCommand product) =>
            await _httpClient.PostAsJsonAsync("api/products", product);

        public async Task UpdateAsync(UpdateProductCommand product) =>
            await _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product);

        public async Task DeleteAsync(int id) =>
            await _httpClient.DeleteAsync($"api/products/{id}");
    }

}
