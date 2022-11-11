using DMR_API.Data.MongoModels;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;

namespace DMR_API.Data
{
    public class IoTContext : DbContext
    {
        public IoTContext(DbContextOptions<IoTContext> options) : base(options) { }
        public DbSet<Mixing> Mixing { get; set; }
        public DbSet<RawData> RawData { get; set; }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Mixing>().HasKey(x => x.ID);// um
        }

    }
}