using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace FileDownloader7;

public class FileDownloaderUI
{
    public const int PROGRESS_BAR_PERCENT = 100;

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
        Panel = new Panel { Width = 470, Height = 20 };
        ProgressBar = new TextProgressBar { Width = 465, Height = 18, Maximum = PROGRESS_BAR_PERCENT, Value = 0, LastValue = 0, CustomText = FileName, VisualMode = ProgressBarDisplayMode.TextAndPercentage, Location = new Point(1, 1) };
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

        // ProgressBar.Maximum = (int)totalBytes;
        // ProgressBar.Value = (int)bytesReceived;

        int currentPercentage = (int)((double)bytesReceived / totalBytes * PROGRESS_BAR_PERCENT);
        if (currentPercentage != ProgressBar.Value)
        {
            // ProgressBar.Value = currentPercentage;
            // ProgressBar.LastValue = currentPercentage;

            // ProgressBar.Invoke(new Action(() => { ProgressBar.Value = currentPercentage; ProgressBar.LastValue = currentPercentage; }));
            ProgressBar.Invoke(new Action(() => ProgressBar.Value = currentPercentage));
        }
    }

    public void UpdateStatus(string url, string status)
    {
        // Logger.Log($"[FileDownloaderUI][{url}] {status}");

        // StatusLabel.Text = status;        
    }
}

