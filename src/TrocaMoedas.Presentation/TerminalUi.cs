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

    /// <summary>
    /// Limpa a tela e reaplica a configuração de largura, mantendo o mesmo
    /// comportamento em todas as telas.
    /// </summary>
    public static void ConfigureScreen()
    {
        AnsiConsole.Clear();
        Configure();
        ShowWarningIfNeeded();
    }

    /// <summary>
    /// Cabeçalho padrão. <paramref name="context"/> é exibido após o bullet,
    /// por exemplo "Ambiente: Docker / SQLite" ou "Início › Converter".
    /// </summary>
    public static void ShowHeader(string context)
    {
        AnsiConsole.MarkupLine($"[cyan bold]TrocaMoedas Pro[/] [dim]v2.0[/]  •  [grey]{context}[/]");
        AnsiConsole.MarkupLine("[grey]─────────────────────────────────────────────────────────────[/]");
        AnsiConsole.WriteLine();
    }

    public static void ShowStep(int current, int total, string label)
    {
        AnsiConsole.MarkupLine($"[grey]Etapa [white]{current}[/] de [white]{total}[/]  •  {label}[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Aguarda Enter. Retorna <c>false</c> quando a entrada foi encerrada (EOF).
    /// </summary>
    public static bool WaitForEnter(string message = "Pressione Enter para voltar ao menu...")
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]{message}[/]");
        return !ConsoleInput.IsEof(ConsoleInput.ReadLine());
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
