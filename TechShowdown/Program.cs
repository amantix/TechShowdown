using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TechShowdown
{
    /*
     * Добавим класс для хранения данных о пользователе
     * Пусть для начала он будет содержать идентификатор и имя
     */
    public class User
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    /*
     * Добавим также контекст базы данных, содержащей таблицу с нашими пользователями
     * Класс наследуется от DbContext
     */
    public sealed class AppDb : DbContext
    {
        /*
         * Пример строки подключения к локальной базе mssqllocaldb
         * Server=(localdb)\mssqllocaldb;Database=AppDb;AttachDbFilename=E:\AppDb.mdf;Trusted_Connection=True;ConnectRetryCount=0
         */
        private readonly string connectionString;

        public AppDb(string connectionString)
        {
            this.connectionString = connectionString;
            Database.EnsureCreated(); //.Migrate();
        }

        //Коллекция DbSet<User> будет использоваться для работы с пользователями
        public DbSet<User> Users { get; set; }

        /*
         * В перегруженном методе OnConfiguring настраиваем подключение к базе данных
         * Пример строки подключения:
         * Server=(localdb)\mssqllocaldb;Database=AppDb;AttachDbFilename=E:\AppDb.mdf;Trusted_Connection=True;ConnectRetryCount=0
         */
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(connectionString);
            
            //.UseInMemoryDatabase("AppDb"); //Можно пользоваться бд в памяти, но для нашего примера не подойдёт
        }

        //Настраиваем модель данных
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Настраиваем маппинг пользователя в бд
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Name).IsRequired();
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            //Добавляем начальные данные (seed) в банку
            modelBuilder.Entity<User>()
                .HasData(
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "John Snow"
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        Name = "Sarah Connor"
                    }
                );
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Считываем строку подключения из аргументов командной строки
             * По взрослому - из файла конфигурации считывать
             */
            var connectionString = args.FirstOrDefault();

            //Все действия с бд выполняем в контексте
            using (var db = new AppDb(connectionString))
            {
                //Пока просто выведем имена всех пользователей
                var users = db.Users.Select(u => u.Name).ToList();
                Console.WriteLine(string.Join('\n', users));
            }
        }
    }
}