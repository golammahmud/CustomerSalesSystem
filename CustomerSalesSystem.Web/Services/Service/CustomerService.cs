using CustomerSalesSystem.Application.DTOs;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class CustomerService(IHttpClientFactory factory, IHttpContextAccessor accessor) : BaseService(accessor, factory)
    {

        public async Task<PagedResponse<CustomerDto>> GetAllAsync(int pageNumber, int pageSize)
        {
            var response = await Client.GetFromJsonAsync<PagedResponse<CustomerDto>>(
                $"customers?pageNumber={pageNumber}&pageSize={pageSize}");

            return response ?? new PagedResponse<CustomerDto>();
        }


        public async Task<CustomerDto?> GetByIdAsync(int id) =>
            await Client.GetFromJsonAsync<CustomerDto>($"customers/{id}");

        public async Task CreateAsync(CustomerDto customer) =>
            await Client.PostAsJsonAsync("customers", customer);

        public async Task UpdateAsync(CustomerDto customer) =>
            await Client.PutAsJsonAsync($"customers/{customer.Id}", customer);

        public async Task DeleteAsync(int id) =>
            await Client.DeleteAsync($"customers/{id}");
    }


}
