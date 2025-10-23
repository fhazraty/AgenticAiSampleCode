namespace AgenticAISample.LLM;

public interface ILlmClient
{
    Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, LlmOptions? options = null, CancellationToken ct = default);
}

public sealed class LlmOptions
{
    public int? MaxTokens { get; init; }
    public double? Temperature { get; init; }
}

