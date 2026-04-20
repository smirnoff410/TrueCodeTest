using Microsoft.EntityFrameworkCore;
using UserService.Application.Repository;
using UserService.Domain.Models;
using UserService.Infrastracture.Persistence;

namespace UserService.Infrastracture.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserServiceContext _context;

        public UserRepository(UserServiceContext context)
        {
            _context = context;
        }
        public async Task<Guid> Add(User user)
        {
            user.Id = Guid.NewGuid();
            await _context.Users.AddAsync(user).ConfigureAwait(false);
            return user.Id;
        }

        public async Task<User?> GetByNameAndPassword(string name, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Name == name && x.Password == password).ConfigureAwait(false);
            return user;
        }

        public async Task<bool> IsExist(string name)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Name == name).ConfigureAwait(false);
            return user != null;
        }
    }
}
