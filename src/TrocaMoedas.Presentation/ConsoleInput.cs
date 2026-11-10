namespace TrocaMoedas.Presentation;

public static class ConsoleInput
{
    public static bool IsEof(string? input) => input is null;

    public static string? ReadLine() => Console.ReadLine();
}