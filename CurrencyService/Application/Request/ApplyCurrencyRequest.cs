namespace CurrencyService.Application.Request
{
    public class ApplyCurrencyRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
    }
}
