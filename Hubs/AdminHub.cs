using FleszynChat.Classes;
using FleszynChat.Scripts;
using FleszynChatt.Classes;
using FleszynChatt.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace FleszynChatt.Hubs
{
    public class AdminHub : Hub
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public AdminHub(IWebHostEnvironment webHostEnvironment, IHubContext<ChatHub> chatHubContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _chatHubContext = chatHubContext;
        }

        public async Task SendUserData()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateUserList", GlobalData.Users);
        }

        public async Task SendUserChats()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateChatsList", GlobalData.Chats);
        }

        public async Task SendBackupFiles()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string backupDirectory = Path.Combine(currentDirectory, "Backups");
            try
            {
                string[] filePaths = Directory.GetFiles(backupDirectory);
                string[] fileNames = Directory.GetFiles(backupDirectory)
    .Select(filePath => GetFileNameFromPath(filePath))
    .ToArray();
                await Clients.Client(Context.ConnectionId).SendAsync("UpdateBackupList", fileNames);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while getting files: {ex.Message}");
            }           
        }

        public async Task DeleteUser(int userID)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            Database.DeleteUser(connection, userID);
            await _chatHubContext.Clients.All.SendAsync("Update");
        }

        public async Task SetPassword(string password)
        {
            GlobalData.defaultPassword = password;
        }

        public async Task ResetUserPassword(int userID)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            Database.ResetUser(connection, userID);
        }

        public async Task DeleteChat(int chatID)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            Database.DeleteChat(connection, chatID);
            await _chatHubContext.Clients.All.SendAsync("Update");
        }

        public async Task ADConnection(string domainController, string login, string password)
        {
            Timer timer = new Timer(ActiveDirectory.ConnectToAD, new Tuple<string, string, string>(domainController, login, password), TimeSpan.Zero, TimeSpan.FromMinutes(15));

            await Task.Delay(Timeout.Infinite);
        }

        public async Task RestoreBackup(string filename)
        {
            Database.RestoreDataBase(filename);
        }

        public override async Task OnConnectedAsync()
        {
            await SendBackupFiles();
            await SendUserData();
            await SendUserChats();
            await SendBackupFiles();
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        public string GetFileNameFromPath(string filePath)
        {
            // Find the last index of the directory separator character '\'
            int lastIndex = filePath.LastIndexOf('\\');

            // If the directory separator character is found, extract the file name
            if (lastIndex >= 0)
            {
                return filePath.Substring(lastIndex + 1);
            }

            // If the directory separator character is not found, return the original filePath
            return filePath;
        }
    }
}

