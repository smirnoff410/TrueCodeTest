namespace CurrencyService.Domain.Models
{
    public class UserCurrency
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
