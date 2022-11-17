using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TextSearchApp
{
	internal class TextBoxTraceListener : TraceListener
	{
		private readonly TextBox textBox;

		public TextBoxTraceListener(TextBox textBox)
		{
			if (textBox == null) throw new ArgumentNullException(nameof(textBox));

			this.textBox = textBox;
		}
		/// <inheritdoc />
		public override void Write(string message)
		{
			if (this.textBox.InvokeRequired)
			{
				this.textBox.BeginInvoke((Action<string>)this.Write, message);
				return;
			}

			this.textBox.AppendText(message);
		}

		/// <inheritdoc />
		public override void WriteLine(string message)
		{
			if (this.textBox.InvokeRequired)
			{
				this.textBox.BeginInvoke((Action<string>)this.WriteLine, message);
				return;
			}

			this.textBox.AppendText(message + "\r\n");
		}
	}
}