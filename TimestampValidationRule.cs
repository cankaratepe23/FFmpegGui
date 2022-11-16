using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Media;

namespace FFmpegGui;

public class TimestampValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var stringValue = (string) value;
        if (!Regex.IsMatch(stringValue, "^([0-9]{1,2}:){0,2}([0-9]{1,2})(.([0-9]{1,3})){0,1}$"))
        {
            return new ValidationResult(false, "Timestamp format is wrong.");
        }

        return ValidationResult.ValidResult;
    }
}