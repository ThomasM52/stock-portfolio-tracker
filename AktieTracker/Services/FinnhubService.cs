using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;

public class FinnhubService
{
    private readonly string _apiKey;
    private static readonly HttpClient _httpClient = new();

    public FinnhubService()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _apiKey = config["Finnhub:ApiKey"];
    }

    public async Task<decimal?> GetCurrentPriceAsync(string ticker)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            return null;

        var url = $"https://finnhub.io/api/v1/quote?symbol={ticker}&token={_apiKey}";
        var response = await _httpClient.GetStringAsync(url);

        using var json = JsonDocument.Parse(response);
        return json.RootElement.GetProperty("c").GetDecimal();
    }

    public async Task<decimal?> GetFiveDayChangePercentAsync(string ticker)
    {
        try
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var fiveDaysAgo = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds();

            var url =
                $"https://finnhub.io/api/v1/stock/candle?symbol={ticker}" +
                $"&resolution=D&from={fiveDaysAgo}&to={now}&token={_apiKey}";

            var httpResponse = await _httpClient.GetAsync(url);

            if (!httpResponse.IsSuccessStatusCode)
                return null;
            var response = await httpResponse.Content.ReadAsStringAsync();

            using var json = JsonDocument.Parse(response);

            if (json.RootElement.GetProperty("s").GetString() != "ok")
                return null;

            var closes = json.RootElement.GetProperty("c");

            if (closes.GetArrayLength() < 2)
                return null;

            var firstClose = closes[0].GetDecimal();
            var lastClose = closes[closes.GetArrayLength() - 1].GetDecimal();

            return ((lastClose - firstClose) / firstClose) * 100m;
        }
        catch
        {
            return null;
        }
    }
}
