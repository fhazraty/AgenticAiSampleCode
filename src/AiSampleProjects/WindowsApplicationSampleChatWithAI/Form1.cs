using System.Text;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using System.Text.RegularExpressions;

namespace WindowsApplicationSampleChatWithAI
{
    public partial class Form1 : Form
    {
		private readonly HttpClient _http;
		private readonly string _baseUrl = "http://localhost:1234";
		private readonly string _model = "openai/gpt-oss-20b";
		private readonly List<object> _messages = new();
		private readonly StringBuilder _chatHtml = new();
		private bool _webViewInitialized = false;

		public Form1()
		{
			InitializeComponent();
			Console.OutputEncoding = Encoding.UTF8;
			_http = new HttpClient 
			{ 
				BaseAddress = new Uri(_baseUrl),
				Timeout = TimeSpan.FromMinutes(5)
			};

			// پیام سیستمی اولیه با راهنمای LaTeX
			_messages.Add(new { 
				role = "system", 
				content = @"همیشه به زبان فارسی پاسخ بده.

برای نوشتن فرمول‌های ریاضی از LaTeX استفاده کن:
- برای فرمول‌های درون‌خطی (inline): $فرمول$
- برای فرمول‌های جداگانه (display): $$فرمول$$

مثال:
- فرمول فیثاغورث: $a^2 + b^2 = c^2$
- معادله درجه دوم: $$x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}$$"
			});
			
			// راه‌اندازی WebView2
			InitializeWebView();
		}

