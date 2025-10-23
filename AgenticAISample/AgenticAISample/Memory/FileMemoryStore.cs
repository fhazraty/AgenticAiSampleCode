using System.Text.Json;

namespace AgenticAISample.Memory;

public interface IMemoryStore
{
    Task AppendAsync(string sessionId, string role, string content, CancellationToken ct = default);
    Task<IReadOnlyList<(string role, string content)>> LoadAsync(string sessionId, int maxMessages, CancellationToken ct = default);
}

public sealed class FileMemoryStore : IMemoryStore
{
    private readonly string _path;

    public FileMemoryStore(AgentConfig config)
    {
        _path = config.Memory.StorePath;
    }

    public async Task AppendAsync(string sessionId, string role, string content, CancellationToken ct = default)
    {
        var db = await LoadDbAsync(ct);
        if (!db.TryGetValue(sessionId, out var list))
        {
            list = new List<Message>();
            db[sessionId] = list;
        }
        list.Add(new Message { Role = role, Content = content, TimestampUtc = DateTime.UtcNow });
        await SaveDbAsync(db, ct);
    }

    public async Task<IReadOnlyList<(string role, string content)>> LoadAsync(string sessionId, int maxMessages, CancellationToken ct = default)
    {
        var db = await LoadDbAsync(ct);
        if (!db.TryGetValue(sessionId, out var list))
        {
            return Array.Empty<(string role, string content)>();
        }

        return list
            .OrderBy(m => m.TimestampUtc)
            .TakeLast(maxMessages)
            .Select(m => (m.Role, m.Content))
            .ToList();
    }

    private async Task<Dictionary<string, List<Message>>> LoadDbAsync(CancellationToken ct)
    {
        try
        {
            if (!File.Exists(_path)) return new();
            await using var fs = File.OpenRead(_path);
            var db = await JsonSerializer.DeserializeAsync<Dictionary<string, List<Message>>>(fs, cancellationToken: ct);
            return db ?? new();
        }
        catch
        {
            return new();
        }
    }

    private async Task SaveDbAsync(Dictionary<string, List<Message>> db, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
        await using var fs = File.Open(_path, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(fs, db, new JsonSerializerOptions { WriteIndented = true }, ct);
    }

    private sealed class Message
    {
        public string Role { get; init; } = "user";
        public string Content { get; init; } = string.Empty;
        public DateTime TimestampUtc { get; init; }
    }
}

