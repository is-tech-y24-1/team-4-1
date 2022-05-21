﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sonar.UserProfile.Data.Users;

namespace Sonar.UserProfile.Data
{
    public class SonarContext : DbContext
    {
        public DbSet<UserDbModel> Users { get; set; }

        public string ConnectionString { get; }

        public SonarContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            ConnectionString =
                Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    configuration["SQLiteConnectionString"]);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={ConnectionString}");
            optionsBuilder.UseSnakeCaseNamingConvention();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.LogTo(Console.WriteLine);
            base.OnConfiguring(optionsBuilder);
        }
    }
}