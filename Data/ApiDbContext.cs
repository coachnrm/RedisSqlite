using Microsoft.EntityFrameworkCore;
using RedisSqlite.Models;

namespace RedisSqlite.Data 
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        { 
        }

        public DbSet<Car> Cars { get; set; }

    }
}