namespace CurrencyService.Application.Services
{
    public interface ICurrentUser
    {
        Guid? GetUserId();
    }
}
