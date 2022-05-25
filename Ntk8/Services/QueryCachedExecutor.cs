using System;
using Dapper.CQRS;
using Microsoft.Extensions.Caching.Memory;

namespace Ntk8.Services
{
    public interface IQueryCachedExecutor
    {
        T GetAndSet<T>(Query<T> query, string key, TimeSpan? timeSpan = null);
    }

    public class QueryCachedExecutor : IQueryCachedExecutor
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IQueryExecutor _queryExecutor;

        public QueryCachedExecutor(
            IMemoryCache memoryCache,
            IQueryExecutor queryExecutor)
        {
            _memoryCache = memoryCache;
            _queryExecutor = queryExecutor;
        }

        public T GetAndSet<T>(Query<T> query, string key, TimeSpan? timeSpan = null)
        {
            _memoryCache
                .TryGetValue(key, out var payload);
            
            if (payload is not null)
            {
                return (T) payload;
            }
            
            var result = _queryExecutor.Execute(query);

            if (result is null)
            {
                return result;
            }
            
            if (timeSpan is not null)
            {
                _memoryCache
                    .Set(key, result, timeSpan.Value);
            }

            if (timeSpan is null)
            {
                _memoryCache
                    .Set(key, result, TimeSpan.FromMinutes(1));
            }
            
            return result;
        }
    }
}