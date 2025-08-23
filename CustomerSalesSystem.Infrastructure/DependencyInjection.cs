using CustomerSalesSystem.Application.Features.Customers.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerSalesSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {

      //  services.AddDbContext<AppDbContext>(options =>
      //options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddDbContext<AppDbContext>(options =>
           options.UseSqlServer(
               config.GetConnectionString("DefaultConnection"),
               sqlOptions =>
               {
                   sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName); // optional
                   sqlOptions.CommandTimeout(60); // optional
               }));

    
    // Register Application CQRS handlers
    services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateCustomerCommand).Assembly)); // <-- this is the important one

        // Register TokenService
        services.AddSingleton<ITokenService, TokenService>();

        //User Management 

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();

        // Repositories (EF Core - Write)
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISalesRepository, SaleRepository>();



        // Dapper Read Access
        services.AddScoped<DapperContext>();
        services.AddScoped<ICustomerReadRepository, CustomerReadRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ISaleReadRepository, SaleReadRepository>();
        services.AddScoped<IGlobalSearchRepository, GlobalSearchRepository>();


        return services;


    }
}
