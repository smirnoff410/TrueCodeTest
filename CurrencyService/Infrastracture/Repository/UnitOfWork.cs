using Common.Repository;
using CurrencyService.Infrastracture.Persistence;

namespace CurrencyService.Infrastracture.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CurrencyServiceContext _context;

        public UnitOfWork(CurrencyServiceContext context)
        {
            _context = context;
        }
        public async Task Save()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
