using CurrencyService.Domain.Models;

namespace CurrencyService.Application.Repository
{
    public interface ICurrencyRepository
    {
        Task<List<Currency>> Get(IEnumerable<string> ids);
        Task Add(Currency currency);
    }
}
