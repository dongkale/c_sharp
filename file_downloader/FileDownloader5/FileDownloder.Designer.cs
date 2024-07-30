namespace FileDownloader5;

partial class FileDownloder
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.btnDownload = new System.Windows.Forms.Button();
        this.btnSettings = new System.Windows.Forms.Button();
        this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
        this.SuspendLayout();
        // 
        // btnDownload
        // 
        this.btnDownload.Location = new System.Drawing.Point(12, 12);
        this.btnDownload.Name = "btnDownload";
        this.btnDownload.Size = new System.Drawing.Size(100, 23);
        this.btnDownload.TabIndex = 0;
        this.btnDownload.Text = "다운로드 시작";
        this.btnDownload.UseVisualStyleBackColor = true;
        this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
        // 
        // btnSettings
        // 
        this.btnSettings.Location = new System.Drawing.Point(130, 12);
        this.btnSettings.Name = "btnSettings";
        this.btnSettings.Size = new System.Drawing.Size(100, 23);
        this.btnSettings.TabIndex = 1;
        this.btnSettings.Text = "설정";
        this.btnSettings.UseVisualStyleBackColor = true;
        this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
        // 
        // flowLayoutPanel1
        // 
        this.flowLayoutPanel1.AutoScroll = true;
        this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 50);
        this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        this.flowLayoutPanel1.Size = new System.Drawing.Size(760, 500);
        this.flowLayoutPanel1.TabIndex = 2;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(784, 561);
        this.Controls.Add(this.flowLayoutPanel1);
        this.Controls.Add(this.btnSettings);
        this.Controls.Add(this.btnDownload);
        this.Name = "Form1";
        this.Text = "다운로드 매니저";
        this.Load += new System.EventHandler(this.FileDownloder_Load);
        this.ResumeLayout(false);
    }

    private System.Windows.Forms.Button btnDownload;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

    #endregion
}
