using System;

namespace Photofy.Models
{
    public class User
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public string LobbyId { get; set; }
        public bool IsReady { get; set; }
        public string Image { get; set; }
        public bool SentImage { get; set; }
    }
}