using CustomerSalesSystem.Web.Helper;
using CustomerSalesSystem.Web.Services;
using CustomerSalesSystem.Web.Services.IService;
using CustomerSalesSystem.Web.Services.Service;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var ApiBaseUrl = builder.Configuration["ApiBaseUrls:CustomerSalesApi"];

// Configure HTTP clients
builder.Services.AddTransient<TokenRefreshHandler>();

builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(ApiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<TokenRefreshHandler>()
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    UseCookies = true,
    CookieContainer = new System.Net.CookieContainer()
});


// Register Services
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<SalesService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IAssistantService, AssistantService>();
builder.Services.AddScoped<IFilterQueryFromAIService, FilterQueryFromAIService>();
builder.Services.AddScoped<IGlobalSearchAndChatService, GlobalSearchAndChatService>();


// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(op =>
    {
        // Secure all pages by default
        op.Conventions.AuthorizeFolder("/");

        // Allow anonymous access to Login and Register
        op.Conventions.AllowAnonymousToPage("/Security/Login");
        op.Conventions.AllowAnonymousToPage("/Security/Register");
    });


builder.Services.AddControllers();

#region Authentication

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Security/Login";           // Where to redirect unauthenticated users
        options.AccessDeniedPath = "/UnAuthorized";      // Forbidden page
        options.LogoutPath = "/Security/Login?Handler=LogOff";         // Separate Razor Page for logout
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true; // Security: HTTP-only cookie
        options.Cookie.IsEssential = true; // GDPR compliance
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
        options.Cookie.SameSite = SameSiteMode.Strict;
    });


#endregion

// For .NET 6+ (Program.cs)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Security: HTTP-only cookie
    options.Cookie.IsEssential = true; // GDPR compliance
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevent CSRF attacks

});


#region Security Headers
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
// Configure CORS (if your frontend is separate)
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecureCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .WithExposedHeaders("X-CSRF-TOKEN");
    });
});

#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAntiforgery();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/", context =>
{
    context.Response.Redirect("/Security/Login");
    return Task.CompletedTask;
});

app.MapControllers();
app.Run();
