using System;
using System.IO;
// using System.Windows.Forms;
// using System.Drawing;

namespace FileDownloader7;

public class FileDownloaderItem
{
    public string Url { get; private set; }
    public string FileName { get; private set; }
    public string DownloadPath { get; private set; }

    // public TextProgressBar ProgressBar { get; private set; } = default!;
    // // public Label StatusLabel { get; private set; } = default!;
    // // public Label UrlLabel { get; private set; } = default!;
    // public Panel Panel { get; private set; } = default!;

    public long TotalBytesReceived { get; set; } = 0;
    public long TotalFileSize { get; set; } = 0;
    public bool IsComplete { get; set; } = false;

    public event Action? ProgressUpdated;
    public event Action<FileDownloaderItem, string>? StatusUpdated;

    public FileDownloaderUI UI { get; private set; }


    public FileDownloaderItem(string url, string folderPath)
    {
        IsComplete = false;

        this.Url = url;
        FileName = Path.GetFileName(url); ;
        DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));

        TotalBytesReceived = 0;
        TotalFileSize = 0;

        UI = new FileDownloaderUI(url);

        // InitializeComponents__();
        // InitializeTotalBytesReceived();
    }

    // private void InitializeComponents()
    // {
    //     Panel = new Panel { Width = 400, Height = 20 };
    //     ProgressBar = new TextProgressBar { Width = 350, Height = 18, CustomText = FileName, VisualMode = ProgressBarDisplayMode.TextAndPercentage, Location = new Point(0, 0) };
    //     // StatusLabel = new Label { Width = 300, Location = new Point(0, 35) };
    //     // UrlLabel = new Label { Text = FileName, Width = 300, Location = new Point(0, 0) };

    //     // Add ProgressBar and StatusLabel to Panel
    //     Panel.Controls.Add(ProgressBar);
    //     // Panel.Controls.Add(StatusLabel);

    //     // Add URL Label to ProgressBar
    //     // Panel.Controls.Add(UrlLabel);
    // }

    public void InitializeTotalBytesReceived()
    {
        TotalBytesReceived = 0;
        for (int i = 0; ; i++)
        {
            string tempFilePath = $"{DownloadPath}.part{i}";
            if (!File.Exists(tempFilePath))
            {
                break;
            }
            TotalBytesReceived += new FileInfo(tempFilePath).Length;
        }

        Logger.Log($"[FileDownloaderItem][{this.Url}] TotalBytesReceived: {TotalBytesReceived}");
    }

    public async void InitializeTotalFileSize(string apiKey)
    {
        // TotalFileSize = await DownloadHelper.GetFileSizeAsync(Url, apiKey);
        await DownloadHelper.GetFileSizeAsync(this.Url, apiKey).ContinueWith(task =>
        {
            TotalFileSize = task.Result;
            Logger.Log($"[FileDownloaderItem][{this.Url}] TotalFileSize: {TotalFileSize}");
        });
    }

    public (Task, CancellationTokenSource) ExecuteTask(string apiKey, Action<Action> uiUpdater)
    {
        CancellationTokenSource cts = new();

        var task = Task.Run(() => DownloadHelper.DownloadFileAsync(apiKey, this, uiUpdater, cts)).ContinueWith(task =>
        {
            if (task.Result)
            {
                IsComplete = true;
                Logger.Log($"[FileDownloaderItem][{this.Url}] 다운로드 완료:{task.Result}");
            }
            else
            {
                IsComplete = false;
                Logger.ErrorLog($"[FileDownloaderItem][{this.Url}] 다운로드 오류: {task.Exception?.Message}");
            }
        });

        return (task, cts);
    }

    public void UpdateProgress(long bytesReceived, long totalBytes)
    {
        // if (totalBytes == 0)
        // {
        //     Logger.ErrorLog($"{Url} - 다운로드 오류: {"totalBytes == 0"}");
        //     return;
        // }

        // ProgressBar.Maximum = (int)totalBytes;
        // ProgressBar.Value = (int)bytesReceived;
        UI.UpdateProgress(this.Url, bytesReceived, totalBytes);

        ProgressUpdated?.Invoke();
    }

    public void UpdateStatus(UpdateStatus status, string statusMessage)
    {
        // StatusLabel.Text = status;
        UI.UpdateStatus(this.Url, statusMessage);

        StatusUpdated?.Invoke(this, statusMessage);
    }

    public bool IsChecksum(string checksum)
    {
        return Utils.CalculateMD5FromFile(DownloadPath) == checksum;
    }
}

