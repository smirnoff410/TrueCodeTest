using CurrencyService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyService.Infrastracture.Persistence
{
    public class CurrencyServiceContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<UserCurrency> UserCurrencies { get; set; }
        public CurrencyServiceContext(DbContextOptions<CurrencyServiceContext> options) : base(options)
        {
            
        }
    }
}
