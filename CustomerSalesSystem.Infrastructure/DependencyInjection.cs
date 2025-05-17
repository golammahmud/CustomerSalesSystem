using CustomerSalesSystem.Application.Features.Customers.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddScoped<ISalesRepository, SaleRepository>();


        // Dapper Read Access
        services.AddScoped<DapperContext>();
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ISaleReadRepository, SaleReadRepository>();

        return services;


    }
}
