using FleszynChat.Classes;
using FleszynChat.Scripts;
using FleszynChatt.Classes;
using FleszynChatt.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Runtime.CompilerServices;

namespace FleszynChatt.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChatHub(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendMessage(int senderId, int recipientId, string? messageContent, string? fileName, string? fileContentBase64)
        {

            string filePath = null;
            if (fileContentBase64 != null && fileContentBase64.Length > 0) {
                byte[] fileBytes = Convert.FromBase64String(fileContentBase64);

                // Save the file
                
                if (!string.IsNullOrEmpty(fileName) && fileBytes.Length > 0)
                {
                    filePath = await SaveFile(fileName, fileBytes);
                }
            }
            
            // Save message to the database
            Console.WriteLine(filePath);
            var message = new Message(senderId, recipientId, DateTime.Now, messageContent, filePath);
            MySqlConnection connection = Database.ConnectDatabase();
            Database.InsertMessage(connection, message);

            // Broadcast the received message to all clients in the recipient chat
            Tuple<string, Message> myTuple = Tuple.Create(GlobalData.Users[senderId].Name + " " + GlobalData.Users[senderId].Surname, message);
            await Clients.Group(recipientId.ToString()).SendAsync("ReceiveMessage", myTuple);
        }

        // Function to save a file
        public async Task<string> SaveFile(string fileName, byte[] fileBytes)
        {
            // Get the web root path
            string webRootPath = _webHostEnvironment.WebRootPath;

            // Specify the directory structure within the web root where you want to save the file
            string relativeFilePath = "ChatFiles";

            // Create a file name with datetime component
            string dateTimeComponent = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string newFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{dateTimeComponent}{Path.GetExtension(fileName)}";

            // Combine the web root path and the relative file path to create the file save path
            string filePath = Path.Combine(webRootPath, relativeFilePath, newFileName);

            // Write the file bytes to the specified file path
            await File.WriteAllBytesAsync(filePath, fileBytes);

            // Get the indices of the last two directory separator characters in the filePath
            int lastSeparatorIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
            int secondLastSeparatorIndex = filePath.Substring(0, lastSeparatorIndex).LastIndexOf(Path.DirectorySeparatorChar);

            // Extract the substring after the second-to-last directory separator character
            string trimmedFilePath = filePath.Substring(secondLastSeparatorIndex + 1);

            // Replace backslashes with forward slashes
            trimmedFilePath = '/' + trimmedFilePath.Replace(Path.DirectorySeparatorChar, '/');
            Console.WriteLine(trimmedFilePath);

            return trimmedFilePath;
        }

        public async Task SendUserData()
        {
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateUserList", GlobalData.Users);
        }

        public async Task SendContactsData()
        {
            MySqlConnection connection = Database.ConnectDatabase();
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateContactList", Database.GetContacts(connection, Context.User.Identity.Name) );
        }

        public async Task UpdateActiveTime(int chatId, int userId)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            Database.UpdateLastActiveDateTime(connection, chatId, userId);
            connection = Database.ConnectDatabase();
            await Clients.Client(GlobalData.Connections[userId]).SendAsync("UpdateContactList", Database.GetContacts(connection, Context.User.Identity.Name));
        }    

        public async Task CreateChat(string chatname, int[] users)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            Database.InsertChatAndParticipants(connection, chatname, users);
            connection = Database.ConnectDatabase();
            GlobalData.Chats = Database.GetChats(connection);
            await Clients.All.SendAsync("Update");
        }

        public override async Task OnConnectedAsync()
        {
            User user = GlobalData.Users.Values.FirstOrDefault(u => u.Username == Context.User.Identity.Name);
            GlobalData.Connections.Add(user.Id, Context.ConnectionId);
            await Clients.Client(Context.ConnectionId).SendAsync("SetUserId", user.Id);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User user = GlobalData.Users.Values.FirstOrDefault(u => u.Username == Context.User.Identity.Name);
            MySqlConnection connection = Database.ConnectDatabase();
            GlobalData.Connections.Remove(user.Id);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinChatGroup(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task GetChatMessages(int chatID , int step)
        {
            MySqlConnection connection = Database.ConnectDatabase();
            await Clients.Client(Context.ConnectionId).SendAsync("UpdateMessages", Database.GetMessages(connection, chatID, step));
        }

        public async Task SelectUserContact(int someoneID, int userID)
        {
            foreach (KeyValuePair<int, Chat> pair in GlobalData.Chats)
            {
                if(pair.Value.Uids.Count == 2 && pair.Value.Uids.Contains(someoneID) && pair.Value.Uids.Contains(userID))
                {
                    Console.WriteLine(pair.Value.ToString());
                    await Clients.Client(Context.ConnectionId).SendAsync("OpenChat", pair.Value.Id);
                    return;
                }
            }

            int[] users = { someoneID, userID };
            MySqlConnection connection = Database.ConnectDatabase();
            Database.InsertChatAndParticipants(connection, GlobalData.Users[someoneID].Name + ", " + GlobalData.Users[userID].Name, users);
            connection = Database.ConnectDatabase();
            GlobalData.Chats = Database.GetChats(connection);
            await Clients.All.SendAsync("Update");
        }
    }
}

