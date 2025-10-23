namespace AgenticAISample;

public sealed class AgentConfig
{
    public AgentSection Agent { get; init; } = new();
    public OllamaSection Ollama { get; init; } = new();
    public ProcessLlmSection ProcessLlm { get; init; } = new();
    public SqlSection Sql { get; init; } = new();
    public MemorySection Memory { get; init; } = new();
    public ReportsSection Reports { get; init; } = new();

    public sealed class AgentSection
{
    public string? Provider { get; init; } = "lmstudio";
    public string? Model { get; init; } = "openai/gpt-oss-20b";
    public string SystemPrompt { get; init; } = "You are a helpful, concise local assistant.";
    public int MaxTokens { get; init; } = 512;
    public double Temperature { get; init; } = 0.3;
}

    public sealed class OllamaSection
    {
        public string? BaseUrl { get; init; }
    }

    public sealed class ProcessLlmSection
    {
        public string? ExecutablePath { get; init; }
        public string? ArgsTemplate { get; init; }
        public string? ModelPath { get; init; }
    }

    public sealed class SqlSection
    {
        public string ConnectionString { get; init; } = "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    public sealed class MemorySection
    {
        public string StorePath { get; init; } = Path.Combine("memory", "agent_memory.json");
        public int MaxMessages { get; init; } = 50;
    }

    public sealed class ReportsSection
    {
        public string OutputDir { get; init; } = "reports";
    }
}

