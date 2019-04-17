using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TechShowdown.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ICollection<Post> Posts { get; set; }
        public bool HasLongName => Name.Length > 10;
        public static readonly Expression<Func<User, bool>> HasLongNameExpression = user => user.Name.Length > 10;
    }
}