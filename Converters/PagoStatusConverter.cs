using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace JoaoCar2.Converters
{
    public class PagoStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool pago)
                return pago ? "Pago" : "Não Pago";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
