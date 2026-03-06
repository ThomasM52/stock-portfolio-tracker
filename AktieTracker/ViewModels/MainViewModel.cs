using AktieTracker.Commands;
using AktieTracker.Models;
using AktieTracker.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AktieTracker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FinnhubService _service = new FinnhubService();
        private readonly StockStorageService _storage = new StockStorageService();

        public ObservableCollection<Stock> Stocks { get; } = new();

        public string NewTicker { get; set; }
        public decimal NewPurchasePrice { get; set; }
        public int NewQuantity { get; set; }

        public RelayCommand AddStockCommand { get; }
        public RelayCommand UpdateStockCommand { get; }
        public RelayCommand UpdateAllStocksCommand { get; }
        public RelayCommand DeleteStockCommand { get; }
        public RelayCommand UpdatePurchasePriceCommand { get; }
        public RelayCommand UpdateQuantityCommand { get; }

        public MainViewModel()
        {
            AddStockCommand = new RelayCommand(_ => AddStock());
            UpdateStockCommand = new RelayCommand(async s => await UpdateStockAsync(s as Stock));
            UpdateAllStocksCommand = new RelayCommand(async _ => await UpdateAllStocksAsync());
            DeleteStockCommand = new RelayCommand(s => DeleteStock(s as Stock));
            UpdatePurchasePriceCommand = new RelayCommand(s => UpdatePurchasePrice(s as Stock));
            UpdateQuantityCommand = new RelayCommand(s => UpdateQuantity(s as Stock));

            LoadStocks();

            Stocks.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(TotalPortfolioValue));
                OnPropertyChanged(nameof(TotalPortfolioProfit));
            };
        }

        private void LoadStocks()
        {
            var loadedStocks = _storage.Load();
            foreach (var stock in loadedStocks)
            {
                Stocks.Add(stock);
            }
        }

        private void AddStock()
        {
            Stocks.Add(new Stock
            {
                Ticker = NewTicker.ToUpper(),
                PurchasePrice = NewPurchasePrice,
                Quantity = NewQuantity
            });

            _storage.Save(Stocks);

            NewTicker = string.Empty;
            NewPurchasePrice = 0;
            NewQuantity = 0;

            OnPropertyChanged(nameof(NewTicker));
            OnPropertyChanged(nameof(NewPurchasePrice));
            OnPropertyChanged(nameof(NewQuantity));
        }

        private void UpdateQuantity(Stock stock)
        {
            if (stock == null)
                return;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Nyt antal for {stock.Ticker}",
                "Opdater antal",
                stock.Quantity.ToString());

            if (int.TryParse(input, out int newQty))
            {
                stock.Quantity = newQty;
                _storage.Save(Stocks);

                OnPropertyChanged(nameof(TotalPortfolioValue));
                OnPropertyChanged(nameof(TotalPortfolioProfit));
            }
        }

        private void DeleteStock(Stock stock)
        {
            if (stock == null)
                return;

            var result = MessageBox.Show(
                $"Vil du slette {stock.Ticker}?",
                "Bekræft",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                Stocks.Remove(stock);
                _storage.Save(Stocks);               
            }
        }

        private void UpdatePurchasePrice(Stock stock)
        {
            if (stock == null)
                return;

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Ny købspris for {stock.Ticker}",
                "Opdater købspris",
                stock.PurchasePrice.ToString());

            if (decimal.TryParse(input, out decimal newPrice))
            {
                stock.PurchasePrice = newPrice;
                _storage.Save(Stocks);

                OnPropertyChanged(nameof(TotalPortfolioValue));
                OnPropertyChanged(nameof(TotalPortfolioProfit));
            }           
        }

        private async Task UpdateStockAsync(Stock stock)
        {
            var price = await _service.GetCurrentPriceAsync(stock.Ticker);           

            if (price != null)
            {
                stock.CurrentPrice = price;
                stock.LastUpdated = DateTime.Now;               
            }

            _storage.Save(Stocks);

            OnPropertyChanged(nameof(TotalPortfolioValue));
            OnPropertyChanged(nameof(TotalPortfolioProfit));
        }

        private async Task UpdateAllStocksAsync()
        {
            foreach (var stock in Stocks)
            {
                await UpdateStockAsync(stock);
            }

            _storage.Save(Stocks);           
        }

        public decimal TotalPortfolioValue => Stocks.Sum(s => s.PositionValue ?? 0);
        public decimal TotalPortfolioProfit => Stocks.Sum(s => s.PositionProfit ?? 0);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
