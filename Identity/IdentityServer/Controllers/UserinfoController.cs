
namespace IdentityServer.Controllers
{
    public class UserinfoController : Controller
    {
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("~/userinfo"), HttpPost("~/userinfo")]
        [IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> Userinfo()
        {
            var userId = User.GetClaim(Claims.Subject);

            //in real project we should do this
            //var user = await _userManager.FindByIdAsync(User.GetClaim(Claims.Subject));

            if (userId is null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [Claims.Subject] = userId
            };

            if (User.HasScope(Scopes.Email))
            {
                //claims[Claims.Email] = await _userManager.GetEmailAsync(user);
                //claims[Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);

                var e = User.GetClaim(Claims.Email);
                claims[Claims.Email] = "admin@gmail";
            }

            //if (User.HasScope(Scopes.Phone))
            //{
            //    claims[Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
            //    claims[Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
            //}

            if (User.HasScope(Scopes.Roles))
            {
                //claims[Claims.Role] = await _userManager.GetRolesAsync(user);
                var r = User.GetClaim(Claims.Role);
                claims[Claims.Role] = "admin";
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }
    }
}
