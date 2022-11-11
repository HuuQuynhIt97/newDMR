using System.Collections.Generic;
using System.Linq;

public class ConnectionMapping<T>
{
    private readonly Dictionary<T, HashSet<string>> _connections =
        new Dictionary<T, HashSet<string>>();

    public int Count
    {
        get
        {
            return _connections.Count;
        }
    }

    public void Add(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string> connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                connections = new HashSet<string>();
                _connections.Add(key, connections);
            }

            lock (connections)
            {
                connections.Add(connectionId);
            }
        }
    }

    public IEnumerable<string> GetConnections(T key)
    {
        HashSet<string> connections;
        if (_connections.TryGetValue(key, out connections))
        {
            return connections;
        }

        return Enumerable.Empty<string>();
    }
    public IEnumerable<T> GetKey()
    {
        var entries = _connections.Select(d => d.Key);
        return entries;
    }

    public HashSet<string> FindConnection(T key)
    {
        HashSet<string> connections;
        if (_connections.TryGetValue(key, out connections))
        {
            return connections;
        }

        return null;
    }
    public string FindKeyByValue(string value)
    {
        var result = _connections.FirstOrDefault(x => x.Value.Any(a => a == value));
        if (result.Key != null)
        {
            return result.Key.ToString();
        }

        return null;
    }
    public T FindKeyByValue2(string value)
    {
        var result = _connections.FirstOrDefault(x => x.Value.Any(a => a == value));
        return (T)result.Key;
    }
    public void Remove(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string> connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                return;
            }

            lock (connections)
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }
    /// <summary>
    /// Only remove a value of current key
    /// </summary>
    /// <param name="key">This is a userID</param>
    /// <param name="connectionId">This is clientId of signalr client</param>
    public void RemoveKeyAndValue(T key, string connectionId)
    {
        lock (_connections)
        {
            HashSet<string> connections;
            if (!_connections.TryGetValue(key, out connections))
            {
                return;
            }

            lock (connections)
            {
                connections.RemoveWhere(x => x == connectionId);

                if (connections.Count == 0)
                {
                    _connections.Remove(key);
                }
            }
        }
    }
    public string ToJson()
    {
        var entries = _connections.Select(d =>
            string.Format("\"{0}\": [\"{1}\"]", d.Key, string.Join(",", d.Value)));
        return "{" + string.Join(",", entries) + "}";
    }
}