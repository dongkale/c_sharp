namespace FileDownloader6;

partial class FileDownloader
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    // private System.Windows.Forms.TextBox txtUrl;
    // private System.Windows.Forms.Button btnDownload;
    // private System.Windows.Forms.Button btnSelectFolder;
    // private System.Windows.Forms.TextBox txtFolder;
    // private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    // private System.Windows.Forms.Button btnAddUrl;

    private System.Windows.Forms.Button btnDownload;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;

    private System.Windows.Forms.Button btnTest;

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
        // this.txtUrl = new System.Windows.Forms.TextBox();
        // this.btnDownload = new System.Windows.Forms.Button();
        // this.btnSelectFolder = new System.Windows.Forms.Button();
        // this.txtFolder = new System.Windows.Forms.TextBox();
        // this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
        // this.btnAddUrl = new System.Windows.Forms.Button();
        // this.SuspendLayout();

        // // txtUrl
        // this.txtUrl.Location = new System.Drawing.Point(12, 12);
        // this.txtUrl.Name = "txtUrl";
        // this.txtUrl.Size = new System.Drawing.Size(360, 20);
        // this.txtUrl.TabIndex = 0;

        // // btnAddUrl
        // this.btnAddUrl.Location = new System.Drawing.Point(378, 10);
        // this.btnAddUrl.Name = "btnAddUrl";
        // this.btnAddUrl.Size = new System.Drawing.Size(75, 23);
        // this.btnAddUrl.TabIndex = 1;
        // this.btnAddUrl.Text = "추가";
        // this.btnAddUrl.UseVisualStyleBackColor = true;
        // this.btnAddUrl.Click += new System.EventHandler(this.btnAddUrl_Click);

        // // btnSelectFolder
        // this.btnSelectFolder.Location = new System.Drawing.Point(378, 38);
        // this.btnSelectFolder.Name = "btnSelectFolder";
        // this.btnSelectFolder.Size = new System.Drawing.Size(75, 23);
        // this.btnSelectFolder.TabIndex = 2;
        // this.btnSelectFolder.Text = "폴더 선택";
        // this.btnSelectFolder.UseVisualStyleBackColor = true;
        // this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);

        // // txtFolder
        // this.txtFolder.Location = new System.Drawing.Point(12, 40);
        // this.txtFolder.Name = "txtFolder";
        // this.txtFolder.Size = new System.Drawing.Size(360, 20);
        // this.txtFolder.TabIndex = 3;
        // this.txtFolder.Text = "C:\\downloadedFiles";

        // // btnDownload
        // this.btnDownload.Location = new System.Drawing.Point(459, 10);
        // this.btnDownload.Name = "btnDownload";
        // this.btnDownload.Size = new System.Drawing.Size(75, 23);
        // this.btnDownload.TabIndex = 4;
        // this.btnDownload.Text = "다운로드";
        // this.btnDownload.UseVisualStyleBackColor = true;
        // this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);

        // // flowLayoutPanel1
        // this.flowLayoutPanel1.AutoScroll = true;
        // this.flowLayoutPanel1.Location = new System.Drawing.Point(12, 70);
        // this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        // this.flowLayoutPanel1.Size = new System.Drawing.Size(522, 300);
        // this.flowLayoutPanel1.TabIndex = 5;

        // // DownLoadeForm
        // this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        // this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        // this.ClientSize = new System.Drawing.Size(546, 382);
        // this.Controls.Add(this.flowLayoutPanel1);
        // this.Controls.Add(this.btnDownload);
        // this.Controls.Add(this.txtFolder);
        // this.Controls.Add(this.btnSelectFolder);
        // this.Controls.Add(this.btnAddUrl);
        // this.Controls.Add(this.txtUrl);
        // this.Name = "DownLoadeForm";
        // this.Text = "멀티 파일 다운로더";
        // this.ResumeLayout(false);
        // this.PerformLayout();

        // -----------------------------------------------------------------------

        this.btnDownload = new System.Windows.Forms.Button();
        this.btnSettings = new System.Windows.Forms.Button();
        this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();

        this.btnTest = new System.Windows.Forms.Button();
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

        //////////////////////////////////////////////////////////////////////////////
        // 
        // btnTest
        // 
        this.btnTest.Location = new System.Drawing.Point(245, 12);
        this.btnTest.Name = "btnTest";
        this.btnTest.Size = new System.Drawing.Size(100, 23);
        this.btnTest.TabIndex = 3;
        this.btnTest.Text = "테스트";
        this.btnTest.UseVisualStyleBackColor = true;
        this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
        //////////////////////////////////////////////////////////////////////////////

        // 
        // flowLayoutPanel
        // 
        this.flowLayoutPanel.AutoScroll = true;
        this.flowLayoutPanel.Location = new System.Drawing.Point(12, 70);
        this.flowLayoutPanel.Name = "flowLayoutPanel";
        this.flowLayoutPanel.Size = new System.Drawing.Size(430, 180);
        this.flowLayoutPanel.TabIndex = 2;
        // 
        // FileDownloader
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(455, 280); // Adjusted height to accommodate overall progress bar
        this.Controls.Add(this.flowLayoutPanel);
        this.Controls.Add(this.btnSettings);
        this.Controls.Add(this.btnTest);    // Test
        this.Controls.Add(this.btnDownload);
        this.Name = "FileDownloader";
        this.Text = "다운로드 매니저";
        this.Load += new System.EventHandler(this.FileDownloader_Load);
        this.ResumeLayout(false);
    }

    #endregion
}
