using Common.Repository;
using UserService.Infrastracture.Persistence;

namespace UserService.Infrastracture.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UserServiceContext _context;

        public UnitOfWork(UserServiceContext context)
        {
            _context = context;
        }
        public async Task Save()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
