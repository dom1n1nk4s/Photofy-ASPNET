using System;
using System.Threading.Tasks;
using System.Linq;
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
        public async Task<Guid> NewUser (string name){
            var user = new User{
                Name = name,
                LobbyId = GenerateLobbyId(),
            };
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();
            if(result == 0) throw new HubException("Error saving database");
            return user.Id;
        }


        public async Task JoinLobby (Guid userId, string lobbyId){
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if(user == null) throw new HubException("No such user found");
            var userToJoin = context.Users.FirstOrDefault(u => u.LobbyId == lobbyId);
            if(userToJoin == null) throw new HubException("No such lobby found");
            
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
            for(int i = 0; i < 4; i++){
                if(ran.Next(0,1)== 0)
                    id[i] = (char)ran.Next(48,57); // generate number
                else 
                    id[i] = (char)ran.Next(65,90); // generate capital letter
            }
            return new String(id);
        }
        
    }
}