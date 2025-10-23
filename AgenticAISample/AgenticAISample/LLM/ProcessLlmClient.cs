using System.Diagnostics;
using System.Text;

namespace AgenticAISample.LLM;

public sealed class ProcessLlmClient : ILlmClient
{
    private readonly string _exePath;
    private readonly string _argsTemplate;
    private readonly string _modelPath;

    public ProcessLlmClient(string exePath, string argsTemplate, string modelPath)
    {
        _exePath = exePath;
        _argsTemplate = argsTemplate;
        _modelPath = modelPath;
    }

    public async Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, LlmOptions? options = null, CancellationToken ct = default)
    {
        var prompt = BuildPrompt(messages);
        var args = _argsTemplate
            .Replace("{MODEL_PATH}", EscapeArg(_modelPath))
            .Replace("{PROMPT}", EscapeArg(prompt))
            .Replace("{MAX_TOKENS}", (options?.MaxTokens ?? 512).ToString())
            .Replace("{TEMPERATURE}", (options?.Temperature ?? 0.3).ToString(System.Globalization.CultureInfo.InvariantCulture));

        var psi = new ProcessStartInfo
        {
            FileName = _exePath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start LLM process");
        var sb = new StringBuilder();
        await Task.WhenAll(
            Task.Run(async () => sb.Append(await proc.StandardOutput.ReadToEndAsync()), ct),
            Task.Run(async () => await proc.StandardError.ReadToEndAsync(), ct)
        );
        await proc.WaitForExitAsync(ct);
        return sb.ToString();
    }

    private static string BuildPrompt(IEnumerable<(string role, string content)> messages)
    {
        var sb = new StringBuilder();
        foreach (var (role, content) in messages)
        {
            sb.Append(role switch
            {
                "system" => "[SYSTEM] ",
                "assistant" => "[ASSISTANT] ",
                _ => "[USER] "
            });
            sb.AppendLine(content);
        }
        sb.Append("[ASSISTANT] ");
        return sb.ToString();
    }

    private static string EscapeArg(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

