using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AktieTracker.Models
{
    public class Stock : INotifyPropertyChanged
    {
        public string Ticker { get; set; }

        private decimal _purchasePrice;
        private int _quantity;
        private decimal? _currentPrice;       

        public decimal PurchasePrice
        {
            get => _purchasePrice;
            set
            {
                _purchasePrice = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(Profit));
                OnPropertyChanged(nameof(ProfitPercent));
                OnPropertyChanged(nameof(PositionProfit));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PositionValue));
                OnPropertyChanged(nameof(PositionProfit));
            }
        }

        public decimal? PositionValue => 
            CurrentPrice.HasValue ? CurrentPrice.Value * Quantity : null;
        public decimal? PositionProfit => 
            CurrentPrice.HasValue ? (CurrentPrice.Value - PurchasePrice) * Quantity : null;
        public decimal? PositionProfitPercent => 
            CurrentPrice.HasValue ? ((CurrentPrice.Value - PurchasePrice) / PurchasePrice) * 100 : null;
        
        public decimal? CurrentPrice
        {
            get => _currentPrice;
            set
            {
                _currentPrice = value;
                OnPropertyChanged();

                OnPropertyChanged(nameof(Profit));
                OnPropertyChanged(nameof(ProfitPercent));
                OnPropertyChanged(nameof(PositionValue));
                OnPropertyChanged(nameof(PositionProfit));
            }
        }

        private DateTime? _lastUpdated;
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        public decimal? Profit =>
            CurrentPrice.HasValue ? CurrentPrice.Value - PurchasePrice : null;

        public decimal? ProfitPercent =>
            CurrentPrice.HasValue ? ((CurrentPrice.Value - PurchasePrice) / PurchasePrice) * 100 : null;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}