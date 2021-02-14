using System;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{
    public class Database : DbContext
    {
        public static bool TestServer = true;

        public Database()
        {
#if DEBUG
            TestServer = true;
#endif
        }

        public DbSet<Models.Account> Account { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql($"server=localhost;database=gameserver;user=gameserver;password=q8CsLozf;Convert Zero Datetime=true", new MySqlServerVersion(new Version(8, 0, 22)));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }
    }
}