using CustomerSalesSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);



// Add services to the container.
builder.Services.AddInfrastructure(builder.Configuration);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins(
                                    "https://localhost:7049",
                                    "https://customersalessystemweb20250523111305-bscjagdyc7ehbvcw.canadacentral-01.azurewebsites.net" // Add your actual frontend URL if deployed
                                )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


builder.Services.AddResponseCompression();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors(MyAllowSpecificOrigins);
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
