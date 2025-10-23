using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AgenticAISample.LLM;

public sealed class OllamaLlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly string _model;

    public OllamaLlmClient(string baseUrl, string model)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        _model = model;
    }

    public async Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, LlmOptions? options = null, CancellationToken ct = default)
    {
        var req = new ChatRequest
        {
            Model = _model,
            Stream = false,
            Options = new ChatRequestOptions
            {
                Temperature = options?.Temperature ?? 0.3,
                NumPredict = options?.MaxTokens ?? 512
            },
            Messages = messages.Select(m => new ChatMessage { Role = m.role, Content = m.content }).ToList()
        };

        using var resp = await _http.PostAsJsonAsync("api/chat", req, cancellationToken: ct);
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
        return data?.Message?.Content ?? string.Empty;
    }

    private sealed class ChatRequest
    {
        [JsonPropertyName("model")] public string Model { get; init; } = string.Empty;
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; init; } = new();
        [JsonPropertyName("stream")] public bool Stream { get; init; }
        [JsonPropertyName("options")] public ChatRequestOptions Options { get; init; } = new();
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; init; } = "user";
        [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
    }

    private sealed class ChatRequestOptions
    {
        [JsonPropertyName("temperature")] public double Temperature { get; init; } = 0.3;
        [JsonPropertyName("num_predict")] public int NumPredict { get; init; } = 512;
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; init; }
    }
}

