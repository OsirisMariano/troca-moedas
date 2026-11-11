using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using TrocaMoedas.Application.Repositories;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Infrastructure.Configuration;
using TrocaMoedas.Infrastructure.Data;
using TrocaMoedas.Infrastructure.Data.Repositories;
using TrocaMoedas.Infrastructure.Services;
using TrocaMoedas.Presentation.Commands;

namespace TrocaMoedas.Presentation;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        TerminalUi.Configure();

        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        await InitializeDatabase(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var menuCommand = scope.ServiceProvider.GetRequiredService<MenuCommand>();

        await menuCommand.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var httpClient = new HttpClient();
        var apiKey = Environment.GetEnvironmentVariable("EXCHANGE_RATE_API_KEY") ?? "";
        var connectionString = "Data Source=data/conversor.db";

        services.AddSingleton(httpClient);

        var cacheTtlMinutes = GetCacheTtlMinutes();
        services.AddSingleton<IExchangeRateCache>(_ =>
            new InMemoryExchangeRateCache(TimeSpan.FromMinutes(cacheTtlMinutes)));

        services.AddSingleton<IExchangeRateService>(sp =>
        {
            var fallback = new FallbackExchangeRateService();

            if (string.IsNullOrEmpty(apiKey))
            {
                AnsiConsole.MarkupLine("[yellow]⚠️  API key não configurada. Usando taxas de fallback.[/]");
                AnsiConsole.MarkupLine("[dim]Configure EXCHANGE_RATE_API_KEY para usar taxas em tempo real.[/]");
                AnsiConsole.WriteLine();
                return fallback;
            }

            var client = sp.GetRequiredService<HttpClient>();
            var cache = sp.GetRequiredService<IExchangeRateCache>();
            var primary = new ExchangeRateApiService(client, apiKey, cache, log: LogWarning);

            return new ResilientExchangeRateService(primary, fallback, LogWarning);
        });

        services.AddSingleton<IConversionRepository>(sp =>
        {
            return new SqliteConversionRepository(connectionString);
        });

        services.AddSingleton(sp => new DatabaseInitializer(connectionString));

        services.AddSingleton<ConversionService>();

        services.AddTransient<ConvertCommand>();
        services.AddTransient<HistoryCommand>();
        services.AddTransient<MenuCommand>();
    }

    private static void LogWarning(string message) =>
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");

    private static int GetCacheTtlMinutes()
    {
        var raw = Environment.GetEnvironmentVariable("EXCHANGE_RATE_CACHE_TTL_MINUTES");
        return int.TryParse(raw, out var minutes) && minutes > 0 ? minutes : 60;
    }

    private static async Task InitializeDatabase(IServiceProvider serviceProvider)
    {
        var initializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
