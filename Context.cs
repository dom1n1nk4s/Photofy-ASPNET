using Microsoft.EntityFrameworkCore;
using Photofy.Models;

namespace Photofy
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<User> Users { get; set; }
        // public DbSet<Lobby> Lobbies { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.ConnectionId);
        }

    }
}
