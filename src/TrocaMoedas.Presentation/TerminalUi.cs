using Spectre.Console;

namespace TrocaMoedas.Presentation;

/// <summary>
/// Ajustes de terminal para evitar UI invisível quando o Docker Compose
/// cria o TTY do container com tamanho 0x0.
/// </summary>
public static class TerminalUi
{
    public const int DefaultWidth = 80;

    private static bool _warningShown;

    public static int ResolveWidth(int consoleWidth) =>
        consoleWidth > 0 ? consoleWidth : DefaultWidth;

    public static void Configure()
    {
        AnsiConsole.Profile.Width = ResolveWidth(GetWindowWidth());
    }

    public static void ShowWarningIfNeeded()
    {
        if (_warningShown || Console.IsInputRedirected || GetWindowWidth() > 0)
        {
            return;
        }

        _warningShown = true;

        AnsiConsole.MarkupLine("[yellow]⚠️  Terminal sem dimensões (TTY 0x0). Usando largura padrão de 80 colunas.[/]");
        AnsiConsole.MarkupLine("[dim]Se a interface não responder, rode com `docker run --rm -it` ou redimensione a janela e pressione Enter.[/]");
        AnsiConsole.WriteLine();
    }

    private static int GetWindowWidth()
    {
        try
        {
            return Console.WindowWidth;
        }
        catch (IOException)
        {
            return 0;
        }
    }
}
