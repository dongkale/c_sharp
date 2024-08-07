using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace FileDownloader7;

public class FileDownloaderUI
{
    public TextProgressBar ProgressBar { get; private set; } = default!;
    // public Label StatusLabel { get; private set; } = default!;
    // public Label UrlLabel { get; private set; } = default!;
    public Panel Panel { get; private set; } = default!;

    public FileDownloaderUI(string url)
    {
        InitializeComponents(Path.GetFileName(url));
    }

    private void InitializeComponents(string FileName)
    {
        Panel = new Panel { Width = 400, Height = 20 };
        ProgressBar = new TextProgressBar { Width = 350, Height = 18, CustomText = FileName, VisualMode = ProgressBarDisplayMode.TextAndPercentage, Location = new Point(0, 0) };
        // StatusLabel = new Label { Width = 300, Location = new Point(0, 35) };
        // UrlLabel = new Label { Text = FileName, Width = 300, Location = new Point(0, 0) };

        // Add ProgressBar and StatusLabel to Panel
        Panel.Controls.Add(ProgressBar);
        // Panel.Controls.Add(StatusLabel);

        // Add URL Label to ProgressBar
        // Panel.Controls.Add(UrlLabel);
    }

    public void UpdateProgress(string url, long bytesReceived, long totalBytes)
    {
        if (totalBytes == 0)
        {
            Logger.ErrorLog($"{url} - 다운로드 오류: {"totalBytes == 0"}");
            return;
        }

        ProgressBar.Maximum = (int)totalBytes;
        ProgressBar.Value = (int)bytesReceived;
    }

    public void UpdateStatus(string url, string status)
    {
        // Logger.Log($"[FileDownloaderUI][{url}] {status}");

        // StatusLabel.Text = status;        
    }
}

