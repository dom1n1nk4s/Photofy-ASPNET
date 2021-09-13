using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Photofy_ASPNET_1.Models;

namespace Photofy.Hubs
{
    public class GameHub : Hub<IClient>
    {
        private readonly Context context;
        public GameHub(Context context)
        {
            this.context = context;
        }
        public string Test()
        {
            Console.WriteLine("Test");
            return "Hello World";
        }
        public async Task<string> NewUser(string name)
        {
            var user = new User
            {
                Name = name,
                LobbyId = GenerateLobbyId(), // need to make sure its unique
            };
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");
            return user.LobbyId;
        }


        public async Task JoinLobby(Guid userId, string lobbyId)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) throw new HubException("No such user found");
            var userToJoin = context.Users.FirstOrDefault(u => u.LobbyId == lobbyId);
            if (userToJoin == null) throw new HubException("No such lobby found");
            user.LobbyId = lobbyId;

            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("Connected");
            await base.OnConnectedAsync();
        }




        private string GenerateLobbyId()
        {
            char[] id = new char[4];
            Random ran = new Random();
            for (int i = 0; i < 4; i++)
            {
                id[i] = (char)ran.Next(65, 90); // generate capital letter
            }
            return new String(id);
        }

    }
}