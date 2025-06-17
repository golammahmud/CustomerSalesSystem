using CustomerSalesSystem.Web.Services;
using CustomerSalesSystem.Web.Services.IService;
using CustomerSalesSystem.Web.Services.Service;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddHttpClient("API", client =>
//{
//    client.BaseAddress = new Uri("https://customersalessystemapi-dya6h3fjfvfvcxfd.canadacentral-01.azurewebsites.net/api/");
//});

var apiBaseUrl = builder.Configuration["ApiBaseUrls:CustomerSalesApi"];

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


builder.Services.AddHttpClient(); // register IHttpClientFactory

builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddScoped<IFilterQueryFromAIService, FilterQueryFromAIService>();
builder.Services.AddScoped<IGlobalSearchAndChatService, GlobalSearchAndChatService>();

//builder.Services.AddHttpClient<CustomerService>(c => c.BaseAddress = new Uri("https://localhost:7120/"));
//builder.Services.AddHttpClient<ProductService>(c => c.BaseAddress = new Uri("https://localhost:7120/"));
//builder.Services.AddHttpClient<SaleService>(c => c.BaseAddress = new Uri("https://localhost:7120/"));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Customers");
    return Task.CompletedTask;
});

app.MapControllers();
app.Run();
