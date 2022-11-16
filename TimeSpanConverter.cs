using System;
using System.Globalization;
using System.Windows.Data;

namespace FFmpegGui;

public class TimeSpanStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var timeSpanValue = (TimeSpan) value;
        return timeSpanValue.ToString(MainWindow.TimestampToStringFormat);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var stringValue = (string) value;

        if (TimeSpan.TryParseExact(stringValue, MainWindow.TimestampFormats, culture, out var timeSpanValue))
        {
            return timeSpanValue;
        }
        else if (int.TryParse(stringValue, out var intValue))
        {
            return TimeSpan.FromSeconds(intValue);
        }
        else
        {
            throw new FormatException("Could not convert the timestamp string to a valid TimeSpan.");
        }
    }
}