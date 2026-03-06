using AktieTracker.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AktieTracker.Services
{
    public class StockStorageService
    {
        private const string FileName = "stocks.json";

        public void Save(IEnumerable<Stock> stocks)
        {
            var json = JsonSerializer.Serialize(stocks, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FileName, json);
        }

        public List<Stock> Load()
        {
            if (!File.Exists(FileName))
                return new List<Stock>();

            var json = File.ReadAllText(FileName);
            return JsonSerializer.Deserialize<List<Stock>>(json) ?? new List<Stock>();
        }
    }
}
