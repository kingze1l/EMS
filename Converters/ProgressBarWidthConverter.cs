using System;
using System.Globalization;
using System.Windows.Data;

namespace EMS.Converters
{
    public class ProgressBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 4 || values[0] == null || values[1] == null || values[2] == null || values[3] == null)
                return 0.0;

            if (!(values[0] is double actualWidth) || 
                !(values[1] is double value) || 
                !(values[2] is double minimum) || 
                !(values[3] is double maximum))
                return 0.0;

            if (maximum <= minimum)
                return 0.0;

            double percentage = (value - minimum) / (maximum - minimum);
            return actualWidth * percentage;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 