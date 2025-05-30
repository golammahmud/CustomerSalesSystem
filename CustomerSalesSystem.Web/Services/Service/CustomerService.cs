using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Customers.Commands;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class CustomerService
    {
        private readonly HttpClient _httpClient;

        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CustomerDto>> GetAllAsync() =>
            await _httpClient.GetFromJsonAsync<List<CustomerDto>>("api/customers");

        public async Task<CustomerDto?> GetByIdAsync(int id) =>
            await _httpClient.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");

        public async Task CreateAsync(CreateCustomerCommand customer) =>
            await _httpClient.PostAsJsonAsync("api/customers", customer);

        public async Task UpdateAsync(UpdateCustomerCommand customer) =>
            await _httpClient.PutAsJsonAsync($"api/customers/{customer.Id}", customer);

        public async Task DeleteAsync(int id) =>
            await _httpClient.DeleteAsync($"api/customers/{id}");
    }


}