		private async void InitializeWebView()
		{
			try
			{
				await webChat.EnsureCoreWebView2Async(null);
				_webViewInitialized = true;
				
				// تنظیمات WebView2
				webChat.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
				webChat.CoreWebView2.Settings.IsZoomControlEnabled = false;
				
				// بارگذاری HTML اولیه
				LoadInitialHtml();
				
				// بررسی اتصال به سرور
				CheckServerConnection();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"خطا در راه‌اندازی WebView2:\n{ex.Message}", "خطا", 
					MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadInitialHtml()
		{
			var html = @"
<!DOCTYPE html>
<html dir='rtl' lang='fa'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>چت</title>
    
    <!-- Marked.js برای Markdown -->
    <script src='https://cdn.jsdelivr.net/npm/marked/marked.min.js'></script>

    <!-- KaTeX برای فرمول‌های ریاضی -->
    <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.css'>
    <script src='https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/katex.min.js'></script>
    <script src='https://cdn.jsdelivr.net/npm/katex@0.16.9/dist/contrib/auto-render.min.js'></script>
    
    <!-- Highlight.js برای کد -->
    <link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css'>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js'></script>
    
    <style>
        * {
    margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
      body {
            font-family: 'B Nazanin', 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
          padding: 20px;
            direction: rtl;
        }
     
     #chat-container {
            max-width: 900px;
    margin: 0 auto;
        }
     
      .message {
            margin-bottom: 20px;
      animation: fadeIn 0.3s ease-in;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(10px); }
            to { opacity: 1; transform: translateY(0); }
        }

   .message-header {
   font-weight: bold;
    margin-bottom: 8px;
  font-size: 14px;
         display: flex;
            align-items: center;
   gap: 8px;
        }
        
     .message-content {
            padding: 15px 20px;
            border-radius: 12px;
  line-height: 1.8;
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
        }
        
        .user .message-header {
     color: #0066cc;
        }
  
        .user .message-content {
       background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
     margin-left: 80px;
 }
    
        .assistant .message-header {
            color: #228b22;
        }
 
        .assistant .message-content {
         background: white;
         border: 2px solid #e0e0e0;
            margin-right: 80px;
 }
        
        .system .message-header {
    color: #666;
        }
   
        .system .message-content {
            background: #f5f5f5;
            border-left: 4px solid #999;
   font-size: 13px;
        }
      
        .error .message-header {
    color: #d32f2f;
 }
        
        .error .message-content {
        background: #ffebee;
   border-left: 4px solid #d32f2f;
        }
        
     /* استایل Markdown */
        .message-content h1 {
            font-size: 24px;
            margin: 15px 0 10px 0;
            color: #1a1a1a;
    border-bottom: 2px solid #667eea;
          padding-bottom: 5px;
        }
   
        .message-content h2 {
  font-size: 20px;
margin: 12px 0 8px 0;
  color: #2a2a2a;
        }
        
 .message-content h3 {
       font-size: 16px;
            margin: 10px 0 6px 0;
            color: #3a3a3a;
        }
     
        .message-content p {
       margin: 8px 0;
        }
        
.message-content code {
            background: #f4f4f4;
            padding: 2px 6px;
    border-radius: 4px;
          font-family: 'Consolas', 'Courier New', monospace;
     font-size: 13px;
            color: #d32f2f;
        }
        
        .message-content pre {
     background: #2d2d2d;
            color: #f8f8f2;
  padding: 15px;
   border-radius: 8px;
       overflow-x: auto;
       margin: 15px 0;
direction: ltr;
            text-align: left;
}
  
        .message-content pre code {
      background: transparent;
  color: inherit;
        padding: 0;
      }
        
   .message-content ul, .message-content ol {
            margin: 10px 0;
         padding-right: 25px;
        }
        
        .message-content li {
            margin: 5px 0;
        }
   
   .message-content blockquote {
            border-right: 4px solid #667eea;
   padding: 10px 15px;
            margin: 15px 0;
    background: #f9f9f9;
        font-style: italic;
            color: #555;
        }
      
        .message-content table {
            border-collapse: collapse;
      width: 100%;
     margin: 15px 0;
      }
      
        .message-content th, .message-content td {
border: 1px solid #ddd;
 padding: 10px;
     text-align: right;
 }
   
        .message-content th {
            background: #667eea;
 color: white;
            font-weight: bold;
        }
        
        .message-content tr:nth-child(even) {
            background: #f9f9f9;
        }
        
 /* فرمول‌های ریاضی */
     .katex {
 font-size: 1.15em;
 direction: ltr;
        }

        .katex-display {
            margin: 25px 0;
          padding: 15px;
            background: #fafafa;
            border-radius: 8px;
     overflow-x: auto;
            overflow-y: hidden;
        border: 1px solid #e0e0e0;
        }

        /* برای inline math */
        .katex-inline {
            padding: 2px 4px;
     background: #f0f8ff;
         border-radius: 3px;
        }
        
      /* لینک‌ها */
      .message-content a {
color: #667eea;
  text-decoration: none;
            border-bottom: 1px dotted #667eea;
    }
    
 .message-content a:hover {
 border-bottom-style: solid;
        }
        
        /* Scrollbar */
     ::-webkit-scrollbar {
        width: 10px;
        }
    
        ::-webkit-scrollbar-track {
            background: #f1f1f1;
        }
        
        ::-webkit-scrollbar-thumb {
    background: #888;
border-radius: 5px;
        }
      
        ::-webkit-scrollbar-thumb:hover {
            background: #555;
  }
    </style>
</head>
<body>
    <div id='chat-container'></div>
    
    <script>
    // تنظیمات Marked.js
        marked.setOptions({
      breaks: true,
    gfm: true,
      highlight: function(code, lang) {
        if (lang && hljs.getLanguage(lang)) {
return hljs.highlight(code, { language: lang }).value;
       }
                return hljs.highlightAuto(code).value;
 }
        });

        function addMessage(sender, content, type) {
      const container = document.getElementById('chat-container');
            const messageDiv = document.createElement('div');
    messageDiv.className = 'message ' + type;
        
   const headerDiv = document.createElement('div');
    headerDiv.className = 'message-header';
headerDiv.textContent = sender;
            
     const contentDiv = document.createElement('div');
  contentDiv.className = 'message-content';
         
 // تبدیل Markdown به HTML
            if (type === 'assistant' || type === 'user') {
        contentDiv.innerHTML = marked.parse(content);
       } else {
   contentDiv.textContent = content;
            }
    
            messageDiv.appendChild(headerDiv);
            messageDiv.appendChild(contentDiv);
     container.appendChild(messageDiv);
    
     // رندر فرمول‌های ریاضی با تنظیمات بهبود یافته
            if (type === 'assistant' || type === 'user') {
       renderMathInElement(contentDiv, {
         delimiters: [
            {left: '$$', right: '$$', display: true},
       {left: '$', right: '$', display: false},
     {left: '\\[', right: '\\]', display: true},
      {left: '\\(', right: '\\)', display: false}
   ],
   throwOnError: false,
     errorColor: '#cc0000',
     strict: false,
    trust: true,
    fleqn: false,
        macros: {
            '\\RR': '\\mathbb{R}',
            '\\NN': '\\mathbb{N}',
              '\\ZZ': '\\mathbb{Z}',
       '\\QQ': '\\mathbb{Q}'
       }
            });
     }
  
     // اسکرول به پایین
            window.scrollTo(0, document.body.scrollHeight);
      }
  
    function clearChat() {
        document.getElementById('chat-container').innerHTML = '';
        }
    </script>
</body>
</html>";
			
			webChat.NavigateToString(html);
		}

		private async void CheckServerConnection()
		{
			try
			{
				AddMessage("ℹ️ سیستم", "در حال بررسی اتصال به سرور...", "system");
				
				var response = await _http.GetAsync("/v1/models");
				
				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					AddMessage("✅ سیستم", $"اتصال به سرور برقرار شد.\n\n**آدرس:** `{_baseUrl}`", "system");
					
					using var doc = JsonDocument.Parse(content);
					if (doc.RootElement.TryGetProperty("data", out var models))
					{
						var modelList = new StringBuilder("### مدل‌های موجود:\n\n");
						foreach (var model in models.EnumerateArray())
						{
							if (model.TryGetProperty("id", out var id))
							{
								modelList.AppendLine($"- `{id.GetString()}`");
							}
						}
						AddMessage("📋 مدل‌ها", modelList.ToString(), "system");
					}
				}
				else
				{
					AddMessage("⚠️ هشدار", $"سرور پاسخ داد اما با خطا: **{response.StatusCode}**", "error");
				}
			}
			catch (HttpRequestException ex)
			{
				AddMessage("❌ خطای اتصال", 
					$"نمی‌توانم به سرور متصل شوم!\n\n" +
					$"**آدرس:** `{_baseUrl}`\n\n" +
					$"**خطا:** {ex.Message}\n\n" +
					$"### راهنمایی:\n" +
					$"1. مطمئن شوید LM Studio یا سرور محلی شما روشن است\n" +
					$"2. بررسی کنید پورت 1234 درست باشد\n" +
					$"3. در LM Studio، سرور را از منوی 'Local Server' راه‌اندازی کنید", 
					"error");
			}
			catch (Exception ex)
			{
				AddMessage("❌ خطا", $"خطای غیرمنتظره: {ex.Message}", "error");
			}
		}

