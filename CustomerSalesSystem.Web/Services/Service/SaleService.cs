using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class SaleService: BaseService
    {
        public SaleService(IHttpContextAccessor accessor, IHttpClientFactory factory)
        : base(accessor, factory) { }

        public async Task<List<SaleDto>> GetAllAsync() =>
            await Client.GetFromJsonAsync<List<SaleDto>>("api/sales");

        public async Task<SaleDto?> GetByIdAsync(int id) =>
            await Client.GetFromJsonAsync<SaleDto>($"api/sales/{id}");

        //public async Task CreateAsync(CreateSaleCommand sale) =>
        //    await _httpClient.PostAsJsonAsync("api/sales", sale);

        //public async Task<List<SaleDto>> FilterAsync(SaleFilterDto filter) =>
        //    await _httpClient.PostAsJsonAsync("api/sales/filter", filter); // optional filter endpoint

        public async Task<decimal> GetCustomerTotalPurchaseAsync(int customerId) =>
            await Client.GetFromJsonAsync<decimal>($"api/sales/total/{customerId}");
    }

}
