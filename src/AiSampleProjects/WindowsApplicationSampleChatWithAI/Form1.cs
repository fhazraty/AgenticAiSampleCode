using System.Text;
using System.Text.Json;

namespace WindowsApplicationSampleChatWithAI
{
    public partial class Form1 : Form
    {
		private readonly HttpClient _http;
		private readonly string _baseUrl = "http://localhost:1234";
		private readonly string _model = "openai/gpt-oss-20b";
		private readonly List<object> _messages = new();

		public Form1()
		{
			InitializeComponent();
			Console.OutputEncoding = Encoding.UTF8;
			_http = new HttpClient { BaseAddress = new Uri(_baseUrl) };

			// پیام سیستمی اولیه
			_messages.Add(new { role = "system", content = "همیشه به زبان فارسی پاسخ بده." });
		}

		private async void btnSend_Click(object sender, EventArgs e)
		{
			var userInput = txtUserInput.Text.Trim();
			if (string.IsNullOrWhiteSpace(userInput)) return;

			AppendMessage("👤 شما", userInput);
			txtUserInput.Clear();

			_messages.Add(new { role = "user", content = userInput });

			var requestBody = new
			{
				model = _model,
				messages = _messages,
				temperature = 0.7,
				max_tokens = -1,
				stream = false
			};

			var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			try
			{
				var resp = await _http.PostAsync("/v1/chat/completions", content);
				var respText = await resp.Content.ReadAsStringAsync();

				if (!resp.IsSuccessStatusCode)
				{
					AppendMessage("❌ خطا", $"{resp.StatusCode}\n{respText}");
					return;
				}

				using var doc = JsonDocument.Parse(respText);
				var root = doc.RootElement;

				var answer =
					root.TryGetProperty("choices", out var choices) &&
					choices.GetArrayLength() > 0 &&
					choices[0].TryGetProperty("message", out var message) &&
					message.TryGetProperty("content", out var contentEl)
						? contentEl.GetString()
						: "(پاسخی دریافت نشد)";

				AppendMessage("🤖 مدل", answer);
				_messages.Add(new { role = "assistant", content = answer });
			}
			catch (Exception ex)
			{
				AppendMessage("⚠️ خطا", ex.Message);
			}
		}

		private void AppendMessage(string sender, string message)
		{
			txtChat.AppendText($"{sender}: {message}{Environment.NewLine}{Environment.NewLine}");
		}

	}
}
