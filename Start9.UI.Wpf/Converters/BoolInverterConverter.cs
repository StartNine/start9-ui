﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Start9.UI.Wpf.Converters
{
    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (!(bool)value);
        }
    }
}
