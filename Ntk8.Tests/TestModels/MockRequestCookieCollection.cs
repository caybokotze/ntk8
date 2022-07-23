using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Ntk8.Tests.TestModels;

public class MockRequestCookieCollection : IRequestCookieCollection
{
    private readonly Dictionary<string, string?> _values;

    public MockRequestCookieCollection(Dictionary<string, string?> values)
    {
        _values = values;
        Keys = _values.Keys;
        Count = _values.Count;
    }
        
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public bool TryGetValue(string key, out string? value)
    {
        var canGet = _values.TryGetValue(key, out var v);
        value = v;
        return canGet;
    }

    public int Count { get; }
        
    public ICollection<string> Keys { get; }

    public string? this[string key] => _values[key];
}