using FleszynChatt.Models;
using FleszynChat.Scripts;
using FleszynChat.Classes;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using FleszynChatt.Classes;
using MySql.Data.MySqlClient;
using FleszynChatt.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;

namespace FleszynChatt.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<ChatHub> chatHubContext)
        {
            _logger = logger;
            _chatHubContext = chatHubContext;
        }

        public ActionResult Index()
        {

            User user = GlobalData.Users.Values.FirstOrDefault(u => u.Username == HttpContext.User.Identity.Name);

            if (user == null){
                return View();
            }
            
            ChatDataViewModel chatData = new ChatDataViewModel(user.Id);

            return View(chatData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ControlPanel() { 
            if(HttpContext.User.Identity.Name == "AdministratorChat")
            {
                return View();
            }
            return RedirectToAction("Login", "Access");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Privacy(PasswordViewModel viewModel)
        {
            if(viewModel.Password1 == viewModel.Password2 && viewModel != null) { 

                MySqlConnection connection = Database.ConnectDatabase();
                Database.SetPasswordUser(connection, HttpContext.User.Identity.Name, viewModel.Password1);
                return RedirectToAction("Index", "Home");
            }
            ViewData["ValidateMessage"] = "*Ró¿ne has³a";
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login","Access");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
