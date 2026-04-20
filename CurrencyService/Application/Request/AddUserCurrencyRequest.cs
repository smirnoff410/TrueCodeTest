namespace CurrencyService.Application.Request
{
    public class AddUserCurrencyRequest
    {
        public string CurrencyId { get; set; }
        public Guid UserId { get; set; }
    }
}
