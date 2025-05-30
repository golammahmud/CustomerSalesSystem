using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class SaleService
    {
        private readonly HttpClient _httpClient;

        public SaleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SaleDto>> GetAllAsync() =>
            await _httpClient.GetFromJsonAsync<List<SaleDto>>("api/sales");

        public async Task<SaleDto?> GetByIdAsync(int id) =>
            await _httpClient.GetFromJsonAsync<SaleDto>($"api/sales/{id}");

        //public async Task CreateAsync(CreateSaleCommand sale) =>
        //    await _httpClient.PostAsJsonAsync("api/sales", sale);

        //public async Task<List<SaleDto>> FilterAsync(SaleFilterDto filter) =>
        //    await _httpClient.PostAsJsonAsync("api/sales/filter", filter); // optional filter endpoint

        public async Task<decimal> GetCustomerTotalPurchaseAsync(int customerId) =>
            await _httpClient.GetFromJsonAsync<decimal>($"api/sales/total/{customerId}");
    }

}
