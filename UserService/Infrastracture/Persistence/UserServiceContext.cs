using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;

namespace UserService.Infrastracture.Persistence
{
    public class UserServiceContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserServiceContext(DbContextOptions<UserServiceContext> options) : base(options)
        {
            
        }
    }
}
