using System.Data;
using System.Text;

namespace AgenticAISample.Reporting;

public sealed class ReportGenerator
{
    private readonly string _outputDir;

    public ReportGenerator(AgentConfig config)
    {
        _outputDir = config.Reports.OutputDir;
    }

    public async Task<string> WriteMarkdownAsync(string title, string body, string fileName, CancellationToken ct = default)
    {
        Directory.CreateDirectory(_outputDir);
        var path = Path.Combine(_outputDir, fileName);
        var sb = new StringBuilder();
        sb.AppendLine($"# {title}");
        sb.AppendLine();
        sb.AppendLine(body);
        await File.WriteAllTextAsync(path, sb.ToString(), ct);
        return path;
    }

    public static string DataTableToMarkdown(DataTable table)
    {
        if (table.Rows.Count == 0)
            return "(No rows)";

        var sb = new StringBuilder();
        // Header
        for (int c = 0; c < table.Columns.Count; c++)
        {
            sb.Append("|").Append(table.Columns[c].ColumnName);
        }
        sb.AppendLine("|");
        for (int c = 0; c < table.Columns.Count; c++) sb.Append("|---");
        sb.AppendLine("|");

        // Rows
        foreach (DataRow row in table.Rows)
        {
            for (int c = 0; c < table.Columns.Count; c++)
            {
                sb.Append("|").Append(Convert.ToString(row[c]) ?? string.Empty);
            }
            sb.AppendLine("|");
        }

        return sb.ToString();
    }
}

