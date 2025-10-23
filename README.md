# 💬 چت فارسی با هوش مصنوعی محلی (Windows Forms + WebView2)

یک برنامه Windows Forms برای چت با مدل‌های هوش مصنوعی به‌صورت محلی (روی سیستم شما) با پشتیبانی کامل از زبان فارسی، Markdown، فرمول‌های ریاضی LaTeX و رنگ‌بندی کد.

![.NET9](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat-square&logo=csharp)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## ✨ امکانات

- 🎯 چت با AI به‌صورت کاملاً محلی (بدون نیاز به اینترنت یا API خارجی)
- 🇮🇷 پشتیبانی کامل از فارسی (راست‌چین، فونت مناسب، متن و جدول)
- 📝 رندر Markdown (عناوین، لیست‌ها، جدول‌ها، نقل‌قول‌ها و ...)
- 🧮 رندر فرمول‌های LaTeX با KaTeX (inline و display)
- 🎨 رنگ‌بندی کد با Highlight.js
- 🖥️ UI مدرن با WebView2
- 🔒 حفظ حریم خصوصی (تمام داده‌ها روی سیستم شما می‌ماند)

---

## 📋 پیش‌نیازها

###1) نصب LM Studio
LM Studio محیطی است برای اجرای مدل‌های هوش مصنوعی روی سیستم شما.

مراحل:
1. از وب‌سایت رسمی دانلود و نصب کنید: https://lmstudio.ai
2. برنامه را اجرا کنید و به تب "Discover" بروید
3. یک مدل مناسب دانلود کنید (چند پیشنهاد در ادامه آمده است)
4. به تب "Chat" رفته و مدل را Load کنید
5. به تب "Local Server" بروید، پورت را1234 بگذارید و Start Server را بزنید

مدل‌های پیشنهادی:
- Qwen2.5-7B-Instruct (کیفیت خوب فارسی)
- Llama-3.2-3B-Instruct (سبک و سریع)
- Mistral-7B-Instruct (عمومی خوب)
- Phi-3.5-mini-instruct (بسیار سبک)

نکته: اگر سیستم ضعیف است، مدل‌های کوچک‌تر را انتخاب کنید (۲ تا۳ گیگابایت).

###2) ابزار توسعه
- Visual Studio2022 یا جدیدتر
- .NET9 SDK
- WebView2 Runtime (معمولاً خودکار نصب می‌شود)

---

## 🚀 راه‌اندازی پروژه

کلون و اجرا:
```bash
git clone https://github.com/fhazraty/AgenticAiSampleCode.git
cd AgenticAiSampleCode/src/AiSampleProjects/WindowsApplicationSampleChatWithAI

dotnet restore
dotnet run
```

---

## ⚙️ تنظیم مدل و سرور

1) آدرس سرور (در صورت نیاز به تغییر پورت):
```csharp
// فایل: src/AiSampleProjects/WindowsApplicationSampleChatWithAI/Form1.cs
private readonly string _baseUrl = "http://localhost:1234"; // اگر پورت عوض شد اینجا را تغییر دهید
```

2) نام مدل (دقیقاً مطابق نامی که در LM Studio لود کرده‌اید):
```csharp
// فایل: src/AiSampleProjects/WindowsApplicationSampleChatWithAI/Form1.cs
private readonly string _model = "qwen2.5-7b-instruct"; // نام مدل خود را قرار دهید
```

نحوه یافتن نام دقیق مدل:
- در LM Studio به تب Local Server بروید و از لیست Model، نام دقیق را کپی کنید.

پارامترهای درخواست (اختیاری):
```csharp
var requestBody = new
{
 model = _model,
 messages = _messages,
 temperature =0.7, //0 تا2 (خلاقیت پاسخ)
 max_tokens = -1, // -1 یعنی بدون محدودیت (در صورت نیاز مقدار دهید)
 stream = false
};
```

---

## 📖 نحوه استفاده

1. LM Studio را اجرا کنید، مدل را Load کنید و Local Server را Start کنید (پورت1234).
2. برنامه را اجرا کنید (dotnet run یا Start در Visual Studio).
3. سوال خود را در کادر پایین بنویسید و ارسال کنید.
4. پاسخ AI با Markdown و LaTeX به‌صورت زیبا رندر می‌شود.

مثال‌های درخواست:
- Markdown: «لیستی از مزایای زبان C# بساز»
- کدنویسی: «یک تابع فیبوناچی در C# بنویس»
- فرمول: «فرمول درجه دوم را با LaTeX بنویس»
- جدول: «جدول مقایسه Python و C# بساز»

---

## 🧮 نمایش فرمول‌ها

Inline Math (درون‌خطی):
```
$a^2 + b^2 = c^2$
```

Display Math (جداگانه):
```
$$x = \frac{-b \pm \sqrt{b^2-4ac}}{2a}$$
```

برنامه به‌طور خودکار `\[ ... \]` و `\( ... \)` را به `$$ ... $$` و `$ ... $` تبدیل می‌کند تا KaTeX بهتر رندر کند.

---

## 🐛 رفع مشکلات رایج

- Service Unavailable (503):
 - مدل در LM Studio لود نشده یا Local Server فعال نیست.
 - مدل را Load کنید، Local Server را Start کنید، نام مدل را بررسی کنید.

- Connection Refused:
 - سرور محلی اجرا نمی‌شود یا پورت درست نیست. پورت1234 را بررسی کنید.

- Model not found:
 - نام مدل در کد با مدل انتخابی در LM Studio مطابقت ندارد.

- فرمول‌ها رندر نمی‌شوند:
 - از علائم `$ ... $` و `$$ ... $$` استفاده کنید.

- کندی:
 - از مدل کوچک‌تر استفاده کنید، GPU Acceleration را در LM Studio فعال کنید، مقدار max_tokens را کاهش دهید.

---

## 🏗️ ساختار پروژه

```
AgenticAiSampleCode/
└─ src/
 └─ AiSampleProjects/
 └─ WindowsApplicationSampleChatWithAI/
 ├─ Form1.cs # منطق برنامه (HttpClient + WebView2)
 ├─ Form1.Designer.cs # تعریف کنترل‌ها و Layout
 ├─ Program.cs # نقطه شروع برنامه
 └─ WindowsApplicationSampleChatWithAI.csproj
```

---

## 📚 کتابخانه‌ها و ابزارها

| ابزار | کاربرد |
|------|--------|
| Microsoft.Web.WebView2 | رندر HTML/CSS/JS در Windows Forms |
| Marked.js | تبدیل Markdown به HTML |
| KaTeX + auto-render | رندر فرمول‌های LaTeX |
| Highlight.js | رنگ‌بندی کد |

---

## 🧪 مثال‌ها

سوال ساده:
```
شما: سلام! چطوری؟
AI: سلام! من یک دستیار هوش مصنوعی هستم. چطور می‌تونم کمکتون کنم؟
```

کد نمونه C#:
```csharp
public static void QuickSort(int[] arr, int low, int high)
{
 if (low < high)
 {
 int pi = Partition(arr, low, high);
 QuickSort(arr, low, pi -1);
 QuickSort(arr, pi +1, high);
 }
}
```

---

## 🤝 مشارکت

- پروژه را Fork کنید، Branch بسازید و Pull Request بدهید.

---

## 📄 مجوز

این پروژه تحت مجوز MIT منتشر شده است.

---

## 👨‍💻 سازنده

- Farhad Hazrati — GitHub: https://github.com/fhazraty

---

## 🌟 حمایت

اگر مفید بود، لطفاً به پروژه ⭐ بدهید.
