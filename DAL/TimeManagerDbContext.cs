using System;
using System.IO;
using DAL.models;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public sealed class TimeManagerDbContext : DbContext
    {
        private string connectionString => @"Data Source=" +
                                          Path.Combine(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                              @"../../../../DAL/")) + @"manager.db";
        public DbSet<User> Users { get; set; }
        
        public TimeManagerDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(connectionString);
        }
    }
}