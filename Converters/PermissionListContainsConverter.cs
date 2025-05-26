using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using EMS.Models;

namespace EMS.Converters
{
    public class PermissionListContainsConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2 || values[0] == null || values[1] == null)
                return false;
            if (values[0] is List<Permission> permissions && values[1] is Permission permission)
                return permissions.Contains(permission);
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // Handled in the ViewModel's command
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
} 