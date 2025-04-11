using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SSP_app.Codes;
using SSP_app.Components;
using SSP_app.Components.Account;
using SSP_app.Data;
using TodoApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<HashingHandler>();
builder.Services.AddScoped<ToDoHandler>();
builder.Services.AddScoped<SymmetricEncryptionHandler>();
builder.Services.AddScoped<AsymmetricEncryptionHandler>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var connectionString2 = builder.Configuration.GetConnectionString("ToDoConnection") ?? throw new InvalidOperationException("Connection string 'ToDoConnection' not found.");
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(connectionString2));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

builder.Services.Configure<IdentityOptions>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequiresAdmin", policy => {
        policy.RequireRole("Admin");
    }).AddPolicy("RequiresAuthenticated", policy => {
        policy.RequireAuthenticatedUser();
    });

string certPath = builder.Configuration.GetValue<string>("Cert:CertPath") ?? throw new InvalidOperationException("Certificate path not found.");
string certPassword = builder.Configuration.GetValue<string>("Cert:CertPassword") ?? throw new InvalidOperationException("Certificate password not found.");

builder.WebHost.UseKestrel(options => {
    options.ListenLocalhost(5011);
    options.ListenLocalhost(5012, listenOptions => {
        listenOptions.UseHttps(certPath, certPassword, httpsOptions => {
            httpsOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls13 | System.Security.Authentication.SslProtocols.Tls12;
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
