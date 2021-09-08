using System;

namespace Photofy_ASPNET_1.Models
{
    public class User
    {
        public Guid Id {get;set;}
        public string Name {get;set;}
        public string LobbyId {get;set;}
        public string Image {get;set;}
    }
}