using System.Text;
using AgenticAISample.Data;
using AgenticAISample.LLM;
using AgenticAISample.Memory;
using AgenticAISample.Reporting;

namespace AgenticAISample;

public sealed class Agent
{
    private readonly ILlmClient _llm;
    private readonly SqlService _sql;
    private readonly IMemoryStore _memory;
    private readonly ReportGenerator _reports;
    private readonly AgentConfig _config;

    private const string SessionId = "default";

    public Agent(ILlmClient llm, SqlService sql, IMemoryStore memory, ReportGenerator reports, AgentConfig config)
    {
        _llm = llm;
        _sql = sql;
        _memory = memory;
        _reports = reports;
        _config = config;
    }

    public async Task<string> ChatAsync(string userMessage, CancellationToken ct = default)
    {
        // Load short-term memory
        var history = await _memory.LoadAsync(SessionId, _config.Memory.MaxMessages, ct);
        var messages = new List<(string role, string content)> { ("system", _config.Agent.SystemPrompt) };
        messages.AddRange(history);
        messages.Add(("user", userMessage));

        var reply = await _llm.ChatAsync(messages, new LlmOptions
        {
            MaxTokens = _config.Agent.MaxTokens,
            Temperature = _config.Agent.Temperature
        }, ct);

        // Persist new messages
        await _memory.AppendAsync(SessionId, "user", userMessage, ct);
        await _memory.AppendAsync(SessionId, "assistant", reply, ct);
        return reply.Trim();
    }

    public async Task<string> GenerateSummaryReportFromQueryAsync(string sql, string reportTitle, string outputFileName, CancellationToken ct = default)
    {
        var table = await _sql.QueryAsync(sql, ct);
        var tablePreview = ReportGenerator.DataTableToMarkdown(table);

        var prompt = new StringBuilder()
            .AppendLine("خلاصه‌ای کوتاه و مفید از داده‌های زیر تولید کن. روی نکات مهم تاکید کن. در انتها 2-3 توصیه عملی بده.")
            .AppendLine()
            .AppendLine("داده‌ها (Markdown):")
            .AppendLine(tablePreview)
            .ToString();

        var summary = await _llm.ChatAsync(new[]
        {
            ("system", _config.Agent.SystemPrompt),
            ("user", prompt)
        }, new LlmOptions { MaxTokens = _config.Agent.MaxTokens, Temperature = _config.Agent.Temperature }, ct);

        var body = new StringBuilder()
            .AppendLine("## خلاصه")
            .AppendLine()
            .AppendLine(summary.Trim())
            .AppendLine()
            .AppendLine("## پیش‌نمایش داده‌ها")
            .AppendLine()
            .AppendLine(tablePreview)
            .ToString();

        return await _reports.WriteMarkdownAsync(reportTitle, body, outputFileName, ct);
    }
}

