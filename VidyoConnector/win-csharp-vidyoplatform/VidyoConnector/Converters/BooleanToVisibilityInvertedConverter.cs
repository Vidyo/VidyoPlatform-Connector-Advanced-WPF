using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

/// <summary>
/// Represents converting from Bool to Visibility enumeration, but inverted.
/// This means the control will be shown if the value of bool property is false and vice versa.
/// Microsoft only provides default converter for bool(true) to Visibility(Visible)
/// </summary>

namespace VidyoConnector.Converters
{
    public class BooleanToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;

            if (value is bool)
            {
                flag = (bool)value;
            }

            if (value is bool?)
            {
                bool? flag2 = (bool?)value;
                flag = (flag2.HasValue && flag2.Value);
            }

            return flag ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
                return (Visibility)value == Visibility.Visible;

            return false;
        }
    }
}