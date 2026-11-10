using System.Globalization;

namespace TrocaMoedas.Presentation;

public static class ConsoleInput
{
    public static bool IsEof(string? input) => input is null;

    public static string? ReadLine() => Console.ReadLine();

    /// <summary>
    /// Aceita ponto ou vírgula como separador decimal e rejeita valores
    /// menores ou iguais a zero.
    /// </summary>
    public static bool TryParseAmount(string? input, out double amount)
    {
        amount = 0;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var value = input.Trim();

        if (value.Contains(',') && value.Contains('.'))
        {
            // O último separador é o decimal; o outro é separador de milhar.
            value = value.LastIndexOf(',') > value.LastIndexOf('.')
                ? value.Replace(".", "").Replace(',', '.')
                : value.Replace(",", "");
        }
        else if (value.Contains(','))
        {
            value = value.Replace(',', '.');
        }

        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            return false;
        }

        amount = parsed;
        return true;
    }
}
