# AgenticAISample (Local .NET 9 Agent)

یک عامل (Agent) کاملاً لوکال با .NET 9 که بدون اینترنت کار می‌کند و قابلیت‌های زیر را دارد:

- اتصال به مدل زبانی محلی (Ollama HTTP یا اجرای محلی مانند `llama.cpp`)
- اتصال به SQL Server و اجرای Query
- تولید خلاصه گزارش و ذخیره در فایل Markdown
- حافظهٔ ساده مبتنی‌بر فایل (JSON) برای نگه‌داشتن سابقه تعامل

## پیش‌نیازها

- .NET SDK 9
- یکی از این‌ها برای LLM محلی:
  - Ollama (در حالت لوکال، بدون نیاز به اینترنت) و مدل نصب‌شده مثل `llama3.1:8b-instruct`
  - یا یک باینری محلی مثل `llama.cpp` (اجرایی + فایل مدل `gguf`)
- SQL Server لوکال (یا دسترسی به یک سرور داخلی)

## راه‌اندازی سریع

1) وارد پوشه پروژه شوید:

```
cd AgenticAISample
```

2) تنظیمات را در `AgenticAISample/appsettings.json` بررسی/اصلاح کنید:

- Provider: یکی از `OllamaHttp` یا `Process`
- برای Ollama: `Ollama.BaseUrl` و `Agent.Model`
- برای Process: مسیر اجرایی (`ProcessLlm.ExecutablePath`)، الگو آرگومان (`ArgsTemplate`)، مسیر مدل (`ModelPath`)
- اتصال پایگاه داده در `Sql.ConnectionString`
- مسیر حافظه (`Memory.StorePath`) و فولدر گزارش‌ها (`Reports.OutputDir`)

نمونه (پیش‌فرض):

```
{
  "Agent": {
    "Provider": "OllamaHttp",
    "Model": "llama3.1:8b-instruct",
    "SystemPrompt": "You are a helpful, concise local assistant.",
    "MaxTokens": 512,
    "Temperature": 0.3
  },
  "Ollama": { "BaseUrl": "http://localhost:11434" },
  "Sql": { "ConnectionString": "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;" },
  "Memory": { "StorePath": "memory\\agent_memory.json", "MaxMessages": 20 },
  "Reports": { "OutputDir": "reports" }
}
```

3) اگر از Ollama استفاده می‌کنید، سرویس را اجرا کنید و مدل را قبلاً `pull` کرده باشید (به صورت آفلاین از کش لوکال):

```
ollama serve
ollama run llama3.1:8b-instruct
```

4) بیلد و اجرا:

```
dotnet build
dotnet run --project AgenticAISample
```

پس از اجرا، یک گفتگوی نمونه و یک گزارش در فولدر `reports` ساخته می‌شود.

## تغییر Provider

- OllamaHttp: از API لوکال `http://localhost:11434/api/chat` استفاده می‌شود (کاملاً آفلاین با مدل‌های لوکال Ollama).
- Process: هر اجراگر محلی مانند `llama.cpp` را می‌توانید با `ExecutablePath` و `ArgsTemplate` تنظیم کنید. متغیرهای `{MODEL_PATH}`, `{PROMPT}`, `{MAX_TOKENS}`, `{TEMPERATURE}` در الگو جایگزین می‌شوند.

## حافظه

- حافظه ساده به صورت JSON در مسیر `Memory.StorePath` نگه‌داری می‌شود.
- آخرین `MaxMessages` پیام به عنوان کانتکست به مدل داده می‌شود.

## گزارش‌گیری

- متد نمونه در `Program.cs` یک Query ساده از `sys.databases` می‌گیرد، آن را با مدل خلاصه می‌کند و فایل Markdown خروجی می‌دهد.

## نکات

- برای اتصال به SQL Server توصیه می‌شود از یک اکانت با حداقل دسترسی استفاده کنید.
- اگر از `Microsoft.Data.SqlClient` استفاده می‌کنید، مطمئن شوید بسته‌های لازم از کش محلی NuGet قابل بازیابی هستند.
- برای کاملاً آفلاین بودن، مدل‌ها باید از قبل روی سیستم نصب و آماده باشند.

