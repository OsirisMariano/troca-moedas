using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using TrocaMoedas.Application.Services;
using TrocaMoedas.Domain.Models;

namespace TrocaMoedas.Infrastructure.Services;

/// <summary>
/// Cache em memória das taxas por moeda base, com expiração por TTL.
/// </summary>
public sealed class InMemoryExchangeRateCache : IExchangeRateCache
{
    private readonly ConcurrentDictionary<Currency, CacheEntry> _entries = new();
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _ttl;
    private readonly int _maxEntries;

    public InMemoryExchangeRateCache(TimeSpan ttl, TimeProvider? timeProvider = null, int maxEntries = 6)
    {
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), "O TTL deve ser maior que zero.");
        }

        if (maxEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxEntries), "O limite deve ser maior que zero.");
        }

        _ttl = ttl;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _maxEntries = maxEntries;
    }

    public bool TryGet(Currency baseCurrency, [NotNullWhen(true)] out ExchangeRate? rate)
    {
        if (_entries.TryGetValue(baseCurrency, out var entry))
        {
            if (_timeProvider.GetUtcNow() - entry.StoredAt < _ttl)
            {
                rate = entry.Rate;
                return true;
            }

            _entries.TryRemove(baseCurrency, out _);
        }

        rate = null;
        return false;
    }

    public void Set(ExchangeRate rate)
    {
        PruneExpired();

        if (!_entries.ContainsKey(rate.BaseCurrency) && _entries.Count >= _maxEntries)
        {
            EvictOldest();
        }

        _entries[rate.BaseCurrency] = new CacheEntry(rate, _timeProvider.GetUtcNow());
    }

    private void PruneExpired()
    {
        var now = _timeProvider.GetUtcNow();

        foreach (var (currency, entry) in _entries)
        {
            if (now - entry.StoredAt >= _ttl)
            {
                _entries.TryRemove(currency, out _);
            }
        }
    }

    private void EvictOldest()
    {
        var oldest = _entries
            .OrderBy(entry => entry.Value.StoredAt)
            .Select(entry => (Currency?)entry.Key)
            .FirstOrDefault();

        if (oldest is not null)
        {
            _entries.TryRemove(oldest.Value, out _);
        }
    }

    private sealed record CacheEntry(ExchangeRate Rate, DateTimeOffset StoredAt);
}
