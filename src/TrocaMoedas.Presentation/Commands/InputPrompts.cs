using Spectre.Console;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Presentation.Commands;

public sealed record HistoryCountOption(string Label, int Count);

public static class InputPrompts
{
    public static IReadOnlyList<HistoryCountOption> HistoryCountOptions { get; } = new[]
    {
        new HistoryCountOption("10 conversões", 10),
        new HistoryCountOption("25 conversões", 25),
        new HistoryCountOption("50 conversões", 50),
        new HistoryCountOption("Todas as conversões", int.MaxValue)
    };

    public static string FormatCurrency(Currency currency) =>
        $"{currency.GetSymbol()} {currency} [dim]- {currency.GetName()}[/]";

    public static IReadOnlyList<Currency> DestinationsFor(IEnumerable<Currency> currencies, Currency origin) =>
        currencies.Where(currency => currency != origin).ToList();

    /// <summary>
    /// Seleção de moeda com setas no modo interativo e tabela numerada
    /// quando a entrada é redirecionada. Retorna <c>null</c> em EOF.
    /// </summary>
    public static Currency? PromptCurrency(string title, IReadOnlyList<Currency> options)
    {
        if (options.Count == 0)
        {
            return null;
        }

        if (Console.IsInputRedirected)
        {
            return PromptCurrencyNumeric(title, options);
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<Currency>()
                .Title($"[yellow]{title}[/]")
                .PageSize(options.Count)
                .UseConverter(FormatCurrency)
                .AddChoices(options));
    }

    /// <summary>
    /// Leitura de valor com validação no próprio prompt. Retorna <c>null</c> em EOF.
    /// </summary>
    public static double? PromptAmount(string title)
    {
        if (Console.IsInputRedirected)
        {
            while (true)
            {
                AnsiConsole.MarkupLine($"[bold yellow]{title}[/]");

                var input = ConsoleInput.ReadLine();
                if (ConsoleInput.IsEof(input))
                {
                    return null;
                }

                if (ConsoleInput.TryParseAmount(input, out var amount))
                {
                    return amount;
                }

                AnsiConsole.MarkupLine("[red]Valor inválido. Informe um número maior que zero (ex.: 100 ou 100,50).[/]");
            }
        }

        var value = AnsiConsole.Prompt(
            new TextPrompt<string>($"[yellow]{title}[/]")
                .Validate(input => ConsoleInput.TryParseAmount(input, out _)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Informe um número maior que zero (ex.: 100 ou 100,50).[/]")));

        return ConsoleInput.TryParseAmount(value, out var parsed) ? parsed : null;
    }

    /// <summary>
    /// Quantidade de conversões do histórico. Retorna <c>null</c> em EOF.
    /// </summary>
    public static HistoryCountOption? PromptHistoryCount()
    {
        if (Console.IsInputRedirected)
        {
            while (true)
            {
                AnsiConsole.MarkupLine("[bold yellow]Quantas conversões deseja ver? (1=10, 2=25, 3=50, 4=Todas)[/]");

                var input = ConsoleInput.ReadLine();
                if (ConsoleInput.IsEof(input))
                {
                    return null;
                }

                if (int.TryParse(input, out var index) && index >= 1 && index <= HistoryCountOptions.Count)
                {
                    return HistoryCountOptions[index - 1];
                }

                AnsiConsole.MarkupLine($"[red]Opção inválida. Digite um número entre 1 e {HistoryCountOptions.Count}.[/]");
            }
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<HistoryCountOption>()
                .Title("[yellow]Quantas conversões deseja ver?[/]")
                .PageSize(HistoryCountOptions.Count)
                .UseConverter(option => option.Label)
                .AddChoices(HistoryCountOptions));
    }

    private static Currency? PromptCurrencyNumeric(string title, IReadOnlyList<Currency> options)
    {
        while (true)
        {
            RenderCurrencyTable(options);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold yellow]{title}[/]");

            var input = ConsoleInput.ReadLine();
            if (ConsoleInput.IsEof(input))
            {
                return null;
            }

            if (int.TryParse(input, out var index) && index >= 1 && index <= options.Count)
            {
                return options[index - 1];
            }

            AnsiConsole.MarkupLine($"[red]Opção inválida. Digite um número entre 1 e {options.Count}.[/]");
        }
    }

    private static void RenderCurrencyTable(IReadOnlyList<Currency> options)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Blue)
            .AddColumn(new TableColumn("[bold]#[/]").Centered())
            .AddColumn(new TableColumn("[bold]Moeda[/]").Centered())
            .AddColumn(new TableColumn("[bold]Nome[/]").Centered());

        for (var i = 0; i < options.Count; i++)
        {
            var currency = options[i];
            table.AddRow($"[yellow]{i + 1}[/]", $"{currency.GetSymbol()} {currency}", currency.GetName());
        }

        AnsiConsole.Write(table);
    }
}
