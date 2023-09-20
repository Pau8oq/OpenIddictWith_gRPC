using Certificates;
using IdentityServer;
using IdentityServer.Helpers;
using IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(c =>
    {
        c.LoginPath = "/account/login";
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultIdentityConnection"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    });
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.AllowClientCredentialsFlow()
        .AllowAuthorizationCodeFlow()
                       .AllowPasswordFlow()
                       .AllowRefreshTokenFlow();

        options.SetAuthorizationEndpointUris("/authorize")
                       .SetIntrospectionEndpointUris("/introspect")
                       .SetLogoutEndpointUris("/logout")
                       .SetTokenEndpointUris("/token")
                       .SetUserinfoEndpointUris("/userinfo")
                       .SetVerificationEndpointUris("/verify");

        options.AddEncryptionCertificate(AuthenticationExtensionMethods.TokenEncryptionCertificateForDocker());
        options.AddSigningCertificate(AuthenticationExtensionMethods.TokenSigningCertificateForDocker());

        //options.AddDevelopmentEncryptionCertificate();
        //options.AddDevelopmentSigningCertificate();

        // Force client applications to use Proof Key for Code Exchange (PKCE).
        options.RequireProofKeyForCodeExchange();

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options.UseAspNetCore()
               .EnableStatusCodePagesIntegration()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough()
               .EnableVerificationEndpointPassthrough();

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "test_scope");

    });

//builder.Services.AddHostedService<ClientsSeeder>();

var app = builder.Build();

app.InitDb(builder.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
