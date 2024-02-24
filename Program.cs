using Microsoft.AspNetCore.Authentication.Cookies;
using FleszynChat.Classes;
using FleszynChat.Scripts;
using MySql.Data.MySqlClient;
using FleszynChatt.Models;
using FleszynChatt.Classes;
using System;
using FleszynChatt.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option => { option.LoginPath = "/Access/Login"; option.ExpireTimeSpan = TimeSpan.FromMinutes(20);});
builder.Services.AddAntiforgery(options =>{options.HeaderName = "X-CSRF-TOKEN";});
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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