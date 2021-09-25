using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Photofy.Models;

namespace Photofy.Hubs
{
    public class GameHub : Hub<IClient>
    {
        private readonly Context context;
        public GameHub(Context context)
        {
            this.context = context;
        }
        public List<User> Test()
        {
            return new List<User> { new User { Name = "a", ConnectionId = "1", IsReady = false }, new User { Name = "b", ConnectionId = "2", IsReady = true } };
        }
        public async Task<string> NewUser(string name)
        {
            User usr;
            string lobbyId;
            do
            {
                lobbyId = GenerateLobbyId();
                usr = context.Users.AsNoTracking().FirstOrDefault(u => u.LobbyId == lobbyId);
            } while (usr != null);

            var user = new User
            {
                Name = name,
                ConnectionId = Context.ConnectionId,
                LobbyId = lobbyId,
            };
            await context.Users.AddAsync(user);
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");
            return user.LobbyId;
        }


        public async Task<List<User>> JoinLobby(string lobbyId)
        {
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user == null) throw new HubException("Your user is not found. Please restart.");
            var usersToJoin = context.Users.AsNoTracking().Where(u => u.LobbyId == lobbyId);
            if (!usersToJoin.Any()) throw new HubException("No such lobby found");
            user.LobbyId = lobbyId;

            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");

            var usersToCall = context.Users.AsNoTracking().Where(u => u.LobbyId == user.LobbyId && u.ConnectionId != user.ConnectionId);
            if (usersToCall.Any())
                await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).MemberDisconnected(user.ConnectionId);

            return usersToJoin.ToList();
        }
        public async Task ToggleReady()
        {
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user == null) throw new HubException("Your user is not found. Please restart.");
            user.IsReady = !user.IsReady;
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");


            var usersToJoin = context.Users.AsNoTracking().Where(u => u.LobbyId == user.LobbyId && u.ConnectionId != user.ConnectionId);

            await Clients.Clients(usersToJoin.Select(u => u.ConnectionId)).MemberToggleReady(user.ConnectionId);

            // check if all ready
            usersToJoin.Append(user);
            foreach (User u in usersToJoin)
            {
                if (!u.IsReady) return;
            }
            await Clients.Clients(usersToJoin.Select(u => u.ConnectionId)).StartImageActivity();
        }

        public async Task SendImage(string image)
        {
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user == null) throw new HubException("Your user is not found. Please restart.");
            user.Image = image;
            user.SentImage = true;
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");

            var usersToCall = context.Users.AsNoTracking().Where(u => u.LobbyId == user.LobbyId);

            foreach (User u in usersToCall)
            {
                if (!u.SentImage) return;
            }
            await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).StartGame(usersToCall.Select(u => u.Image).ToList());

        }
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Connected {Context.ConnectionId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var usr = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (usr != null)
            {
                var usersToCall = context.Users.AsNoTracking().Where(u => u.LobbyId == usr.LobbyId && u.ConnectionId != usr.ConnectionId);
                if (usersToCall.Any())
                    await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).MemberDisconnected(usr.ConnectionId);

                context.Users.Remove(usr);
                Console.WriteLine($"Removed ID {Context.ConnectionId} from database");
                await context.SaveChangesAsync();
            }
            else Console.Write($"Failed to remove ID {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
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