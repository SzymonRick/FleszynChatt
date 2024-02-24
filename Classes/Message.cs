namespace FleszynChat.Classes
{
    public class Message
    {
        public int SenderID { get; set; }
        public int RecipientID { get; set; }
        public DateTime SendDate { get; set; }
        public string? MessageText { get; set; }
        public string? FilePath { get; set; }

        public Message(int senderId, int recipientId, DateTime sendDate, string? messageText, string? filePath)
        {
            SenderID = senderId;
            RecipientID = recipientId;
            SendDate = sendDate;
            MessageText = messageText;
            FilePath = filePath;
        }
    }
}
