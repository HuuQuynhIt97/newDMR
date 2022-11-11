using dmr_api.Models;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace DMR_API.Data
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options) { }
       
        public DbSet<DispatchList> DispatchList { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var ct = DateTime.Now;
            modelBuilder.Entity<BPFCEstablish>().HasKey(x => x.ID);
          
        }
    }
}