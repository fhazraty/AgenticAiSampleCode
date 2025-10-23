using System.Text.Json;
using AgenticAISample;
using AgenticAISample.Data;
using AgenticAISample.LLM;
using AgenticAISample.Memory;
using AgenticAISample.Reporting;

var builder = Host.CreateApplicationBuilder(args);

// Load configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

// Bind config
var agentConfig = new AgentConfig();
builder.Configuration.Bind(agentConfig);

// DI registrations
builder.Services.AddSingleton(agentConfig);
builder.Services.AddSingleton<IMemoryStore, FileMemoryStore>();
builder.Services.AddSingleton<SqlService>();
builder.Services.AddSingleton<ReportGenerator>();

// LLM provider selection (supports LM Studio / Ollama / Process)
builder.Services.AddSingleton<ILlmClient>(sp =>
{
    var cfg = sp.GetRequiredService<AgentConfig>();

    // fallbacks to safe defaults
    var provider = cfg.Agent?.Provider?.ToLowerInvariant() ?? "lmstudio";
    var model = cfg.Agent?.Model ?? "openai/gpt-oss-20b";
    var baseUrl = provider switch
    {
        "lmstudio" => cfg.LmStudio?.BaseUrl ?? cfg.Ollama?.BaseUrl ?? "http://localhost:1234",
        "ollamahttp" => cfg.Ollama?.BaseUrl ?? "http://localhost:11434",
        _ => cfg.Ollama?.BaseUrl ?? "http://localhost:1234"
    };

    return provider switch
    {
        // LM Studio exposes an OpenAI-compatible API on /v1/chat/completions
        "lmstudio" => new AgenticAISample.LLM.OpenAiCompatibleLlmClient(baseUrl, model, cfg.LmStudio?.ApiKey),
        "ollamahttp" => new OllamaLlmClient("http://localhost:11434", model),
        "process" => new ProcessLlmClient(
            cfg.ProcessLlm.ExecutablePath!,
            cfg.ProcessLlm.ArgsTemplate!,
            cfg.ProcessLlm.ModelPath!),
        _ => new OllamaLlmClient(baseUrl, model),
    };
});

builder.Services.AddSingleton<Agent>();

var app = builder.Build();

// Ensure folders exist
var conf = app.Services.GetRequiredService<AgentConfig>();
Directory.CreateDirectory(Path.GetDirectoryName(conf.Memory.StorePath) ?? ".");
Directory.CreateDirectory(conf.Reports.OutputDir);

var agent = app.Services.GetRequiredService<Agent>();

// Demo workflow: ask model, query SQL, summarize, save report
Console.WriteLine("Local Agent ready with GPT-OSS-20B via LM Studio.\n");

// 1) Simple prompt using memory
var response = await agent.ChatAsync("سلام! امروز می‌خوام یک گزارش خلاصه از فروش روزانه بگیرم. کمکم می‌کنی؟");
Console.WriteLine($"LLM: {response}\n");

// 2) Sample SQL query + summarize
var sql = "SELECT TOP 5 name, create_date FROM sys.databases ORDER BY create_date DESC";
var reportPath = await agent.GenerateSummaryReportFromQueryAsync(
    sql,
    reportTitle: "نمونه گزارش سیستم پایگاه داده",
    outputFileName: $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md");

Console.WriteLine($"Report written to: {reportPath}");
Console.WriteLine("Done.");
