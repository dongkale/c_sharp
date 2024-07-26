/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014-10-07
 * Time: 오후 6:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace SimpleDownloader
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
            this.prgDownloadOne = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAddress = new System.Windows.Forms.TextBox();
            this.lblProgress = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.txtAddressList = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStart_ = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // prgDownloadOne
            // 
            this.prgDownloadOne.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.prgDownloadOne.Location = new System.Drawing.Point(110, 307);
            this.prgDownloadOne.Margin = new System.Windows.Forms.Padding(5);
            this.prgDownloadOne.Name = "prgDownloadOne";
            this.prgDownloadOne.Size = new System.Drawing.Size(671, 30);
            this.prgDownloadOne.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Font = new System.Drawing.Font("굴림", 10F);
            this.label1.Location = new System.Drawing.Point(14, 265);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 30);
            this.label1.TabIndex = 7;
            this.label1.Text = "Current";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAddress
            // 
            this.txtAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAddress.Font = new System.Drawing.Font("굴림", 10F);
            this.txtAddress.Location = new System.Drawing.Point(110, 269);
            this.txtAddress.Margin = new System.Windows.Forms.Padding(5);
            this.txtAddress.Name = "txtAddress";
            this.txtAddress.ReadOnly = true;
            this.txtAddress.Size = new System.Drawing.Size(671, 30);
            this.txtAddress.TabIndex = 1;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblProgress.Font = new System.Drawing.Font("굴림", 10F);
            this.lblProgress.Location = new System.Drawing.Point(14, 307);
            this.lblProgress.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(93, 30);
            this.lblProgress.TabIndex = 8;
            this.lblProgress.Text = "Progress";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.Font = new System.Drawing.Font("굴림", 10F);
            this.label2.Location = new System.Drawing.Point(13, 347);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 30);
            this.label2.TabIndex = 9;
            this.label2.Text = "Folder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.Font = new System.Drawing.Font("굴림", 10F);
            this.txtFolder.Location = new System.Drawing.Point(110, 347);
            this.txtFolder.Margin = new System.Windows.Forms.Padding(5);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(671, 30);
            this.txtFolder.TabIndex = 3;
            this.txtFolder.Text = "C:\\downloadedFiles";
            // 
            // txtAddressList
            // 
            this.txtAddressList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAddressList.Font = new System.Drawing.Font("굴림", 10F);
            this.txtAddressList.Location = new System.Drawing.Point(110, 13);
            this.txtAddressList.Margin = new System.Windows.Forms.Padding(4);
            this.txtAddressList.Name = "txtAddressList";
            this.txtAddressList.Size = new System.Drawing.Size(671, 247);
            this.txtAddressList.TabIndex = 0;
            this.txtAddressList.Text = "";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("굴림", 10F);
            this.label3.Location = new System.Drawing.Point(11, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 30);
            this.label3.TabIndex = 6;
            this.label3.Text = "URL";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnStart_
            // 
            this.btnStart_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart_.Font = new System.Drawing.Font("굴림", 10F);
            this.btnStart_.Location = new System.Drawing.Point(633, 390);
            this.btnStart_.Margin = new System.Windows.Forms.Padding(4);
            this.btnStart_.Name = "btnStart_";
            this.btnStart_.Size = new System.Drawing.Size(148, 49);
            this.btnStart_.TabIndex = 10;
            this.btnStart_.Text = "Download";
            this.btnStart_.UseVisualStyleBackColor = true;
            this.btnStart_.Click += new System.EventHandler(this.btnStart__Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(797, 452);
            this.Controls.Add(this.btnStart_);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtAddressList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.prgDownloadOne);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.txtAddress);
            this.Font = new System.Drawing.Font("굴림", 11F);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.MaximumSize = new System.Drawing.Size(3000, 3000);
            this.MinimumSize = new System.Drawing.Size(356, 354);
            this.Name = "MainForm";
            this.Text = "Simple Multi-Line Downloader";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		private System.Windows.Forms.Label lblProgress;
		private System.Windows.Forms.ProgressBar prgDownloadOne;
		private System.Windows.Forms.TextBox txtAddress;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.RichTextBox txtAddressList;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStart_;
    }
}
