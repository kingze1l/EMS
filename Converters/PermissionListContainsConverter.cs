using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace EMS.Converters
{
    public class PermissionListContainsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == null || values[1] == null)
                return false;

            var permissions = values[0] as List<string>;
            var permission = values[1] as string;

            return permissions != null && permission != null && permissions.Contains(permission);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 