using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HRMS.API.Helpers;

// Turns a list of simple DTOs into a CSV file (UTF-8 with BOM so Excel shows Bangla/Unicode correctly).
public static class CsvExporter
{
    public static byte[] ToCsv<T>(IEnumerable<T> rows)
    {
        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => IsSimple(p.PropertyType))
            .ToArray();

        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", properties.Select(p => Escape(Humanize(p.Name), false))));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(",", properties.Select(p =>
                Escape(FormatValue(p.GetValue(row)), p.PropertyType == typeof(string)))));
        }

        var preamble = new UTF8Encoding(true).GetPreamble();
        var body = Encoding.UTF8.GetBytes(builder.ToString());
        return preamble.Concat(body).ToArray();
    }

    private static bool IsSimple(Type type)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime);
    }

    private static string Humanize(string name) => Regex.Replace(name, "(?<!^)([A-Z])", " $1");

    private static string FormatValue(object? value) => value switch
    {
        null => "",
        DateTime d => d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                                                   : d.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
        bool b => b ? "Yes" : "No",
        decimal m => m.ToString("0.##", CultureInfo.InvariantCulture),
        double x => x.ToString("0.##", CultureInfo.InvariantCulture),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? ""
    };

    private static string Escape(string value, bool isText)
    {
        // Spreadsheet formula injection guard: text cells starting with = + - @ are prefixed with '
        if (isText && value.Length > 0 && "=+-@\t\r".IndexOf(value[0]) >= 0)
            value = "'" + value;

        if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0)
            value = "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }
}
