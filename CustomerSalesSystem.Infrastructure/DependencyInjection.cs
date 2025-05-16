using CustomerSalesSystem.Application.Features.Customers.Commands;
using CustomerSalesSystem.Application.Interfaces.Repositories;
using CustomerSalesSystem.Domain.Interfaces;
using CustomerSalesSystem.Infrastructure.Persistence;
using CustomerSalesSystem.Infrastructure.Repositories.Customers;
using CustomerSalesSystem.Infrastructure.Repositories.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CustomerSalesSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {

        services.AddDbContext<AppDbContext>(options =>
      options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // Register Application CQRS handlers
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly)); // <-- this is the important one

        // Repositories (EF Core - Write)
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
      

        // Dapper Read Access
        services.AddScoped<DapperContext>();
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();

        return services;


    }
}
