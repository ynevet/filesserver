using System.Data.Entity;
using FileServer.DataAccess.Models;

namespace FileServer.DataAccess
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
