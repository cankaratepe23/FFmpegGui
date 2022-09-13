using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FFmpegGui;

public class BooleanConverter<T> : IValueConverter
{
    protected BooleanConverter(T trueValue, T falseValue)
    {
        True = trueValue;
        False = falseValue;
    }

    public T True { get; set; }
    public T False { get; set; }

    public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is true ? True : False) ?? throw new InvalidOperationException();
    }

    public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is T castValue && EqualityComparer<T>.Default.Equals(castValue, True);
    }
}

public sealed class CustomBooleanToVisibilityConverter : BooleanConverter<Visibility>
{
    public CustomBooleanToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed)
    {
    }
}