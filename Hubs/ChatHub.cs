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
        public async Task SendMessage(int senderId, int recipientId, string messageContent, byte[] fileBytes)
        {
            Console.WriteLine(messageContent);
            // Save the file
            string filePath = null;
            if (fileBytes != null && fileBytes.Length > 0)
            {
                filePath = await SaveFile(fileBytes);
            }

            // Save message to the database
            var message = new Message(senderId, recipientId, DateTime.Now, messageContent, filePath);
            MySqlConnection connection = Database.ConnectDatabase();
            Database.InsertMessage(connection, message);

            // Broadcast the received message to all clients in the recipient chat
            Tuple<string, Message> myTuple = Tuple.Create(GlobalData.Users[senderId].Name + " " + GlobalData.Users[senderId].Surname, message);
            await Clients.Group(recipientId.ToString()).SendAsync("ReceiveMessage", myTuple);
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

        // Function to save a file
        private async Task<string> SaveFile(byte[] fileBytes)
        {
            string fileName = Guid.NewGuid().ToString(); // Generate a unique file name
            string filePath = Path.Combine("your_file_directory", fileName); // Specify your file directory
            await File.WriteAllBytesAsync(filePath, fileBytes);
            return filePath;
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

