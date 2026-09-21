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

        services.AddSingleton<IExchangeRateService>(sp =>
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                AnsiConsole.MarkupLine("[yellow]⚠️  API key não configurada. Usando taxas de fallback.[/]");
                AnsiConsole.MarkupLine("[dim]Configure EXCHANGE_RATE_API_KEY para usar taxas em tempo real.[/]");
                AnsiConsole.WriteLine();
                return new FallbackExchangeRateService();
            }
            else
            {
                var client = sp.GetRequiredService<HttpClient>();
                return new ExchangeRateApiService(client, apiKey);
            }
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

    private static async Task InitializeDatabase(IServiceProvider serviceProvider)
    {
        var initializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }
}
