using CurrencyService.Application.Repository;
using CurrencyService.Domain.Models;
using CurrencyService.Infrastracture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CurrencyService.Infrastracture.Repository
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly CurrencyServiceContext _context;

        public CurrencyRepository(CurrencyServiceContext context)
        {
            _context = context;
        }

        public async Task<List<Currency>> Get(IEnumerable<string> ids)
        {
            var currencies = await _context.Currencies
                .Where(x => ids.Contains(x.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            return currencies;
        }

        public async Task Add(Currency currency)
        {
            await _context.AddAsync(currency);
        }
    }
}
