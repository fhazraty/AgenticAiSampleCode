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
			_http = new HttpClient 
			{ 
				BaseAddress = new Uri(_baseUrl),
				Timeout = TimeSpan.FromMinutes(5) // افزایش timeout برای پاسخ‌های طولانی
			};

			// پیام سیستمی اولیه
			_messages.Add(new { role = "system", content = "همیشه به زبان فارسی پاسخ بده." });
			
			// بررسی اتصال در ابتدا
			CheckServerConnection();
		}

		private async void CheckServerConnection()
		{
			try
			{
				AppendMessage("ℹ️ سیستم", "در حال بررسی اتصال به سرور...", Color.Gray, Color.FromArgb(245, 245, 245));
				
				var response = await _http.GetAsync("/v1/models");
				
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					AppendMessage("✅ سیستم", $"اتصال به سرور برقرار شد.\nآدرس: {_baseUrl}", Color.Green, Color.FromArgb(240, 255, 240));
					
					// نمایش لیست مدل‌ها
					using var doc = JsonDocument.Parse(content);
					if (doc.RootElement.TryGetProperty("data", out var models))
					{
						var modelList = new StringBuilder("مدل‌های موجود:\n");
						foreach (var model in models.EnumerateArray())
						{
							if (model.TryGetProperty("id", out var id))
							{
								modelList.AppendLine($"  • {id.GetString()}");
							}
						}
						AppendMessage("📋 مدل‌ها", modelList.ToString(), Color.Blue, Color.FromArgb(240, 245, 255));
					}
				}
				else
				{
					AppendMessage("⚠️ هشدار", $"سرور پاسخ داد اما با خطا: {response.StatusCode}", Color.Orange, Color.FromArgb(255, 245, 230));
				}
			}
			catch (HttpRequestException ex)
			{
				AppendMessage("❌ خطای اتصال", 
					$"نمی‌توانم به سرور متصل شوم!\n\n" +
					$"آدرس: {_baseUrl}\n" +
					$"خطا: {ex.Message}\n\n" +
					$"راهنمایی:\n" +
					$"1. مطمئن شوید LM Studio یا سرور محلی شما روشن است\n" +
					$"2. بررسی کنید پورت 1234 درست باشد\n" +
					$"3. در LM Studio، سرور را از منوی 'Local Server' راه‌اندازی کنید", 
					Color.Red, Color.FromArgb(255, 230, 230));
			}
			catch (Exception ex)
			{
				AppendMessage("❌ خطا", $"خطای غیرمنتظره: {ex.Message}", Color.Red, Color.FromArgb(255, 230, 230));
			}
		}

		private async void btnSend_Click(object sender, EventArgs e)
		{
			var userInput = txtUserInput.Text.Trim();
			if (string.IsNullOrWhiteSpace(userInput)) return;

			// غیرفعال کردن دکمه ارسال تا پاسخ دریافت شود
			btnSend.Enabled = false;
			txtUserInput.Enabled = false;

			AppendMessage("👤 شما", userInput, Color.FromArgb(0, 102, 204), Color.FromArgb(230, 240, 255));
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
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			});

			using var content = new StringContent(json, Encoding.UTF8, "application/json");

			try
			{
				AppendMessage("⏳ سیستم", "در حال دریافت پاسخ...", Color.Gray, Color.FromArgb(245, 245, 245));
				
				var resp = await _http.PostAsync("/v1/chat/completions", content);
				var respText = await resp.Content.ReadAsStringAsync();

				if (!resp.IsSuccessStatusCode)
				{
					var errorMessage = new StringBuilder();
					errorMessage.AppendLine($"کد خطا: {resp.StatusCode} ({(int)resp.StatusCode})");
					errorMessage.AppendLine($"\nپاسخ سرور:");
					errorMessage.AppendLine(respText);
					
					if (resp.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
					{
						errorMessage.AppendLine("\n⚠️ راهنمایی:");
						errorMessage.AppendLine("• سرویس در دسترس نیست");
						errorMessage.AppendLine("• مدل را در LM Studio بارگذاری کنید");
						errorMessage.AppendLine("• از منوی 'Local Server' سرور را شروع کنید");
						errorMessage.AppendLine($"• مدل مورد نظر: {_model}");
					}
					else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
					{
						errorMessage.AppendLine("\n⚠️ راهنمایی:");
						errorMessage.AppendLine($"• مدل '{_model}' یافت نشد");
						errorMessage.AppendLine("• نام مدل را در کد تغییر دهید");
					}
					
					AppendMessage("❌ خطای سرور", errorMessage.ToString(), Color.Red, Color.FromArgb(255, 230, 230));
					
					// حذف پیام کاربر از تاریخچه
					_messages.RemoveAt(_messages.Count - 1);
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

				AppendMessage("🤖 مدل", answer, Color.FromArgb(34, 139, 34), Color.FromArgb(240, 255, 240));
				_messages.Add(new { role = "assistant", content = answer });
			}
			catch (TaskCanceledException)
			{
				AppendMessage("⏱️ خطای زمان", 
					"زمان انتظار تمام شد!\n" +
					"سرور خیلی دیر پاسخ داد یا اصلاً پاسخ نداد.", 
					Color.Orange, Color.FromArgb(255, 245, 230));
				_messages.RemoveAt(_messages.Count - 1);
			}
			catch (HttpRequestException ex)
			{
				AppendMessage("❌ خطای شبکه", 
					$"نمی‌توانم به سرور متصل شوم!\n\n{ex.Message}\n\n" +
					"لطفاً مطمئن شوید:\n" +
					"• LM Studio باز است\n" +
					"• سرور محلی (Local Server) روشن است\n" +
					$"• آدرس {_baseUrl} صحیح است", 
					Color.Red, Color.FromArgb(255, 230, 230));
				_messages.RemoveAt(_messages.Count - 1);
			}
			catch (Exception ex)
			{
				AppendMessage("⚠️ خطا", $"خطای غیرمنتظره:\n{ex.Message}\n\nنوع خطا: {ex.GetType().Name}", 
					Color.Red, Color.FromArgb(255, 230, 230));
				_messages.RemoveAt(_messages.Count - 1);
			}
			finally
			{
				// فعال کردن مجدد کنترل‌ها
				btnSend.Enabled = true;
				txtUserInput.Enabled = true;
				txtUserInput.Focus();
			}
		}

		private void AppendMessage(string sender, string message, Color senderColor, Color? backgroundColor = null)
		{
			// حرکت به انتهای متن
			txtChat.SelectionStart = txtChat.TextLength;
			txtChat.SelectionLength = 0;

			// اگر رنگ پس‌زمینه تعیین شده، یک بلوک رنگی بساز
			if (backgroundColor.HasValue)
			{
				// افزودن فاصله بالا
				txtChat.SelectionBackColor = Color.Transparent;
				txtChat.AppendText(Environment.NewLine);
			}

			// نوشتن فرستنده با رنگ و Bold
			txtChat.SelectionFont = new Font(txtChat.Font, FontStyle.Bold);
			txtChat.SelectionColor = senderColor;
			txtChat.SelectionBackColor = backgroundColor ?? Color.Transparent;
			txtChat.AppendText($"{sender}:");
			txtChat.AppendText(Environment.NewLine);

			// نوشتن پیام با فونت عادی
			txtChat.SelectionFont = new Font(txtChat.Font, FontStyle.Regular);
			txtChat.SelectionColor = Color.Black;
			txtChat.SelectionBackColor = backgroundColor ?? Color.Transparent;
			txtChat.AppendText(message);
			txtChat.AppendText(Environment.NewLine);

			// فاصله انتهایی
			txtChat.SelectionBackColor = Color.Transparent;
			txtChat.AppendText(Environment.NewLine);

			// اسکرول به انتها
			txtChat.SelectionStart = txtChat.TextLength;
			txtChat.ScrollToCaret();
		}
	}
}
