using Microsoft.Data.Sqlite;
using TrocaMoedas.Domain.Models;
using TrocaMoedas.Infrastructure.Data;
using TrocaMoedas.Infrastructure.Data.Repositories;
using Xunit;

namespace CurrencyConverter.Tests.Data;

public class SqliteConversionRepositoryTests : IDisposable
{
    private readonly SqliteConnection _keepAlive;
    private readonly string _connectionString;
    private readonly SqliteConversionRepository _repository;

    public SqliteConversionRepositoryTests()
    {
        var dbName = $"testdb-{Guid.NewGuid():N}";
        _connectionString = $"Data Source=file:{dbName}?mode=memory&cache=shared";

        _keepAlive = new SqliteConnection(_connectionString);
        _keepAlive.Open();

        var initializer = new DatabaseInitializer(_connectionString);
        initializer.InitializeAsync().GetAwaiter().GetResult();

        _repository = new SqliteConversionRepository(_connectionString);
    }

    public void Dispose()
    {
        _keepAlive.Dispose();
    }

    [Fact]
    public async Task SaveAsync_ThenGetRecent_ReturnsSavedConversionWithAllFields()
    {
        var conversion = new Conversion
        {
            FromCurrency = Currency.BRL,
            ToCurrency = Currency.USD,
            Amount = 100,
            Result = 20,
            Rate = 0.2,
            CreatedAt = new DateTime(2026, 9, 21, 12, 30, 0)
        };

        await _repository.SaveAsync(conversion);

        var loaded = Assert.Single(await _repository.GetRecentAsync(10));

        Assert.True(loaded.Id > 0);
        Assert.Equal(Currency.BRL, loaded.FromCurrency);
        Assert.Equal(Currency.USD, loaded.ToCurrency);
        Assert.Equal(100, loaded.Amount, precision: 6);
        Assert.Equal(20, loaded.Result, precision: 6);
        Assert.Equal(0.2, loaded.Rate, precision: 6);
        Assert.Equal(conversion.CreatedAt, loaded.CreatedAt);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsMostRecentFirst()
    {
        var older = new DateTime(2026, 1, 1, 10, 0, 0);
        var middle = older.AddDays(10);
        var newest = older.AddDays(20);

        await _repository.SaveAsync(CreateConversion(Currency.BRL, Currency.USD, older));
        await _repository.SaveAsync(CreateConversion(Currency.EUR, Currency.GBP, newest));
        await _repository.SaveAsync(CreateConversion(Currency.USD, Currency.ARS, middle));

        var loaded = await _repository.GetRecentAsync(10);

        Assert.Equal(3, loaded.Count);
        Assert.Equal(newest, loaded[0].CreatedAt);
        Assert.Equal(middle, loaded[1].CreatedAt);
        Assert.Equal(older, loaded[2].CreatedAt);
    }

    [Fact]
    public async Task GetRecentAsync_RespectsLimit()
    {
        for (var i = 1; i <= 3; i++)
        {
            await _repository.SaveAsync(CreateConversion(Currency.BRL, Currency.USD,
                new DateTime(2026, 1, 1).AddDays(i)));
        }

        var loaded = await _repository.GetRecentAsync(2);

        Assert.Equal(2, loaded.Count);
    }

    [Fact]
    public async Task GetRecentAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var loaded = await _repository.GetRecentAsync(10);

        Assert.Empty(loaded);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllConversions()
    {
        await _repository.SaveAsync(CreateConversion(Currency.BRL, Currency.USD));
        await _repository.SaveAsync(CreateConversion(Currency.USD, Currency.BRL));
        await _repository.SaveAsync(CreateConversion(Currency.EUR, Currency.JPY));

        await _repository.ClearAsync();

        Assert.Empty(await _repository.GetRecentAsync(10));
    }

    [Fact]
    public async Task SaveAsync_MultipleConversions_PersistsEachIndependently()
    {
        var first = new Conversion
        {
            FromCurrency = Currency.BRL,
            ToCurrency = Currency.USD,
            Amount = 50,
            Result = 10,
            Rate = 0.2,
            CreatedAt = new DateTime(2026, 1, 1)
        };
        var second = new Conversion
        {
            FromCurrency = Currency.USD,
            ToCurrency = Currency.BRL,
            Amount = 10,
            Result = 50,
            Rate = 5.0,
            CreatedAt = new DateTime(2026, 1, 2)
        };

        await _repository.SaveAsync(first);
        await _repository.SaveAsync(second);

        var loaded = await _repository.GetRecentAsync(10);

        Assert.Equal(2, loaded.Count);
        Assert.Equal(10, loaded[0].Amount, precision: 6);
        Assert.Equal(50, loaded[1].Amount, precision: 6);
    }

    private static Conversion CreateConversion(Currency from, Currency to, DateTime? createdAt = null)
    {
        return new Conversion
        {
            FromCurrency = from,
            ToCurrency = to,
            Amount = 100,
            Result = 100,
            Rate = 1.0,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
    }
}