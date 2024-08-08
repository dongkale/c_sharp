namespace FileDownloader7;

partial class FileDownloader
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.Button btnUpdate;
    private System.Windows.Forms.Button btnDownload;
    private System.Windows.Forms.Button btnSettings;
    private System.Windows.Forms.FlowLayoutPanel panelDownloadList;

    private System.Windows.Forms.Button btnTest;

    public const string BTN_DOWNLOAD_START_TEXT = "다운로드 시작";
    public const string BTN_DOWNLOAD_PAUSE_TEXT = "다운로드 중단";

    public const string BTN_UPDATE_START_TEXT = "업데이트";
    public const string BTN_UPDATE_CHECK_TEXT = "업데이트 검사";

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
        this.btnUpdate = new System.Windows.Forms.Button();
        this.btnDownload = new System.Windows.Forms.Button();
        this.btnSettings = new System.Windows.Forms.Button();
        this.panelDownloadList = new System.Windows.Forms.FlowLayoutPanel();

        this.btnTest = new System.Windows.Forms.Button();
        this.SuspendLayout();

        //////////////////////////////////////////////////////////////////////////////
        // 
        // btnUpdate
        // 
        this.btnUpdate.Location = new System.Drawing.Point(12, 12);
        this.btnUpdate.Name = "btnUpdate";
        this.btnUpdate.Size = new System.Drawing.Size(90, 23);
        this.btnUpdate.TabIndex = 0;
        this.btnUpdate.Text = BTN_UPDATE_START_TEXT;
        this.btnUpdate.UseVisualStyleBackColor = true;
        this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
        //////////////////////////////////////////////////////////////////////////////
        // 
        // btnDownload
        // 
        this.btnDownload.Location = new System.Drawing.Point(120, 12);
        this.btnDownload.Name = "btnDownload";
        this.btnDownload.Size = new System.Drawing.Size(90, 23);
        this.btnDownload.TabIndex = 1;
        this.btnDownload.Text = BTN_DOWNLOAD_START_TEXT;
        this.btnDownload.UseVisualStyleBackColor = true;
        this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
        this.btnDownload.Enabled = false;
        // 
        // btnSettings
        // 
        this.btnSettings.Location = new System.Drawing.Point(230, 12);
        this.btnSettings.Name = "btnSettings";
        this.btnSettings.Size = new System.Drawing.Size(90, 23);
        this.btnSettings.TabIndex = 2;
        this.btnSettings.Text = "설정";
        this.btnSettings.UseVisualStyleBackColor = true;
        this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);

        //////////////////////////////////////////////////////////////////////////////
        // 
        // btnTest
        // 
        this.btnTest.Location = new System.Drawing.Point(340, 12);
        this.btnTest.Name = "btnTest";
        this.btnTest.Size = new System.Drawing.Size(80, 23);
        this.btnTest.TabIndex = 4;
        this.btnTest.Text = "테스트";
        this.btnTest.UseVisualStyleBackColor = true;
        this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
        //////////////////////////////////////////////////////////////////////////////

        // 
        // panelDownloadList
        // 
        this.panelDownloadList.AutoScroll = true;
        this.panelDownloadList.Location = new System.Drawing.Point(12, 70);
        this.panelDownloadList.Name = "panelDownloadList";
        this.panelDownloadList.Size = new System.Drawing.Size(430, 180);
        this.panelDownloadList.TabIndex = 3;
        // 
        // FileDownloader
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(455, 280); // Adjusted height to accommodate overall progress bar
        this.Controls.Add(this.btnUpdate);
        this.Controls.Add(this.panelDownloadList);
        this.Controls.Add(this.btnSettings);
        this.Controls.Add(this.btnTest);    // Test
        this.Controls.Add(this.btnDownload);
        this.Name = "FileDownloader";
        this.Text = "다운로드 매니저";
        // this.Load += new System.EventHandler(this.FileDownloader_Load);
        this.Load += new System.EventHandler(this.Loader);
        this.ResumeLayout(false);
    }

    #endregion
}
