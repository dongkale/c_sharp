using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.IO;
// using System.Windows.Forms;
// using System.Drawing;

namespace FileDownloader7;

public enum ServerType
{
    None = 0,
    FileServer,
    AwsS3,
}

public class FileDownloaderItem
{
    public ServerType ServerType { get; private set; } = ServerType.None;
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

    // file server
    private string ApiKey = String.Empty;

    // S3
    private string AwsS3BucketName = String.Empty;
    private AmazonS3Client AwsS3Client = default!;

    public FileDownloaderItem(string url, string folderPath, ServerType serverType)
    {
        this.ServerType = serverType;

        this.IsComplete = false;

        this.Url = url;
        this.FileName = Path.GetFileName(url); ;

        this.DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));

        this.TotalBytesReceived = 0;
        this.TotalFileSize = 0;

        this.UI = new FileDownloaderUI(url);
    }

    public void SetS3(AmazonS3Client s3Client, string bucketName)
    {
        this.AwsS3Client = s3Client;
        this.AwsS3BucketName = bucketName;
    }

    public void SetFileServer(string apiKey)
    {
        this.ApiKey = apiKey;
    }

    private bool IsValidFileServer()
    {
        return this.ApiKey != String.Empty;
    }

    private bool IsValidAwsS3()
    {
        return this.AwsS3Client != null && this.AwsS3BucketName != String.Empty;
    }

    public void InitializeTotalBytesReceived()
    {
        this.TotalBytesReceived = 0;
        for (int i = 0; ; i++)
        {
            string tempFilePath = $"{DownloadPath}.part{i}";
            if (!File.Exists(tempFilePath))
            {
                break;
            }
            this.TotalBytesReceived += new FileInfo(tempFilePath).Length;
        }

        Logger.Log($"[InitializeTotalFileSize][{this.Url}] TotalBytesReceived: {this.TotalBytesReceived}");
    }

    public bool InitializeTotalFileSize()
    {
        bool result = false;
        switch (this.ServerType)
        {
            case ServerType.FileServer:
                if (!this.IsValidFileServer())
                {
                    result = false;
                    Logger.ErrorLog($"[InitializeTotalFileSize][{this.Url}] ApiKey is invalid.");
                    break;
                }

                InitializeTotalFileSize(this.ApiKey);
                // await DownloadHelper.GetFileSizeAsync(this.ApiKey, this.Url).ContinueWith(task =>
                // {
                //     this.TotalFileSize = task.Result;
                //     Logger.Log($"[InitializeTotalFileSize][{this.Url}] TotalFileSize: {this.TotalFileSize}");
                // });
                result = true;
                break;
            case ServerType.AwsS3:
                if (!this.IsValidAwsS3())
                {
                    result = false;
                    Logger.ErrorLog($"[InitializeTotalFileSize][{this.Url}] AwsS3Client or AwsS3BucketName is invalid.");
                    break;
                }

                InitializeTotalFileSize(this.AwsS3Client, this.AwsS3BucketName);
                result = true;
                break;
            default:
                result = false;
                Logger.ErrorLog($"[InitializeTotalFileSize][{this.Url}] ServerType is invalid.");
                break;
        }

        return result;
    }

    private async void InitializeTotalFileSize(string apiKey)
    {
        // TotalFileSize = await DownloadHelper.GetFileSizeAsync(apiKey, Url);
        await DownloadHelper.GetFileSizeAsync(apiKey, this.Url).ContinueWith(task =>
        {
            this.TotalFileSize = task.Result;
            Logger.Log($"[InitializeTotalFileSize][{this.Url}] TotalFileSize: {this.TotalFileSize}");
        });
    }

    private async void InitializeTotalFileSize(AmazonS3Client s3Client, string bucketName)
    {
        string s3FilePath = Utils.ExtractFilePathFromUrl(this.Url);

        await DownloadHelper.GetFileSizeAsync_S3(s3Client, bucketName, s3FilePath).ContinueWith(task =>
        {
            this.TotalFileSize = task.Result;
            Logger.Log($"[InitializeTotalFileSize(S3)][{this.Url}] TotalFileSize: {this.TotalFileSize}");
        });
    }

    public (Task, CancellationTokenSource) ExecuteTask(Action<Action> uiUpdater)
    {
        switch (this.ServerType)
        {
            case ServerType.FileServer:
                return this.ExecuteTask(this.ApiKey, uiUpdater);
            case ServerType.AwsS3:
                return this.ExecuteTask(this.AwsS3Client, this.AwsS3BucketName, uiUpdater);
            default:
                Logger.ErrorLog($"[ExecuteTask][{this.Url}] ServerType is invalid.");
                return (Task.CompletedTask, new CancellationTokenSource());
        }
    }

    private (Task, CancellationTokenSource) ExecuteTask(string apiKey, Action<Action> uiUpdater)
    {
        CancellationTokenSource cts = new();

        var task = Task.Run(() => DownloadHelper.DownloadFileAsync(apiKey, this, uiUpdater, cts)).ContinueWith(task =>
        {
            if (task.Result)
            {
                this.IsComplete = true;
                Logger.Log($"[ExecuteTask][{this.Url}] 다운로드 완료:{task.Result}");
            }
            else
            {
                this.IsComplete = false;
                Logger.ErrorLog($"[ExecuteTask][{this.Url}] 다운로드 오류: {task.Exception?.Message}");
            }
        });

        return (task, cts);
    }

    private (Task, CancellationTokenSource) ExecuteTask(AmazonS3Client s3Client, string bucketName, Action<Action> uiUpdater)
    {
        CancellationTokenSource cts = new();

        var task = Task.Run(() => DownloadHelper.DownloadFileAsync_S3(s3Client, bucketName, this, uiUpdater, cts)).ContinueWith(task =>
        {
            if (task.Result)
            {
                this.IsComplete = true;
                Logger.Log($"[ExecuteTask(S3)][{this.Url}] 다운로드 완료:{task.Result}");
            }
            else
            {
                this.IsComplete = false;
                Logger.ErrorLog($"[ExecuteTask(S3)][{this.Url}] 다운로드 오류: {task.Exception?.Message}");
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

        this.UI?.UpdateProgress(this.Url, bytesReceived, totalBytes);

        this.ProgressUpdated?.Invoke();
    }

    public void UpdateStatus(UpdateStatus status, string statusMessage)
    {
        this.UI.UpdateStatus(this.Url, statusMessage);

        this.StatusUpdated?.Invoke(this, statusMessage);
    }

    public void UpdateValue(long bytesReceived)
    {
        this.TotalBytesReceived += bytesReceived;
    }

    // public bool IsChecksum(string checksum)
    // {
    //     return Utils.CalculateMD5FromFile(DownloadPath) == checksum;
    // }
}

