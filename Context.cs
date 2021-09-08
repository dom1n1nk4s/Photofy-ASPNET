using Microsoft.EntityFrameworkCore;

namespace Photofy
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }
    }
}
