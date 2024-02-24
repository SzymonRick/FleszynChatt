namespace FleszynChat.Classes
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<int> Uids { get; set; }

        public Chat(int gid, string name, List<int> uids)
        {
            Id = gid;
            Name = name;
            Uids = uids;
        }
    }
}
