namespace CurrencyService.Application.Services
{
    using CurrencyService.Domain.Models;
    public interface ICurrencyGrabber
    {
        Task<List<Currency>> Grab();
    }
}
