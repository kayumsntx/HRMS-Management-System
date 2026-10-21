using HRMS.Application.Common.Exceptions;

namespace HRMS.Application.Common.Helpers;

public static class EnumHelper
{
    public static bool IsValid<TEnum>(string? value) where TEnum : struct, Enum
        => !string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value.Trim(), true, out var parsed) && Enum.IsDefined(typeof(TEnum), parsed);

    public static string Normalize<TEnum>(string? value, string label) where TEnum : struct, Enum
    {
        if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<TEnum>(value.Trim(), true, out var parsed) && Enum.IsDefined(typeof(TEnum), parsed))
            return parsed.ToString();

        throw new BadRequestException($"Invalid {label}. Allowed: {string.Join(", ", Enum.GetNames<TEnum>())}.");
    }
}

public static class NumberHelper
{
    // Next sequential number, e.g. Next("RQ", ["RQ000007"], 6) => "RQ000008"
    public static string Next(string prefix, IEnumerable<string> existing, int width)
    {
        var max = existing
            .Where(n => n.StartsWith(prefix) && int.TryParse(n.Substring(prefix.Length), out _))
            .Select(n => int.Parse(n.Substring(prefix.Length)))
            .DefaultIfEmpty(0)
            .Max();

        return prefix + (max + 1).ToString().PadLeft(width, '0');
    }
}
