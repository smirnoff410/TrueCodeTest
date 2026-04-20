using CurrencyService.Application.Repository;
using CurrencyService.Domain.Models;
using CurrencyService.Infrastracture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CurrencyService.Infrastracture.Repository
{
    public class UserCurrencyRepository : IUserCurrencyRepository
    {
        private readonly CurrencyServiceContext _context;

        public UserCurrencyRepository(CurrencyServiceContext context)
        {
            _context = context;
        }

        public async Task<Guid> Add(UserCurrency userCurrency)
        {
            userCurrency.Id = Guid.NewGuid();
            await _context.UserCurrencies.AddAsync(userCurrency).ConfigureAwait(false);
            return userCurrency.Id;
        }

        public async Task<List<Currency>> GetByUserId(Guid userId)
        {
            var currencies = await _context.UserCurrencies
                .AsNoTracking()
                .Include(x => x.Currency)
                .Where(x => x.UserId == userId)
                .Select(x => x.Currency)
                .ToListAsync()
                .ConfigureAwait(false);

            return currencies;
        }
    }
}
