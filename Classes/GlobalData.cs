using FleszynChat.Classes;
using FleszynChat.Scripts;
using MySql.Data.MySqlClient;

namespace FleszynChatt.Classes
{
    public static class GlobalData
    {
        public static Dictionary<int, User> Users { get; set; } = new Dictionary<int, User>();
        public static Dictionary<int, Chat> Chats { get; set; } = new Dictionary<int, Chat>();
        public static Dictionary<int, string> Connections { get; set; } = new Dictionary<int, string>();
        public static string defaultProfilePicturePath {  get; set; }
    }
}
