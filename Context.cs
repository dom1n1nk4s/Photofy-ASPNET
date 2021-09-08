using Microsoft.EntityFrameworkCore;
using Photofy_ASPNET_1.Models;

namespace Photofy
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
        public DbSet<User> Users {get;set;}
        public DbSet<Lobby> Lobbies {get;set;}
    }
}
