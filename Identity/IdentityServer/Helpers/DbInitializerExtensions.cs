using IdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using static System.Formats.Asn1.AsnWriter;
using System.Threading;

namespace IdentityServer.Helpers
{
    public static class DbInitializerExtensions
    {
        public static async void InitDb(this IApplicationBuilder app, ConfigurationManager config)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();

            await PopulateScopes(scope);

            await PopulateInternalApps(scope);
        }
        private static async ValueTask PopulateScopes(IServiceScope scope)
        {
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var scopeDescriptor = new OpenIddictScopeDescriptor
            {
                Name = "test_scope",
                Resources = { "grpc_resource" }
            };

            var scopeInstance = await scopeManager.FindByNameAsync(scopeDescriptor.Name);

            if (scopeInstance == null)
            {
                await scopeManager.CreateAsync(scopeDescriptor);
            }
            else
            {
                await scopeManager.UpdateAsync(scopeInstance, scopeDescriptor);
            }
        }

        private static async ValueTask PopulateInternalApps(IServiceScope scopeService)
        {
            var appManager = scopeService.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

            var appDescriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = "grpc_client",
                ClientSecret = "grpc_secret",
                //Type = OpenIddictConstants.ClientTypes.Confidential,
                ConsentType = ConsentTypes.Explicit,
                RedirectUris =
                {
                    new Uri("https://host.docker.internal:7120/callback/login/local")
                },
                PostLogoutRedirectUris =
                {
                    new Uri("https://host.docker.internal:7120/callback/logout/local")
                },
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Logout,
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + "test_scope"
                },
                Requirements =
                {
                    Requirements.Features.ProofKeyForCodeExchange
                }
            };

            var client = await appManager.FindByClientIdAsync(appDescriptor.ClientId);

            if (client == null)
            {
                await appManager.CreateAsync(appDescriptor);
            }
            else
            {
                await appManager.DeleteAsync(client);
                await appManager.CreateAsync(appDescriptor);
            }
        }
    }
}
