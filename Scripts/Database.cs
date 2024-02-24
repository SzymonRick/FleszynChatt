using FleszynChat.Classes;
using Humanizer;
using MySql.Data.MySqlClient;
using System.Data;

namespace FleszynChat.Scripts
{
    public class Database
    {
        static string server = "localhost";
        static string username = "root";
        static string databaseName = "ChatApp";

        static string connectionString = $"Server={server};Uid={username};Database={databaseName};";

        static public MySqlConnection ConnectDatabase()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();    
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to the database: {ex.Message}");
                connection = null;
            }
            return connection;
        }

        static public void CreateDataBase()
        {
            MySqlConnection con = new MySqlConnection($"Server={server};Uid={username};");
            using (con)
            {
                try
                {
                    con.Open();

                    string createDatabaseQuery = "CREATE DATABASE IF NOT EXISTS ChatApp;";
                    MySqlCommand createDatabaseCommand = new MySqlCommand(createDatabaseQuery, con);
                    createDatabaseCommand.ExecuteNonQuery();

                    string useDatabaseQuery = "USE ChatApp;";
                    MySqlCommand useDatabaseCommand = new MySqlCommand(useDatabaseQuery, con);
                    useDatabaseCommand.ExecuteNonQuery();

                    string createUsersTableQuery = "CREATE TABLE IF NOT EXISTS Users (" +
                                                    "UserID INT AUTO_INCREMENT PRIMARY KEY," +
                                                    "Username VARCHAR(255) UNIQUE," +
                                                    "Password VARCHAR(255)," +
                                                    "Email VARCHAR(255)," +
                                                    "Name VARCHAR(255)," +
                                                    "Surname VARCHAR(255)," +
                                                    "ProfilePicturePath VARCHAR(255) NULL" +
                                                    ");";
                    MySqlCommand createUsersTableCommand = new MySqlCommand(createUsersTableQuery, con);
                    createUsersTableCommand.ExecuteNonQuery();

                    string createChatsTableQuery = "CREATE TABLE IF NOT EXISTS Chats (" +
                                                    "ChatID INT AUTO_INCREMENT PRIMARY KEY," +
                                                    "ChatName VARCHAR(255)" +
                                                    ");";
                    MySqlCommand createChatsTableCommand = new MySqlCommand(createChatsTableQuery, con);
                    createChatsTableCommand.ExecuteNonQuery();

                    string createChatParticipantsTableQuery = "CREATE TABLE IF NOT EXISTS ChatParticipants (" +
                                                                "ChatID INT," +
                                                                "UserID INT," +
                                                                "PRIMARY KEY (ChatID, UserID)," +
                                                                "FOREIGN KEY (ChatID) REFERENCES Chats(ChatID)," +
                                                                "FOREIGN KEY (UserID) REFERENCES Users(UserID)," +
                                                                "LastActiveDateTime DATETIME" +
                                                                ");";
                    MySqlCommand createChatParticipantsTableCommand = new MySqlCommand(createChatParticipantsTableQuery, con);
                    createChatParticipantsTableCommand.ExecuteNonQuery();

                    string createMessagesTableQuery = "CREATE TABLE IF NOT EXISTS Messages (" +
                                                        "MessageID INT AUTO_INCREMENT PRIMARY KEY," +
                                                        "SenderUserID INT," +
                                                        "RecipientID INT," +
                                                        "SendDate DATETIME," +
                                                        "MessageText TEXT NULL," +
                                                        "FilePath VARCHAR(255) NULL," +
                                                        "FOREIGN KEY (SenderUserID) REFERENCES Users(UserID)," +
                                                        "FOREIGN KEY (RecipientID) REFERENCES Chats(ChatID)" +
                                                        ");";
                    MySqlCommand createMessagesTableCommand = new MySqlCommand(createMessagesTableQuery, con);
                    createMessagesTableCommand.ExecuteNonQuery();

                    con.Close();
                    con.Dispose();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                con.Dispose();
            }
        }

        static public Dictionary<int, User> GetUsers(MySqlConnection con)
        {
            Dictionary<int, User> users = new Dictionary<int, User>();
            try
            {
                using (con)
                {
                    if (con != null)
                    {
                        string query = "SELECT UserID, Username, Password, Email, Name, Surname, ProfilePicturePath FROM Users";

                        using (MySqlCommand command = new MySqlCommand(query, con))
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int userId = reader.GetInt32("UserID");
                                string username = reader.GetString("Username");
                                string password = "*";
                                string email = reader.GetString("Email");
                                string name = reader.GetString("Name");
                                string surname = reader.GetString("Surname");
                                string profilePicturePath = reader.IsDBNull("ProfilePicturePath") ? null : reader.GetString("ProfilePicturePath");

                                User user = new User(userId, username, password, name, surname, email, profilePicturePath);
                                users.Add(userId, user);

                            }
                        }

                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving users: {ex.Message}");
                con.Dispose();
            }

            return users;
        }

        static public Dictionary<int, Chat> GetChats(MySqlConnection con)
        {
            Dictionary<int, Chat> chats = new Dictionary<int, Chat>();
            try
            {
                using (con)
                {
                    if (con != null)
                    {
                        string query = "SELECT c.ChatID, c.ChatName, GROUP_CONCAT(cp.UserID) AS ParticipantIDs " +
                                       "FROM Chats c " +
                                       "LEFT JOIN ChatParticipants cp ON c.ChatID = cp.ChatID " +
                                       "GROUP BY c.ChatID";

                        using (MySqlCommand command = new MySqlCommand(query, con))
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int chatId = reader.GetInt32("ChatID");
                                string chatName = reader.GetString("ChatName");
                                string participantIds = reader.GetString("ParticipantIDs");

                                List<int> uids = new List<int>(Array.ConvertAll(participantIds.Split(','), int.Parse));

                                Chat chat = new Chat(chatId, chatName, uids);
                                chats.Add(chatId, chat);
                               
                            }
                        }

                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving chats: {ex.Message}");
                con.Dispose();
            }
            return chats;
        }

        static public Dictionary<int, Contact> GetContacts(MySqlConnection con, string username)
        {
            Dictionary<int, Contact> contacts = new Dictionary<int, Contact>();
            try
            {
                string query = @"
                SELECT c.ChatID, c.ChatName, m.MessageText, m.SendDate, cp.LastActiveDateTime
                FROM Chats c
                INNER JOIN ChatParticipants cp ON c.ChatID = cp.ChatID
                LEFT JOIN Messages m ON c.ChatID = m.RecipientID AND m.SendDate = (
                SELECT MAX(SendDate) FROM Messages WHERE RecipientID = c.ChatID
                )
                WHERE cp.UserID = (
                SELECT UserID FROM Users WHERE Username = @Username
                )
                ORDER BY cp.LastActiveDateTime DESC;
                ";

                using (MySqlCommand command = new MySqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int chatId = reader.GetInt32("ChatID");
                            string chatName = reader.GetString("ChatName");
                            string? lastMessage = reader.IsDBNull("MessageText") ? null : reader.GetString("MessageText");
                            DateTime lastActiveTime = reader.GetDateTime("LastActiveDateTime");
                            DateTime lastMessageTime = reader.IsDBNull("SendDate") ? DateTime.MinValue : reader.GetDateTime("SendDate");

                            Contact contact = new Contact
                            {
                                Id = chatId,
                                Name = chatName,
                                LastMessage = lastMessage,
                                LastActiveTime = lastActiveTime,
                                LastMessageTime = lastMessageTime
                            };

                            contacts.Add(chatId, contact);
                            
                        }
                    }

                    con.Close();
                    con.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving contacts: {ex.Message}");
                con.Dispose();
            }
            return contacts;
        }

        static public bool LoginQuerry(MySqlConnection con, string username, string password)
        {

            bool isAuthenticated = false;

            try
            {
                using (con)
                {
                    if(con != null)
                    {
                        string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Username", username);
                            cmd.Parameters.AddWithValue("@Password", password);

                            int count = Convert.ToInt32(cmd.ExecuteScalar());

                            if (count > 0)
                            {
                                isAuthenticated = true;
                            }

                            
                        }

                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error retrieving user login information: {ex.Message}");
                con.Dispose();
            }
            return isAuthenticated;
        }

        static public void InsertMessage(MySqlConnection con, Message message)
        {
            try
            {
                using (con)
                {
                    if (con != null)
                    {
                        string query = @"
                        INSERT INTO Messages (SenderUserID, RecipientID, SendDate, MessageText, FilePath)
                        VALUES (@SenderUserID, @RecipientID, @SendDate, @MessageText, @FilePath);
                    ";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@SenderUserID", message.SenderID);
                            cmd.Parameters.AddWithValue("@RecipientID", message.RecipientID);
                            cmd.Parameters.AddWithValue("@SendDate", message.SendDate);
                            cmd.Parameters.AddWithValue("@MessageText", message.MessageText);
                            cmd.Parameters.AddWithValue("@FilePath", message.FilePath != null ? (object)message.FilePath : DBNull.Value);

                            cmd.ExecuteNonQuery();
                            

                        }

                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting message: {ex.Message}");
                con.Dispose();
            }
        }

        static public void InsertChatAndParticipants(MySqlConnection con, string chatName, int[] participantUserIds)
        {
            try
            {
                using (con)
                {
                    if (con != null)
                    {
                        // Insert chat name into Chats table and retrieve generated ChatID
                        string chatInsertQuery = @"
                    INSERT INTO Chats (ChatName)
                    VALUES (@ChatName);
                    SELECT LAST_INSERT_ID();
                ";

                        using (MySqlCommand chatInsertCmd = new MySqlCommand(chatInsertQuery, con))
                        {
                            chatInsertCmd.Parameters.AddWithValue("@ChatName", chatName);

                            int chatId = Convert.ToInt32(chatInsertCmd.ExecuteScalar());

                            // Insert participants into ChatParticipants table using a foreach loop
                            string participantsInsertQuery = @"
                        INSERT INTO ChatParticipants (ChatID, UserID, LastActiveDateTime)
                        VALUES (@ChatID, @UserID, @LastActiveDateTime);
                    ";

                            using (MySqlCommand participantsInsertCmd = new MySqlCommand(participantsInsertQuery, con))
                            {
                                participantsInsertCmd.Parameters.Add("@ChatID", MySqlDbType.Int32).Value = chatId;
                                participantsInsertCmd.Parameters.Add("@UserID", MySqlDbType.Int32);
                                participantsInsertCmd.Parameters.Add("@LastActiveDateTime", MySqlDbType.DateTime).Value = DateTime.Now;

                                foreach (int userId in participantUserIds)
                                {
                                    participantsInsertCmd.Parameters["@UserID"].Value = userId;
                                    participantsInsertCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        con.Close();
                        con.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting chat and participants: {ex.Message}");
            }
        }

        static public void UpdateLastActiveDateTime(MySqlConnection con, int chatID, int userID)
        {
            try
            {
                using (con)
                {
                    if (con != null)
                    {
                        string query = @"
                UPDATE ChatParticipants
                SET LastActiveDateTime = CURRENT_TIMESTAMP
                WHERE ChatID = @ChatID AND UserID = @UserID;
            ";

                        using (MySqlCommand cmd = new MySqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@ChatID", chatID);
                            cmd.Parameters.AddWithValue("@UserID", userID);

                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last active date time: {ex.Message}");
            }
        }

        static public List<Tuple<string, Message>> GetMessages(MySqlConnection con, int recipientId, int skipCount)
        {
            List<Tuple<string, Message>> messages = new List<Tuple<string, Message>>();

            try
            {
                string query = @"
    SELECT m.SenderUserID, m.RecipientID, m.SendDate, m.MessageText, m.FilePath, 
           CONCAT(u.Name, ' ', u.Surname) AS SenderFullName
    FROM Messages m
    INNER JOIN Users u ON m.SenderUserID = u.UserID
    WHERE m.RecipientID = @RecipientID
    ORDER BY m.SendDate DESC
    LIMIT 50
    OFFSET @SkipCount;
";

                using (MySqlCommand command = new MySqlCommand(query, con))
                {
                    command.Parameters.AddWithValue("@RecipientID", recipientId);
                    command.Parameters.AddWithValue("@SkipCount", skipCount);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int senderId = reader.GetInt32("SenderUserID");
                            int recipientID = reader.GetInt32("RecipientID");
                            DateTime sendDate = reader.GetDateTime("SendDate");
                            string messageText = reader.IsDBNull("MessageText") ? null : reader.GetString("MessageText");
                            string filePath = reader.IsDBNull("FilePath") ? null : reader.GetString("FilePath");
                            string senderFullName = reader.GetString("SenderFullName");

                            Message message = new Message(senderId, recipientID, sendDate, messageText, filePath);
                            messages.Add(new Tuple<string, Message>(senderFullName, message));
                        }
                    }
                }
                con.Close();
                con.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving messages: {ex.Message}");
                con.Dispose();
            }

            return messages;
        }
    }
}
