using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FleszynChatt.Models;
using FleszynChat.Scripts;
using MySql.Data.MySqlClient;
using FleszynChatt.Classes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FleszynChatt.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated) {
                if (HttpContext.User.Identity.Name == "AdministratorChat")
                {
                    return RedirectToAction("ControlPanel", "Home");
                }
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelLogin)
        {
            List<Claim> claims = new List<Claim>();
            MySqlConnection connection = Database.ConnectDatabase();
            if (Database.LoginQuerry(connection,modelLogin.Login, modelLogin.Password))
            {

                claims.Add(new Claim(ClaimTypes.Name, modelLogin.Login));


                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties() { 
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                if (modelLogin.Password == GlobalData.defaultPassword)
                {
                    return RedirectToAction("Privacy", "Home");
                }
                //return RedirectToAction("ControlPanel", "Home");
                return RedirectToAction("Index", "Home");               
            }

            if (modelLogin.Login == "AdministratorChat" && Hasher.VerifyPassword(modelLogin.Password, GlobalData.AdminPasswordhash, GlobalData.salt))
            {
                claims.Add(new Claim(ClaimTypes.Name, modelLogin.Login));


                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = modelLogin.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                return RedirectToAction("ControlPanel", "Home");
            }

            ViewData["ValidateMessage"] = "*Niepoprawne dane logowania";
            return View();           
        }
    }
}