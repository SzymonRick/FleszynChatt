using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net;
using FleszynChatt.Hubs;
using FleszynChatt.Models;
using FleszynChatt.Classes;
using MySql.Data.MySqlClient;
using FleszynChat.Classes;
using FleszynChat.Scripts;
using Microsoft.AspNet.SignalR;
using Owin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Access/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = null; // Set the maximum WebSocket message size
});

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
});

builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
    options.HttpsPort = 5001;
});

builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(5000); // Listen on port 5000 for HTTP
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps(); // Listen on port 5001 for HTTPS
    });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

Database.CreateDataBase();
MySqlConnection connection = Database.ConnectDatabase();
GlobalData.Users = Database.GetUsers(connection);
connection = Database.ConnectDatabase();
GlobalData.Chats = Database.GetChats(connection);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.MapHub<ChatHub>("/chatHub");

app.Run();