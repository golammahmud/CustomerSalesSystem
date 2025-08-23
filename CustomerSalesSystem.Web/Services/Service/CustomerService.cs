using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Customers.Commands;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class CustomerService(IHttpClientFactory factory, IHttpContextAccessor accessor) : BaseService(accessor, factory)
    {

        public async Task<List<CustomerDto>> GetAllAsync()
        {
           
            return await Client.GetFromJsonAsync<List<CustomerDto>>("api/customers");
        }
          

        public async Task<CustomerDto?> GetByIdAsync(int id) =>
            await Client.GetFromJsonAsync<CustomerDto>($"api/customers/{id}");

        public async Task CreateAsync(CreateCustomerCommand customer) =>
            await Client.PostAsJsonAsync("api/customers", customer);

        public async Task UpdateAsync(UpdateCustomerCommand customer) =>
            await Client.PutAsJsonAsync($"api/customers/{customer.Id}", customer);

        public async Task DeleteAsync(int id) =>
            await Client.DeleteAsync($"api/customers/{id}");
    }


}
