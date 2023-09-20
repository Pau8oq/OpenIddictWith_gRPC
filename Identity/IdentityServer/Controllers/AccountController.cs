using IdentityServer.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (model.Email == "admin@gmail.com" && model.Password == "admin")
            {

                var claims = new List<Claim>
                {
                    //The "sub" claim typically contains a unique identifier for the user,
                    //and its format may vary depending on the OP and the type of identifier being used.
                    //Commonly, it could be a username, an email address, a user ID, or even a universally unique identifier (UUID)
                    new ("sub", "1031"),
                    new ("name", "Admin Name"),
                    new ("role", "Admin")
                };

                //https://youtu.be/02Yh3sxzAYI?si=yrXia-lg5lHobrwF&t=1578
                var ci = new ClaimsIdentity(claims, "pwd", "name", "role");
                var cp = new ClaimsPrincipal(ci);

                await HttpContext.SignInAsync(cp);

                return LocalRedirect(returnUrl);
            }

            ModelState.AddModelError("", "Something went wrong!!");
            return View(model);
        }
    }
}
