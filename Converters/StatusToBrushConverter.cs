using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using EMS.Models;

namespace EMS.Converters
{
    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is LeaveStatus status)
            {
                return status switch
                {
                    LeaveStatus.Pending => new SolidColorBrush(Colors.Orange),
                    LeaveStatus.Approved => new SolidColorBrush(Colors.Green),
                    LeaveStatus.Rejected => new SolidColorBrush(Colors.Red),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 