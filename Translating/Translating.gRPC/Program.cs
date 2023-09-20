using Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Validation.AspNetCore;
using System.Reflection;
using Translating.Domain.AggregatesModel.TranslationAggregate;
using Translating.gRPC;
using Translating.gRPC.Interceptors;
using Translating.gRPC.Services;
using Translating.gRPC.Services.v1;
using Translating.Infrastructure;
using Translating.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<ExceptionInterceptor>();
});

builder.Services.AddGrpcReflection();
builder.Services.AddScoped<ITranslationRepository, TranslationRepository>();
builder.Services.AddSingleton<ProtoService>();

builder.Services.AddDbContext<TranslationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TranslationService"), sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
    });
});

builder.Services
    .AddOpenIddict()
    .AddValidation(options =>
    {

        options.SetIssuer("https://host.docker.internal:7220");
        options.AddAudiences("grpc_resource");

        options.UseIntrospection()
               .SetClientId("grpc_client")
               .SetClientSecret("grpc_secret");

        options.UseSystemNetHttp();

        options.AddEncryptionCertificate(AuthenticationExtensionMethods.TokenEncryptionCertificateForDocker());

        options.UseAspNetCore();
    });

IdentityModelEventSource.ShowPII = true;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.ApplyMigrationAndSeed();

if (app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcReflectionService();
app.MapGrpcService<Translating.gRPC.Services.v1.TranslationGrpcService>();

app.MapGet("/protos", (ProtoService protoService) =>
{
    return Results.Ok(protoService.GetAll());
});

app.MapGet("/protos/v{version:int}/{protoName}", (ProtoService protoService, int version, string protoName) =>
{
    var filePath = protoService.Get(version, protoName);

    if (filePath != null)
        return Results.File(filePath);

    return Results.NotFound();
});

app.MapGet("/protos/v{version:int}/{protoName}/view", async (ProtoService protoService, int version, string protoName) =>
{
    var text = await protoService.ViewAsync(version, protoName);

    if (!string.IsNullOrEmpty(text))
    {
        return Results.Text(text);
    }

    return Results.NotFound();
});

app.Run();
