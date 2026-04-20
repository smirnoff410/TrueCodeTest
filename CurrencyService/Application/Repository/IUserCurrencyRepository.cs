using CurrencyService.Domain.Models;

namespace CurrencyService.Application.Repository
{
    public interface IUserCurrencyRepository
    {
        Task<List<Currency>> GetByUserId(Guid userId);
        Task<Guid> Add(UserCurrency userCurrency);
    }
}
