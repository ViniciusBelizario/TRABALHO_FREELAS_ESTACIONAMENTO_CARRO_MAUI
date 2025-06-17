using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace JoaoCar2.Converters
{
    public class PagoToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool pago && pago)
                return "pago.png";      // Ícone de pago
            else
                return "naopago.png";   // Ícone de não pago
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
