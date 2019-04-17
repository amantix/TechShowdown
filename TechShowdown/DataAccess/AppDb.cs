using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TechShowdown.Models;

namespace TechShowdown.DataAccess
{
    public sealed class AppDb : DbContext
    {
        /*
         * Пример строки подключения к локальной базе mssqllocaldb
         * Server=(localdb)\mssqllocaldb;Database=AppDb;AttachDbFilename=E:\AppDb.mdf;Trusted_Connection=True;ConnectRetryCount=0
         */
        private readonly string connectionString;

        //Публичный конструктор без параметров нужен для корректной работы Entity Framework
        public AppDb(){}

        public AppDb(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString)
                .UseLoggerFactory(new LoggerFactory(new[] {new ConsoleLoggerProvider((_, __) => true, true)}))
                .ConfigureWarnings(w => w.Throw(RelationalEventId.QueryClientEvaluationWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.UserId);
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedNever();
            });

            var johnId = new Guid("b32b790d-f9cb-49eb-9e7b-4e6132592084");
            var sarahId = new Guid("62ad51cc-7475-48bb-b7a8-afe5631cd26f");
            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = johnId,
                        Name = "John Snow"
                    },
                    new User
                    {
                        Id = sarahId,
                        Name = "Sarah Connor"
                    }
                );
            modelBuilder.Entity<Post>()
                .HasData(
                    new Post
                    {
                        Id = new Guid("880200b0-21fc-4a48-8588-12f0556df6d4"),
                        UserId = johnId,
                        Content = "Winter is coming"
                    },
                    new Post
                    {
                        Id = new Guid("62b935d6-6d3e-4aeb-97c9-0242645b751b"),
                        UserId = sarahId,
                        Content = "Hasta la vista, baby"
                    }
                );
        }
    }
}