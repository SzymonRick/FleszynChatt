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
using Microsoft.AspNetCore.Identity;

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

string currentDirectory = Directory.GetCurrentDirectory();
string saveDirectory = Path.Combine(currentDirectory, "Data");
string filePath = Path.Combine(saveDirectory, "data.fls");

if (!File.Exists(filePath))
{
    try
    {
        // Create the Data directory if it doesn't exist
        Directory.CreateDirectory(saveDirectory);

        // Create the data.fls file
        File.Create(filePath).Close();

        Console.WriteLine("The file 'data.fls' has been created in the directory.");
        Console.WriteLine("Enter admin password:");
        string adminPassword = Console.ReadLine();

        // Hash the password and get the hashed password and salt
        (string hashedPassword, string salt) = Hasher.HashPassword(adminPassword);

        // Store hashed password and salt in GlobalData
        GlobalData.AdminPasswordhash = hashedPassword;
        GlobalData.salt = Convert.FromBase64String(salt);

        // Write hashed password and salt to the file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(GlobalData.AdminPasswordhash);
            writer.WriteLine(Convert.ToBase64String(GlobalData.salt));
        }

        Console.Clear();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the file: {ex.Message}");
    }
}
else
{
    try
    {
        // Read hashed password and salt from the file
        using (StreamReader reader = new StreamReader(filePath))
        {
            GlobalData.AdminPasswordhash = reader.ReadLine();
            GlobalData.salt = Convert.FromBase64String(reader.ReadLine());
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while reading from the file: {ex.Message}");
    }
}

Timer timer = new Timer(Database.BackupDataBase, null, TimeSpan.Zero, TimeSpan.FromMinutes(60));

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.MapHub<AdminHub>("/adminHub");

app.Run();

Thread.Sleep(Timeout.Infinite);