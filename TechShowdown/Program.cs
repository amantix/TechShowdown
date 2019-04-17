using System;
using System.Linq;
using TechShowdown.DataAccess;
using TechShowdown.Models;

namespace TechShowdown
{
    static class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args.FirstOrDefault();

            using (var db = new AppDb(connectionString))
            {
                var users = db.Users
                    .Where(u => u.Posts.Any())
                    .Where(User.HasLongNameExpression)
                    .Select(user => new {user.Name, user.Posts.FirstOrDefault().Content})
                    .ToList();
                Console.WriteLine(string.Join('\n', users));
            }
        }
    }
}