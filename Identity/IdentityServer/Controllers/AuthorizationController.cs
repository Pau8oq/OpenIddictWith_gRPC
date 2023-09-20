
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationController(
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictScopeManager scopeManager)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        [HttpGet("~/authorize")]
        [HttpPost("~/authorize")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                  throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            var result = await HttpContext.AuthenticateAsync();
            //CookieAuthenticationDefaults.AuthenticationScheme можна не вказувати так як ми вказали це як default в Program.cs builder.Services.AddAuthentication...
            //var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result == null || !result.Succeeded || request.HasPrompt(Prompts.Login) ||
                (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {

                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                // To avoid endless login -> authorization redirects, the prompt=login flag
                // is removed from the authorization request payload before redirecting the user.
                var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

                var parameters = Request.HasFormContentType ?
                    Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                    Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                return Challenge(new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                }/*, CookieAuthenticationDefaults.AuthenticationScheme*/); //так як вже  задано як default
            }

            var identity = new ClaimsIdentity(
                 authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                 nameType: Claims.Name,
                 roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, result.Principal.GetClaim(Claims.Subject))
                    .SetClaim(Claims.Email, "admin@gmail") //cab be something like that await _userManager.GetEmailAsync(user)
                    .SetClaim(Claims.Name, result.Principal.GetClaim(Claims.Name)) //can be something like that await _userManager.GetUserNameAsync(user)
                    .SetClaim(Claims.Role, result.Principal.GetClaim(Claims.Role));

            // Note: in this sample, the granted scopes match the requested scope
            // but you may want to allow the user to uncheck specific scopes.
            // For that, simply restrict the list of scopes before calling SetScopes.
            identity.SetScopes(request.GetScopes());
            identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }


        [HttpPost("~/token")]
        public async ValueTask<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                 throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsClientCredentialsGrantType())
            {
                var clientId = request.ClientId;
                var identity = new ClaimsIdentity(authenticationType: TokenValidationParameters.DefaultAuthenticationType);

                identity.SetClaim(Claims.Subject, clientId);
                identity.SetScopes(request.GetScopes());

                var principals = new ClaimsPrincipal(identity);

                return SignIn(principals, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            if (request.IsRefreshTokenGrantType() || request.IsAuthorizationCodeGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                var identity = new ClaimsIdentity(result.Principal!.Claims,
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

                identity.SetClaim(Claims.Subject, result.Principal.GetClaim(Claims.Subject))
                    .SetClaim(Claims.Email, "admin@gmail")
                    .SetClaim(Claims.Name, result.Principal.GetClaim(Claims.Name))
                    .SetClaim(Claims.Role, result.Principal.GetClaim(Claims.Role));

                identity.SetScopes(request.GetScopes());
                identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                var res = SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                return res;
            }

            throw new NotImplementedException("The specified grant type is not implemented.");
        }
    }
}
