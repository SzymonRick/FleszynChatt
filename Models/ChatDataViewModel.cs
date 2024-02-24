using FleszynChat.Classes;

namespace FleszynChatt.Models
{
    public class ChatDataViewModel
    {
        public int UserId { get; set; }

        public ChatDataViewModel(int userId)
        {
            UserId= userId;
        }
    }  
}
