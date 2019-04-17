using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace TechShowdown
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Post> Posts { get; set; }

        /*
         * В логах после выполнения запроса с применением этого фильтра выводится
         * The LINQ expression 'where [u].HasLongName' could not be translated and will be evaluated locally.
         * Это - отвратительно: вытянется вся таблица и пофильтруется локально!
         */
        public bool HasLongName => Name.Length > 10;

        /*
         * Вместо описания обычного свойства реализуем фильтр в виде Expression
         * Который сможет транслироваться в запрос
         */
        public static Expression<Func<User, bool>> HasLongNameExpression = user => user.Name.Length > 10;
    }

    public class Post
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public User User { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
    }

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

        //Коллекциии, в которые лениво мапятся таблицы пользователей и постов
        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString)
                .UseLoggerFactory(new LoggerFactory(new[] {new ConsoleLoggerProvider((_, __) => true, true)}))
                //Настроим выбрасывание исключения в случае скатывания в локальные вычисления
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
                        Id = Guid.NewGuid(),
                        UserId = johnId,
                        Content = "Winter is coming"
                    },
                    new Post
                    {
                        Id = Guid.NewGuid(),
                        UserId = sarahId,
                        Content = "Hasta la vista, baby"
                    }
                );
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args.FirstOrDefault();

            using (var db = new AppDb(connectionString))
            {
                //Выведем пользователей с первым постом каждого из них
                var users = db.Users
                    .Where(u => u.Posts.Any())
                    .Where(User.HasLongNameExpression) //Даже после запрета локального выполнения всё работает
                    .Select(user => new {user.Name, user.Posts.FirstOrDefault().Content})
                    .ToList();
                Console.WriteLine(string.Join('\n', users));
            }
        }
    }
}