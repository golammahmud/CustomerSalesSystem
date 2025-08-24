using CustomerSalesSystem.Application.DTOs;
using CustomerSalesSystem.Application.Features.Sales.Commands;
using CustomerSalesSystem.Domain.Entities;
using System.Net.Http.Json;

namespace CustomerSalesSystem.Web.Services.Service
{
    public class SalesService : BaseService
    {
        public SalesService(IHttpContextAccessor accessor, IHttpClientFactory factory)
            : base(accessor, factory) { }

        public async Task<List<SaleDto>> GetAllAsync()
        {
            try
            {
                return await Client.GetFromJsonAsync<List<SaleDto>>("sales")
                       ?? new List<SaleDto>();
            }
            catch (HttpRequestException )
            {
                // Log error if needed
                return new List<SaleDto>();
            }
        }

        public async Task<SaleDto?> GetByIdAsync(int id)
        {
            try
            {
                return await Client.GetFromJsonAsync<SaleDto>($"sales/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<bool> CreateAsync(CreateSaleDto sale)
        {
            try
            {
                var response = await Client.PostAsJsonAsync("sales", sale);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        public async Task<List<SaleDto>> FilterAsync(int? customerId, DateTime? date)
        {
            try
            {
                var response = await Client.GetAsync($"sales/filter?customerId={customerId}&date={date}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<SaleDto>>()
                           ?? new List<SaleDto>();
                }
                return new List<SaleDto>();
            }
            catch (HttpRequestException)
            {
                return new List<SaleDto>();
            }
        }

        public async Task<decimal> GetCustomerTotalPurchaseAsync(int customerId)
        {
            try
            {
                return await Client.GetFromJsonAsync<decimal>($"sales/total/{customerId}");
            }
            catch (HttpRequestException)
            {
                return 0m;
            }
        }
    }

}
