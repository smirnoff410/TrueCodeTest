using CurrencyService.Application.Services;
using System.Xml.Serialization;

namespace CurrencyService.Infrastracture.Services.Currency
{
    using CurrencyService.Domain.Models;
    using Mapster;
    using System;
    using System.Text;

    public class CurrencyGrabber : ICurrencyGrabber
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyGrabber> _logger;
        private const string BaseUrl = "http://www.cbr.ru/scripts/XML_daily.asp";

        public CurrencyGrabber(
            HttpClient httpClient,
            ILogger<CurrencyGrabber> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<List<Currency>> Grab()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                response.EnsureSuccessStatusCode();

                var bytes = await response.Content.ReadAsByteArrayAsync();

                Encoding encoding = Encoding.GetEncoding("windows-1251");
                string responseString = encoding.GetString(bytes, 0, bytes.Length);

                var currencies = await ParseXmlAsync(responseString);
                return currencies.Currencies.Adapt<List<Currency>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while grabbing currency rates");
                throw;
            }
        }
        private async Task<CurrencyResponse> ParseXmlAsync(string xmlContent)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(CurrencyResponse));

                using var reader = new StringReader(xmlContent);
                var response = (CurrencyResponse?)serializer.Deserialize(reader);

                if (response == null)
                {
                    throw new InvalidOperationException("Failed to deserialize XML response");
                }

                _logger.LogDebug("Deserialized {Count} currencies for date {Date}",
                    response.Currencies.Count, response.Date);

                return response;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "XML deserialization failed. XML content: {Xml}", xmlContent);
                throw new Exception("Failed to parse currency XML", ex);
            }
        }
    }
}
