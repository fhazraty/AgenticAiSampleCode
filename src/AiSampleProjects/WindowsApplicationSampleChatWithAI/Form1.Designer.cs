using System.Text;
using System.Text.Json;

namespace WindowsApplicationSampleChatWithAI
{
    partial class Form1
    {
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.RichTextBox txtChat;
		private System.Windows.Forms.TextBox txtUserInput;
		private System.Windows.Forms.Button btnSend;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
				components.Dispose();
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			txtChat = new RichTextBox();
			txtUserInput = new TextBox();
			btnSend = new Button();
			SuspendLayout();
			// 
			// txtChat
			// 
			txtChat.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtChat.BackColor = Color.FromArgb(240, 242, 245);
			txtChat.BorderStyle = BorderStyle.None;
			txtChat.Font = new Font("B Nazanin", 12F);
			txtChat.Location = new Point(12, 12);
			txtChat.Name = "txtChat";
			txtChat.ReadOnly = true;
			txtChat.RightToLeft = RightToLeft.Yes;
			txtChat.ScrollBars = RichTextBoxScrollBars.Vertical;
			txtChat.Size = new Size(560, 320);
			txtChat.TabIndex = 0;
			// 
			// txtUserInput
			// 
			txtUserInput.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			txtUserInput.Font = new Font("B Nazanin", 12F);
			txtUserInput.Location = new Point(12, 340);
			txtUserInput.Name = "txtUserInput";
			txtUserInput.RightToLeft = RightToLeft.Yes;
			txtUserInput.Size = new Size(460, 31);
			txtUserInput.TabIndex = 1;
			// 
			// btnSend
			// 
			btnSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			btnSend.Font = new Font("B Nazanin", 12F);
			btnSend.Location = new Point(480, 340);
			btnSend.Name = "btnSend";
			btnSend.Size = new Size(92, 33);
			btnSend.TabIndex = 0;
			btnSend.Text = "ارسال";
			btnSend.UseVisualStyleBackColor = true;
			btnSend.Click += btnSend_Click;
			// 
			// Form1
			// 
			ClientSize = new Size(584, 391);
			Controls.Add(btnSend);
			Controls.Add(txtUserInput);
			Controls.Add(txtChat);
			Font = new Font("B Nazanin", 12F);
			Name = "Form1";
			StartPosition = FormStartPosition.CenterScreen;
			Text = "چت فارسی با مدل محلی";
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
