namespace FileDownloader6;

public class FileDownloaderItem
{
    public string Url { get; private set; }
    public string DownloadPath { get; private set; }
    public TextProgressBar ProgressBar { get; private set; } = default!;
    public Label StatusLabel { get; private set; } = default!;
    public Label UrlLabel { get; private set; } = default!;
    public Panel Panel { get; private set; } = default!;
    public long TotalBytesReceived { get; set; } = 0;
    public long TotalFileSize { get; set; } = 0;
    public bool IsComplete { get; set; } = false;

    public FileDownloaderItem(string url, string folderPath)
    {
        IsComplete = false;
        Url = url;
        DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));

        InitializeComponents();
        InitializeTotalBytesReceived();
    }

    private void InitializeComponents()
    {
        int width = 400;

        Panel = new Panel { Width = 400, Height = 35 };
        ProgressBar = new TextProgressBar { Width = 350, Height = 15, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(0, 15) };
        StatusLabel = new Label { Width = 300, Location = new System.Drawing.Point(0, 35) };
        UrlLabel = new Label { Text = Url, Width = 300, Location = new System.Drawing.Point(0, 0) };

        // Add ProgressBar and StatusLabel to Panel
        Panel.Controls.Add(ProgressBar);
        Panel.Controls.Add(StatusLabel);

        // Add URL Label to ProgressBar
        Panel.Controls.Add(UrlLabel);
    }

    private void InitializeTotalBytesReceived()
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
    }

    public void UpdateProgress(long bytesReceived, long totalBytes)
    {
        if (totalBytes == 0)
        {
            Logger.ErrorLog(String.Format("{0} - 다운로드 오류: {1}", Url, "totalBytes == 0"));
            return;
        }

        ProgressBar.Maximum = (int)totalBytes;
        ProgressBar.Value = (int)bytesReceived;
        // ProgressBar.CustomText = $"{bytesReceived} / {totalBytes} bytes ({(bytesReceived * 100) / totalBytes}%)";

        // Update URL Label to display within ProgressBar
        // UrlLabel.Text = Url;
        return;
    }

    public void UpdateStatus(string status)
    {
        StatusLabel.Text = status;
    }
}