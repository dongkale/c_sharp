namespace FileDownloader5;

public partial class FileDownloder : Form
{
    private List<DownloadItem> downloadItems = new List<DownloadItem>();
    private string downloadFolder = "C:\\downloadedFiles";
    private const int PartCount = 4; // Number of parts to split the download
    private TextProgressBar overallProgressBar; // Overall progress bar

    public FileDownloder()
    {
        InitializeComponent();

        // downloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "downloads");
        Directory.CreateDirectory(downloadFolder);

        // Initialize the overall progress bar
        overallProgressBar = new TextProgressBar { Width = 760, Height = 20, Location = new System.Drawing.Point(12, 580), VisualMode = ProgressBarDisplayMode.Percentage };
        this.Controls.Add(overallProgressBar);
    }

    private void FileDownloder_Load(object sender, EventArgs e)
    {
        // Load URLs from internal list
        var urls = new List<string>
        {
            "http://localhost:3030/image-15/image_15.bin",
            "http://localhost:3030/image-12/image_12.bin"
        };

        foreach (var url in urls)
        {
            AddDownloadItem(url);
        }
    }

    private void AddDownloadItem(string url)
    {
        if (downloadItems.Any(x => x.Url == url))
        {
            MessageBox.Show("이미 추가된 URL입니다.");
            return;
        }

        DownloadItem item = new DownloadItem(url, downloadFolder);
        downloadItems.Add(item);
        flowLayoutPanel1.Controls.Add(item.Panel);
        Logger.Log($"URL 추가: {url}");
    }

    private void btnDownload_Click(object sender, EventArgs e)
    {
        if (downloadItems.Count == 0)
        {
            MessageBox.Show("URL을 추가해주세요.");
            return;
        }

        overallProgressBar.Maximum = downloadItems.Count;
        overallProgressBar.Value = 0;

        foreach (var item in downloadItems)
        {
            StartDownload(item);
        }
    }

    private void StartDownload(DownloadItem item)
    {
        Task.Run(() => DownloadFileAsync(item));
    }

    private async Task DownloadFileAsync(DownloadItem item)
    {
        long totalFileSize = 0;

        using (HttpClient client = new HttpClient())
        {
            try
            {
                using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, item.Url)))
                {
                    response.EnsureSuccessStatusCode();
                    totalFileSize = response.Content.Headers.ContentLength ?? 0;
                    item.TotalFileSize = totalFileSize;
                }

                long partSize = totalFileSize / PartCount;
                List<Task> downloadTasks = new List<Task>();

                for (int i = 0; i < PartCount; i++)
                {
                    long start = i * partSize;
                    long end = (i == PartCount - 1) ? totalFileSize - 1 : (start + partSize - 1);
                    downloadTasks.Add(DownloadPartAsync(item, start, end, i));
                }

                await Task.WhenAll(downloadTasks);
                CombineParts(item, PartCount);
                Invoke(new Action(() => item.UpdateStatus("다운로드 완료")));
                Logger.Log($"다운로드 완료: {item.Url}");
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => item.UpdateStatus("다운로드 오류: " + ex.Message)));
                Logger.ErrorLog($"다운로드 오류: {item.Url} - {ex.Message}");
            }
        }
    }

    private async Task DownloadPartAsync(DownloadItem item, long start, long end, int partIndex)
    {
        string tempFilePath = $"{item.DownloadPath}.part{partIndex}";
        long existingLength = 0;

        if (File.Exists(tempFilePath))
        {
            existingLength = new FileInfo(tempFilePath).Length;
            if (existingLength >= (end - start + 1))
            {
                return;
            }
            start += existingLength;
        }

        using (HttpClient client = new HttpClient())
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, item.Url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        lock (item)
                        {
                            item.TotalBytesReceived += bytesRead;
                            Invoke(new Action(() => UpdateOverallProgress()));
                        }
                    }
                }
            }
        }
    }

    private void CombineParts(DownloadItem item, int partCount)
    {
        using (FileStream output = new FileStream(item.DownloadPath, FileMode.Create))
        {
            for (int i = 0; i < partCount; i++)
            {
                string tempFilePath = $"{item.DownloadPath}.part{i}";

                if (File.Exists(tempFilePath))
                {
                    using (FileStream input = new FileStream(tempFilePath, FileMode.Open))
                    {
                        input.CopyTo(output);
                    }
                    File.Delete(tempFilePath);
                }
            }
        }
    }

    private void UpdateOverallProgress()
    {
        long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);
        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;
        overallProgressBar.Value = percentage;
        overallProgressBar.Text = $"{percentage}%";
    }


    private void btnSettings_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                downloadFolder = folderDialog.SelectedPath;
                Logger.Log($"다운로드 폴더 변경: {downloadFolder}");
            }
        }
    }
}
