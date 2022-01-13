using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Photofy.Dtos;
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

        public string GetId()
        {
            return Context.ConnectionId;
        }
        public async Task<string> NewUser(string name)
        {
            var userAlreadyExists = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (userAlreadyExists != null) throw new HubException("Your user is bugged. Please restart.");
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
            var usersToJoin = context.Users.AsNoTracking().Where(u => u.LobbyId == lobbyId && u.ConnectionId != user.ConnectionId);
            if (!usersToJoin.Any()) throw new HubException("No such lobby found");
            foreach (User u in usersToJoin)
            {
                if (user.Name == u.Name) throw new HubException("Duplicate user name. Please restart with different name.");
            }
            string oldId = user.LobbyId;
            user.LobbyId = lobbyId;
            var participants = await usersToJoin.ToListAsync();
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");

            var usersToCall = context.Users.AsNoTracking().Where(u => u.LobbyId == oldId);
            if (usersToCall.Any())
                await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).MemberDisconnected(user.ConnectionId);

            await Clients.Clients(usersToJoin.Select(u => u.ConnectionId)).JoinedNewMember(user.Name, user.IsReady, user.ConnectionId);
            participants.Add(user);
            return participants;
        }
        public async Task ToggleReady()
        {
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user == null) throw new HubException("Your user is not found. Please restart.");
            user.IsReady = !user.IsReady;
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");

            var usersToJoin = await context.Users.Where(u => u.LobbyId == user.LobbyId && u.ConnectionId != user.ConnectionId).ToListAsync();

            await Clients.Clients(usersToJoin.Select(u => u.ConnectionId)).MemberToggleReady(user.ConnectionId);

            usersToJoin.Add(user);
            if (usersToJoin.All(u => u.IsReady))
            {
                usersToJoin.ForEach(u => u.IsReady = false);
                result = await context.SaveChangesAsync();
                if (result == 0) throw new HubException("Error saving database");

                await Clients.Clients(usersToJoin.Select(u => u.ConnectionId)).StartImageActivity();
            }
        }
        public async Task EstablishChoices(string choices)
        {
            var user = context.Users.FirstOrDefault(u => u.ConnectionId == Context.ConnectionId);
            if (user == null) throw new HubException("Your user is not found. Please restart.");
            user.Choices = choices;
            var result = await context.SaveChangesAsync();
            if (result == 0) throw new HubException("Error saving database");
            await startResultIfSubmitted(user.LobbyId);

        }
        private async Task startResultIfSubmitted(string userLobbyId)
        {
            var usersToJoin = await context.Users.Where(u => u.LobbyId == userLobbyId).ToListAsync();

            if (usersToJoin.All(u => u.Choices != null))
            {
                await Clients.Clients(usersToJoin.Select(u => u.ConnectionId))
                .StartResultActivity(
                    usersToJoin.Select(u => new GuessDto { Name = u.Name, Guess = u.Choices }).ToList());

                foreach (User u in usersToJoin)
                {
                    u.Choices = null;
                    u.Image = null;
                    u.IsReady = false;
                    u.SentImage = false;
                }

                await context.SaveChangesAsync();
            }
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
                context.Users.Remove(usr);
                Console.WriteLine($"Removed ID {Context.ConnectionId} from database");
                await context.SaveChangesAsync();

                var usersToCall = context.Users.Where(u => u.LobbyId == usr.LobbyId);
                if (usersToCall.Any())
                {
                    await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).MemberDisconnected(usr.ConnectionId);
                    await startResultIfSubmitted(usr.LobbyId);

                    if (usersToCall.All(u => u.SentImage))
                    {
                        await usersToCall.ForEachAsync(u => u.SentImage = false);
                        await context.SaveChangesAsync();

                        await Clients.Clients(usersToCall.Select(u => u.ConnectionId)).StartGame();
                    }

                }
            }
            else Console.WriteLine($"Failed to remove ID {Context.ConnectionId}");

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