using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AktieTracker.Converters
{
    public class ProfitColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit > 0)
                    return Brushes.Green;

                if (profit < 0)
                    return Brushes.Red;
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}