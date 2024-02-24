namespace FleszynChat.Classes
{

    public class User
    {
        public int Id { get; set; }      
        public string Password { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? ProfilePicturePath { get; set; }

        public User(int id, string username, string password, string name, string surname, string email, string? profilePicturePath)
        {
            Id = id;
            Username = username;
            Password = password;
            Name = name;
            Surname = surname;
            Email = email;
            ProfilePicturePath = profilePicturePath;
        }
    }
}