		private async void btnSend_Click(object sender, EventArgs e)
		{
			if (!_webViewInitialized)
			{
				MessageBox.Show("لطفاً صبر کنید تا WebView آماده شود...", "توجه", 
					MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			var userInput = txtUserInput.Text.Trim();
			if (string.IsNullOrWhiteSpace(userInput)) return;

			btnSend.Enabled = false;
			txtUserInput.Enabled = false;

			AddMessage("👤 شما", userInput, "user");
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
				AddMessage("⏳ سیستم", "در حال دریافت پاسخ...", "system");
				
				var resp = await _http.PostAsync("/v1/chat/completions", content);
				var respText = await resp.Content.ReadAsStringAsync();

				if (!resp.IsSuccessStatusCode)
				{
					var errorMessage = new StringBuilder();
					errorMessage.AppendLine($"**کد خطا:** `{resp.StatusCode}` ({(int)resp.StatusCode})");
					errorMessage.AppendLine($"\n### پاسخ سرور:\n```\n{respText}\n```");
					
					if (resp.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
					{
						errorMessage.AppendLine("\n### ⚠️ راهنمایی:");
						errorMessage.AppendLine("- سرویس در دسترس نیست");
						errorMessage.AppendLine("- مدل را در LM Studio بارگذاری کنید");
						errorMessage.AppendLine("- از منوی 'Local Server' سرور را شروع کنید");
						errorMessage.AppendLine($"- مدل مورد نظر: `{_model}`");
					}
					else if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
					{
						errorMessage.AppendLine("\n### ⚠️ راهنمایی:");
						errorMessage.AppendLine($"- مدل `{_model}` یافت نشد");
						errorMessage.AppendLine("- نام مدل را در کد تغییر دهید");
					}
					
					AddMessage("❌ خطای سرور", errorMessage.ToString(), "error");
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

				// پردازش فرمول‌های ریاضی قبل از نمایش
				answer = ProcessMathFormulas(answer);

				AddMessage("🤖 مدل", answer, "assistant");
				_messages.Add(new { role = "assistant", content = answer });
			}
			catch (TaskCanceledException)
			{
				AddMessage("⏱️ خطای زمان", 
					"زمان انتظار تمام شد!\n\nسرور خیلی دیر پاسخ داد یا اصلاً پاسخ نداد.", 
					"error");
				_messages.RemoveAt(_messages.Count - 1);
			}
			catch (HttpRequestException ex)
			{
				AddMessage("❌ خطای شبکه", 
					$"نمی‌توانم به سرور متصل شوم!\n\n**خطا:** {ex.Message}\n\n" +
					"### لطفاً مطمئن شوید:\n" +
					"- LM Studio باز است\n" +
					"- سرور محلی (Local Server) روشن است\n" +
					$"- آدرس `{_baseUrl}` صحیح است", 
					"error");
				_messages.RemoveAt(_messages.Count - 1);
			}
			catch (Exception ex)
			{
				AddMessage("⚠️ خطا", 
					$"**خطای غیرمنتظره:**\n\n{ex.Message}\n\n**نوع خطا:** `{ex.GetType().Name}`", 
					"error");
				_messages.RemoveAt(_messages.Count - 1);
			}
			finally
			{
				btnSend.Enabled = true;
				txtUserInput.Enabled = true;
				txtUserInput.Focus();
			}
		}

		private string ProcessMathFormulas(string content)
		{
			if (string.IsNullOrEmpty(content))
				return content;

			// تبدیل \[ ... \] به $$ ... $$
			content = Regex.Replace(content, @"\\\[(.*?)\\\]", "$$$$1$$", RegexOptions.Singleline);
			
			// تبدیل \( ... \) به $ ... $
			content = Regex.Replace(content, @"\\\((.*?)\\\)", "$$1$", RegexOptions.Singleline);

			return content;
		}

		private void AddMessage(string sender, string content, string type)
		{
			if (!_webViewInitialized) return;

			// Escape کردن کاراکترهای ویژه برای JavaScript
			var escapedContent = System.Text.Json.JsonSerializer.Serialize(content);
			var escapedSender = System.Text.Json.JsonSerializer.Serialize(sender);
			
			var script = $"addMessage({escapedSender}, {escapedContent}, '{type}');";
			webChat.CoreWebView2.ExecuteScriptAsync(script);
		}
	}
}
