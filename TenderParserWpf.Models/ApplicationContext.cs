using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using TenderParserWpf.Models.Models;

namespace TenderParserWpf.Models
{
    public class ApplicationContext:DbContext
    {
        public ApplicationContext() => Database.EnsureCreated();
        public DbSet<WordByPir> WordByPirTable { get; set; }

        public DbSet<WordBySipoe> WordBySipoeTable { get; set; } 

        public DbSet<WordBySiPoTitee> WordBySiPoTiteeTable { get; set; } 

        public DbSet<WordByIiId> WordByIiIdTable { get; set; } 

        public DbSet<WordByGirIGrr> WordByGirIGrrTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var pathDB = Path.Combine(Environment.CurrentDirectory, "wpfTender.db");
            optionsBuilder.UseSqlite("Data Source=" + pathDB);
        }
    }
}
