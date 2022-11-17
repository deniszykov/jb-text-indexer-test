
namespace TextSearchApp
{
	partial class TextIndexAndLookupWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pathList = new System.Windows.Forms.ListBox();
			this.addFileButton = new System.Windows.Forms.Button();
			this.removePathButton = new System.Windows.Forms.Button();
			this.searchBox = new System.Windows.Forms.TextBox();
			this.searchLabel = new System.Windows.Forms.Label();
			this.searchResultBox = new System.Windows.Forms.ListBox();
			this.logsTextBox = new System.Windows.Forms.TextBox();
			this.logsLabel = new System.Windows.Forms.Label();
			this.addDirectoryButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// pathList
			// 
			this.pathList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pathList.FormattingEnabled = true;
			this.pathList.Location = new System.Drawing.Point(12, 12);
			this.pathList.Name = "pathList";
			this.pathList.Size = new System.Drawing.Size(402, 173);
			this.pathList.TabIndex = 0;
			this.pathList.SelectedIndexChanged += new System.EventHandler(this.pathList_SelectedIndexChanged);
			// 
			// addFileButton
			// 
			this.addFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addFileButton.Location = new System.Drawing.Point(420, 12);
			this.addFileButton.Name = "addFileButton";
			this.addFileButton.Size = new System.Drawing.Size(30, 23);
			this.addFileButton.TabIndex = 1;
			this.addFileButton.Text = "+F";
			this.addFileButton.UseVisualStyleBackColor = true;
			this.addFileButton.Click += new System.EventHandler(this.addPathButton_Click);
			// 
			// removePathButton
			// 
			this.removePathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.removePathButton.Enabled = false;
			this.removePathButton.Location = new System.Drawing.Point(420, 70);
			this.removePathButton.Name = "removePathButton";
			this.removePathButton.Size = new System.Drawing.Size(30, 23);
			this.removePathButton.TabIndex = 2;
			this.removePathButton.Text = "-";
			this.removePathButton.UseVisualStyleBackColor = true;
			this.removePathButton.Click += new System.EventHandler(this.removePathButton_Click);
			// 
			// searchBox
			// 
			this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchBox.Location = new System.Drawing.Point(12, 225);
			this.searchBox.Name = "searchBox";
			this.searchBox.Size = new System.Drawing.Size(402, 20);
			this.searchBox.TabIndex = 3;
			this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
			// 
			// searchLabel
			// 
			this.searchLabel.AutoSize = true;
			this.searchLabel.Location = new System.Drawing.Point(12, 206);
			this.searchLabel.Name = "searchLabel";
			this.searchLabel.Size = new System.Drawing.Size(73, 13);
			this.searchLabel.TabIndex = 4;
			this.searchLabel.Text = "Search Word:";
			// 
			// searchResultBox
			// 
			this.searchResultBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.searchResultBox.FormattingEnabled = true;
			this.searchResultBox.Location = new System.Drawing.Point(12, 251);
			this.searchResultBox.Name = "searchResultBox";
			this.searchResultBox.Size = new System.Drawing.Size(402, 108);
			this.searchResultBox.TabIndex = 0;
			// 
			// logsTextBox
			// 
			this.logsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.logsTextBox.Location = new System.Drawing.Point(12, 394);
			this.logsTextBox.Multiline = true;
			this.logsTextBox.Name = "logsTextBox";
			this.logsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.logsTextBox.Size = new System.Drawing.Size(402, 150);
			this.logsTextBox.TabIndex = 5;
			// 
			// logsLabel
			// 
			this.logsLabel.AutoSize = true;
			this.logsLabel.Location = new System.Drawing.Point(9, 378);
			this.logsLabel.Name = "logsLabel";
			this.logsLabel.Size = new System.Drawing.Size(33, 13);
			this.logsLabel.TabIndex = 4;
			this.logsLabel.Text = "Logs:";
			// 
			// addDirectoryButton
			// 
			this.addDirectoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addDirectoryButton.Location = new System.Drawing.Point(420, 41);
			this.addDirectoryButton.Name = "addDirectoryButton";
			this.addDirectoryButton.Size = new System.Drawing.Size(30, 23);
			this.addDirectoryButton.TabIndex = 1;
			this.addDirectoryButton.Text = "+D";
			this.addDirectoryButton.UseVisualStyleBackColor = true;
			this.addDirectoryButton.Click += new System.EventHandler(this.addPathButton_Click);
			// 
			// TextIndexAndLookupWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(462, 556);
			this.Controls.Add(this.logsTextBox);
			this.Controls.Add(this.logsLabel);
			this.Controls.Add(this.searchLabel);
			this.Controls.Add(this.searchBox);
			this.Controls.Add(this.removePathButton);
			this.Controls.Add(this.addDirectoryButton);
			this.Controls.Add(this.addFileButton);
			this.Controls.Add(this.searchResultBox);
			this.Controls.Add(this.pathList);
			this.MinimumSize = new System.Drawing.Size(380, 504);
			this.Name = "TextIndexAndLookupWindow";
			this.Text = "Text Search App";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox pathList;
		private System.Windows.Forms.Button addFileButton;
		private System.Windows.Forms.Button removePathButton;
		private System.Windows.Forms.TextBox searchBox;
		private System.Windows.Forms.Label searchLabel;
		private System.Windows.Forms.ListBox searchResultBox;
		private System.Windows.Forms.TextBox logsTextBox;
		private System.Windows.Forms.Label logsLabel;
		private System.Windows.Forms.Button addDirectoryButton;
	}
}

