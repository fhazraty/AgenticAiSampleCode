using System.Text;
using System.Text.Json;

namespace WindowsApplicationSampleChatWithAI
{
    partial class Form1
    {
		private System.ComponentModel.IContainer components = null;
		private Microsoft.Web.WebView2.WinForms.WebView2 webChat;
		private System.Windows.Forms.TextBox txtUserInput;
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.Panel pnlInput;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			webChat = new Microsoft.Web.WebView2.WinForms.WebView2();
			txtUserInput = new TextBox();
			btnSend = new Button();
			pnlInput = new Panel();
			((System.ComponentModel.ISupportInitialize)webChat).BeginInit();
			pnlInput.SuspendLayout();
			SuspendLayout();
			// 
			// webChat
			// 
			webChat.AllowExternalDrop = false;
			webChat.CreationProperties = null;
			webChat.DefaultBackgroundColor = Color.White;
			webChat.Dock = DockStyle.Fill;
			webChat.Location = new Point(0, 0);
			webChat.Name = "webChat";
			webChat.Size = new Size(784, 511);
			webChat.TabIndex = 0;
			webChat.ZoomFactor = 1D;
			// 
			// pnlInput
			// 
			pnlInput.BackColor = Color.FromArgb(240, 242, 245);
			pnlInput.Controls.Add(txtUserInput);
			pnlInput.Controls.Add(btnSend);
			pnlInput.Dock = DockStyle.Bottom;
			pnlInput.Location = new Point(0, 511);
			pnlInput.Name = "pnlInput";
			pnlInput.Padding = new Padding(10);
			pnlInput.Size = new Size(784, 60);
			pnlInput.TabIndex = 1;
			// 
			// txtUserInput
			// 
			txtUserInput.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			txtUserInput.Font = new Font("B Nazanin", 12F);
			txtUserInput.Location = new Point(110, 15);
			txtUserInput.Name = "txtUserInput";
			txtUserInput.RightToLeft = RightToLeft.Yes;
			txtUserInput.Size = new Size(662, 31);
			txtUserInput.TabIndex = 1;
			// 
			// btnSend
			// 
			btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			btnSend.BackColor = Color.FromArgb(0, 120, 215);
			btnSend.FlatStyle = FlatStyle.Flat;
			btnSend.Font = new Font("B Nazanin", 12F, FontStyle.Bold);
			btnSend.ForeColor = Color.White;
			btnSend.Location = new Point(13, 13);
			btnSend.Name = "btnSend";
			btnSend.Size = new Size(90, 35);
			btnSend.TabIndex = 0;
			btnSend.Text = "ارسال";
			btnSend.UseVisualStyleBackColor = false;
			btnSend.Click += btnSend_Click;
			// 
			// Form1
			// 
			ClientSize = new Size(784, 571);
			Controls.Add(webChat);
			Controls.Add(pnlInput);
			Font = new Font("B Nazanin", 12F);
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "چت فارسی با مدل محلی - WebView2";
			((System.ComponentModel.ISupportInitialize)webChat).EndInit();
			pnlInput.ResumeLayout(false);
			pnlInput.PerformLayout();
			ResumeLayout(false);
		}
	}
}
