namespace FleszynChat.Classes
{
    public class Contact
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastActiveTime { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }
}
