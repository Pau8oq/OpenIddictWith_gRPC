using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Client;
using System.Security.Cryptography.X509Certificates;
using Translating.Web.Domain.Repositories;
using Translating.Web.Infrastructure.Repositories;
using Translating.Web.Infrastructure.v1;
using Translating.Web.Interceptors;
using static OpenIddict.Client.AspNetCore.OpenIddictClientAspNetCoreConstants;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITranslationRepository, TranslationRepository>();

var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Trace);
});

var uri = new Uri(builder.Configuration.GetSection("TranslationServiceUri").Value);

builder.Services.AddGrpcClient<TranslationService.TranslationServiceClient>(o =>
    {
        o.Address = uri;
    })
    .ConfigurePrimaryHttpMessageHandler(() => 
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(X509Certificate2.CreateFromPemFile("/https/client.crt", "/https/client.key"));

        return handler;
    })
    .AddCallCredentials(async (context, metadata, serviceProvider) =>
    {
        var contextProvider = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var accessToken = await contextProvider.HttpContext.GetTokenAsync(Tokens.BackchannelAccessToken);

        metadata.Add("Authorization", $"Bearer {accessToken}");
    })
    .AddInterceptor(() => new TracerInterceptor(loggerFactory.CreateLogger<TracerInterceptor>()))
    .ConfigureChannel(o =>
    {
        o.MaxReceiveMessageSize = 6291456; // 6 MB,
        o.MaxSendMessageSize = 6291456; // 6 MB
    });


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //дозволяє обійтись без Login action method
    //Ask the OpenIddict client middleware to redirect the user agent to the identity provider
    //options.DefaultChallengeScheme = OpenIddictClientAspNetCoreDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
    options.SlidingExpiration = false;
});

builder.Services.AddOpenIddict()
            .AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow()
                      .AllowRefreshTokenFlow();

                options.AddDevelopmentEncryptionCertificate()
                       .AddDevelopmentSigningCertificate();

                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                options.UseAspNetCore()
                       .EnableStatusCodePagesIntegration()
                       .EnableRedirectionEndpointPassthrough()
                       .EnablePostLogoutRedirectionEndpointPassthrough();

                // Register the System.Net.Http integration and use the identity of the current
                // assembly as a more specific user agent, which can be useful when dealing with
                // providers that use the user agent as a way to throttle requests (e.g Reddit).
                options.UseSystemNetHttp()
                       .SetProductInformation(typeof(Program).Assembly);

                // Add a client registration matching the client application definition in the server project.
                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = new Uri("https://host.docker.internal:7220", UriKind.Absolute),

                    ClientId = "grpc_client",
                    ClientSecret = "grpc_secret",
                    Scopes = { Scopes.Email, Scopes.Profile, Scopes.Roles, Scopes.OfflineAccess, "test_scope" },

                    RedirectUri = new Uri("callback/login/local", UriKind.Relative),
                    PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
                });

                options.DisableTokenStorage();

            });


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
