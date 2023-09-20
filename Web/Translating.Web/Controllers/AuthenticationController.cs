using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Client.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;

namespace Translating.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        //цей метод ми можемо використовувати коли DefaultChallengeScheme не заданий а за все  відповідає 
        //CookieAuthenticationDefaults.AuthenticationScheme і ми спочатку переходимо на сторінку логування і тут вже викликаємо Challenge який нам підходить
        [HttpGet("~/login")]
        public ActionResult LogIn(string provider, string returnUrl)
        {
            var properties = new AuthenticationProperties(new Dictionary<string, string?>
            {
                // Note: when only one client is registered in the client options,
                // specifying the issuer URI or the provider name is not required.
                [OpenIddictClientAspNetCoreConstants.Properties.ProviderName] = provider
            })
            {
                // Only allow local return URLs to prevent open redirect attacks.
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            };

            // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
            return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            //// Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            //// Note: the authenticationType parameter must match the value configured in Startup.cs
            //return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // Note: this controller uses the same callback action for all providers
        // but for users who prefer using a different action per provider,
        // the following action can be split into separate actions.
        [HttpGet("~/callback/login/{provider}"), HttpPost("~/callback/login/{provider}"), IgnoreAntiforgeryToken]
        public async Task<ActionResult> LogInCallback()
        {
            // Retrieve the authorization data validated by OpenIddict as part of the callback handling.
            var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

            // Multiple strategies exist to handle OAuth 2.0/OpenID Connect callbacks, each with their pros and cons:
            //
            //   * Directly using the tokens to perform the necessary action(s) on behalf of the user, which is suitable
            //     for applications that don't need a long-term access to the user's resources or don't want to store
            //     access/refresh tokens in a database or in an authentication cookie (which has security implications).
            //     It is also suitable for applications that don't need to authenticate users but only need to perform
            //     action(s) on their behalf by making API calls using the access token returned by the remote server.
            //
            //   * Storing the external claims/tokens in a database (and optionally keeping the essential claims in an
            //     authentication cookie so that cookie size limits are not hit). For the applications that use ASP.NET
            //     Core Identity, the UserManager.SetAuthenticationTokenAsync() API can be used to store external tokens.
            //
            //     Note: in this case, it's recommended to use column encryption to protect the tokens in the database.
            //
            //   * Storing the external claims/tokens in an authentication cookie, which doesn't require having
            //     a user database but may be affected by the cookie size limits enforced by most browser vendors
            //     (e.g Safari for macOS and Safari for iOS/iPadOS enforce a per-domain 4KB limit for all cookies).
            //
            //     Note: this is the approach used here, but the external claims are first filtered to only persist
            //     a few claims like the user identifier. The same approach is used to store the access/refresh tokens.

            // Important: if the remote server doesn't support OpenID Connect and doesn't expose a userinfo endpoint,
            // result.Principal.Identity will represent an unauthenticated identity and won't contain any claim.
            //
            // Such identities cannot be used as-is to build an authentication cookie in ASP.NET Core (as the
            // antiforgery stack requires at least a name claim to bind CSRF cookies to the user's identity) but
            // the access/refresh tokens can be retrieved using result.Properties.GetTokens() to make API calls.
            if (result.Principal is not ClaimsPrincipal { Identity.IsAuthenticated: true })
            {
                throw new InvalidOperationException("The external authorization data cannot be used for authentication.");
            }

            // Build an identity based on the external claims and that will be used to create the authentication cookie.
            var identity = new ClaimsIdentity(authenticationType: "ExternalLogin");

            // By default, OpenIddict will automatically try to map the email/name and name identifier claims from
            // their standard OpenID Connect or provider-specific equivalent, if available. If needed, additional
            // claims can be resolved from the external identity and copied to the final authentication cookie.
            identity.SetClaim(Claims.Email, result.Principal.GetClaim(Claims.Email))
                    .SetClaim(Claims.Name, result.Principal.GetClaim(Claims.Name))
                    .SetClaim(Claims.Role, result.Principal.GetClaim(Claims.Role))
                    .SetClaim(Claims.Subject, result.Principal.GetClaim(Claims.Subject));

            // Preserve the registration identifier to be able to resolve it later.
            identity.SetClaim(Claims.Private.RegistrationId, result.Principal.GetClaim(Claims.Private.RegistrationId));

            // Build the authentication properties based on the properties that were added when the challenge was triggered.
            var properties = new AuthenticationProperties(result.Properties.Items)
            {
                RedirectUri = result.Properties.RedirectUri ?? "/"
            };

            // If needed, the tokens returned by the authorization server can be stored in the authentication cookie.
            //
            // To make cookies less heavy, tokens that are not used are filtered out before creating the cookie.
            properties.StoreTokens(result.Properties.GetTokens().Where(token => token switch
            {
                // Preserve the access, identity and refresh tokens returned in the token response, if available.
                {
                    Name: OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken or
                          OpenIddictClientAspNetCoreConstants.Tokens.BackchannelIdentityToken or
                          OpenIddictClientAspNetCoreConstants.Tokens.RefreshToken
                } => true,

                // Ignore the other tokens.
                _ => false
            }));

            // Ask the default sign-in handler to return a new cookie and redirect the
            // user agent to the return URL stored in the authentication properties.
            //
            // For scenarios where the default sign-in handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            return SignIn(new ClaimsPrincipal(identity), properties);
        }
    }
}
