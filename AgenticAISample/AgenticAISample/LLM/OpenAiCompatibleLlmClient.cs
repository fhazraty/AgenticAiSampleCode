using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AgenticAISample.LLM;

public sealed class OpenAiCompatibleLlmClient : ILlmClient
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string? _apiKey;

    public OpenAiCompatibleLlmClient(string baseUrl, string model, string? apiKey = null)
    {
        _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        _model = model;
        _apiKey = apiKey;

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public async Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, LlmOptions? options = null, CancellationToken ct = default)
    {
        var req = new ChatCompletionsRequest
        {
            Model = _model,
            Messages = messages.Select(m => new ChatMessage { Role = m.role, Content = m.content }).ToList(),
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens
        };

        using var resp = await _http.PostAsJsonAsync("v1/chat/completions", req, cancellationToken: ct);
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<ChatCompletionsResponse>(cancellationToken: ct);
        return data?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
    }

    private sealed class ChatCompletionsRequest
    {
        [JsonPropertyName("model")] public string Model { get; init; } = string.Empty;
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; init; } = new();
        [JsonPropertyName("temperature")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public double? Temperature { get; init; }
        [JsonPropertyName("max_tokens")] [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? MaxTokens { get; init; }
    }

    private sealed class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; init; } = "user";
        [JsonPropertyName("content")] public string Content { get; init; } = string.Empty;
    }

    private sealed class ChatChoice
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; init; }
    }

    private sealed class ChatCompletionsResponse
    {
        [JsonPropertyName("choices")] public List<ChatChoice>? Choices { get; init; }
    }
}

