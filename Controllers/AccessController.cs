using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FleszynChatt.Models;
using FleszynChat.Scripts;
using MySql.Data.MySqlClient;

namespace FleszynChatt.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated) {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelLogin)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            if (Database.LoginQuerry(connection,modelLogin.Login, modelLogin.Password))
            {
                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, modelLogin.Login)
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties() { 
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
                return RedirectToAction("Index", "Home");
            }

            ViewData["ValidateMessage"] = "*Niepoprawne dane logowania";
            return View();
        }
    }
}
