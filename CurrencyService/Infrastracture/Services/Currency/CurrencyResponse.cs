using System.Xml.Serialization;

namespace CurrencyService.Infrastracture.Services.Currency
{
    [XmlRoot("ValCurs")]
    public class CurrencyResponse
    {
        [XmlAttribute("Date")]
        public string Date { get; set; } = string.Empty;

        [XmlElement("Valute")]
        public List<CurrencyXml> Currencies { get; set; } = new();
    }

    public class CurrencyXml
    {
        [XmlAttribute("ID")]
        public string Id { get; set; } = string.Empty;
        [XmlElement("NumCode")]
        public string NumCode { get; set; } = string.Empty;

        [XmlElement("CharCode")]
        public string CharCode { get; set; } = string.Empty;

        [XmlElement("Nominal")]
        public int Nominal { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; } = string.Empty;

        [XmlElement("Value")]
        public string Value { get; set; } = string.Empty;

        // Преобразование строки с запятой в decimal
        public decimal GetDecimalValue()
        {
            if (decimal.TryParse(Value, out var result))
                return result;

            // Замена запятой на точку для парсинга
            if (decimal.TryParse(Value.Replace('.', ','), out result))
                return result;

            return 0;
        }

        // Расчет курса с учетом номинала
        public decimal GetRate()
        {
            return GetDecimalValue() / Nominal;
        }
    }
}
