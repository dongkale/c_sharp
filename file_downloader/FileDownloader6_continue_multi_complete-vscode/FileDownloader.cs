using System.Net;

namespace FileDownloader6;

public partial class FileDownloader : Form
{
    public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
    public const int DEFAULT_FILE_PART_COUNT = 4;

    private List<FileDownloaderItem> downloadItems = new List<FileDownloaderItem>();
    private string downloadFolder = DEFAULT_DOWNLOAD_FOLDER;
    private const int PartCount = DEFAULT_FILE_PART_COUNT; // Number of parts to split the download

    private TextProgressBar overallProgressBar; // Overall progress bar

    // public delegate void FnUpdater(FileDownloaderItem item);

    public FileDownloader()
    {
        InitializeComponent();

        Directory.CreateDirectory(downloadFolder);

        // Initialize the overall progress bar
        overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(15, 45) };
        this.Controls.Add(overallProgressBar);
        // flowLayoutPanel.Controls.Add(overallProgressBar);
    }

    private void FileDownloader_Load(object sender, EventArgs e)
    {
        // Load URLs from internal list
        var urls = new List<string>
        {
            "http://localhost:3030/image-15/image_15.bin",
            "http://localhost:3030/image-12/image_12.bin",
            "http://localhost:3030/image-11/image_11.bin",
            // "http://localhost:3030/image-10/image_10.bin",
            // "http://localhost:3030/image-09/image_09.bin",
            // "http://localhost:3030/image-08/image_08.bin",
            // "http://localhost:3030/image-07/image_07.bin",
        };

        foreach (var url in urls)
        {
            AddDownloadItem(url);
        }
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

    private void AddDownloadItem(string url)
    {
        if (downloadItems.Any(x => x.Url == url))
        {
            MessageBox.Show("이미 추가된 URL입니다.");
            Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
            return;
        }

        FileDownloaderItem item = new FileDownloaderItem(url, downloadFolder);

        downloadItems.Add(item);
        flowLayoutPanel.Controls.Add(item.Panel);

        Logger.Log($"{url} - 다운로드 추가");
    }

    private void btnDownload_Click(object sender, EventArgs e)
    {
        if (downloadItems.Count == 0 || string.IsNullOrEmpty(downloadFolder))
        {
            MessageBox.Show("다운로드 폴더를 지정해주세요.");
            return;
        }

        overallProgressBar.Maximum = 100;  // downloadItems.Count;
        overallProgressBar.Value = 0;

        foreach (var item in downloadItems)
        {
            StartDownload(item);
        }
    }

    private void StartDownload(FileDownloaderItem item)
    {
        Task.Run(() => DownloadFileAsync(item, UpdateOverallProgress));
    }

    private async Task DownloadFileAsync(FileDownloaderItem item, Action<FileDownloaderItem>? updater = null) // FnUpdater? updater = null)
    {
        long totalFileSize = 0;
        long totalBytesReceived = 0;

        if (File.Exists(item.DownloadPath))
        {
            totalBytesReceived = new FileInfo(item.DownloadPath).Length;
        }

        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Get the total file size
                using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, item.Url)))
                {
                    response.EnsureSuccessStatusCode();
                    totalFileSize = response.Content.Headers.ContentLength ?? 0;
                    item.TotalFileSize = totalFileSize;
                }

                if (totalBytesReceived >= totalFileSize)
                {
                    item.TotalBytesReceived = totalFileSize;

                    Invoke(new Action(() => item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize)));
                    Invoke(new Action(() => updater?.Invoke(item)));

                    Invoke(new Action(() => item.UpdateStatus("파일이 이미 완전히 다운로드되었습니다.")));
                    Logger.Log($"{item.Url} - 파일이 이미 완전히 다운로드되었습니다.");
                    return;
                }

                long partSize = totalFileSize / PartCount;
                List<Task> downloadTasks = new List<Task>();
                for (int i = 0; i < PartCount; i++)
                {
                    long start = i * partSize;
                    long end = (i == PartCount - 1) ? totalFileSize - 1 : (start + partSize - 1);
                    downloadTasks.Add(DownloadPartAsync(item, start, end, i, updater));
                }

                await Task.WhenAll(downloadTasks);

                // Ensure parts are combined correctly
                // __old_CombineParts(item, PartCount);
                CombineParts(item.DownloadPath, PartCount);

                Invoke(new Action(() => item.UpdateStatus("다운로드 완료")));

                // Invoke(new Action(() =>
                // {
                //     item.UpdateStatus("다운로드 완료");
                //     // overallProgressBar.Value++;
                //     // overallProgressBar.CustomText = $"{(int)((double)overallProgressBar.Value / overallProgressBar.Maximum * 100)}%";
                // }));

                Logger.Log($"{item.Url} - 다운로드 완료");
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => item.UpdateStatus("다운로드 오류: " + ex.Message)));
                Logger.ErrorLog($"{item.Url} - 다운로드 오류: {ex.Message}");
            }
        }
    }

    private async Task DownloadPartAsync(FileDownloaderItem item, long start, long end, int partIndex, Action<FileDownloaderItem>? updater = null) // FnUpdater? updater = null)
    {
        string tempFilePath = $"{item.DownloadPath}.part{partIndex}";
        long existingLength = 0;

        if (File.Exists(tempFilePath))
        {
            existingLength = new FileInfo(tempFilePath).Length;
            if (existingLength >= (end - start + 1))
            {
                // Skip downloading if the part is already complete
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

                            Invoke(new Action(() => item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize)));
                            Invoke(new Action(() => updater?.Invoke(item)));

                            // Invoke(new Action(() => UpdateOverallProgress()));
                            // updater?.Invoke(item);
                            // if (updater != null)
                            // {
                            //     Invoke(new Action(() => updater(item)));
                            // }

                        }
                    }
                }
            }
        }
    }

    private void __old_CombineParts(FileDownloaderItem item, int partCount)
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

    private void CombineParts(string downloadPath, int partCount)
    {
        using (FileStream output = new FileStream(downloadPath, FileMode.Create))
        {
            for (int i = 0; i < partCount; i++)
            {
                string tempFilePath = $"{downloadPath}.part{i}";
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

    private void UpdateOverallProgress__()
    {
        // item.UpdateProgress(item.TotalBytesReceived, item.TotalFileSize);

        long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);

        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;
        overallProgressBar.Value = percentage;

        // overallProgressBar.Maximum = (int)totalFileSize;
        // overallProgressBar.Value = (int)totalBytesReceived;

        // overallProgressBar.CustomText = $"{percentage}%";

        // overallProgressBar.Value = percentage;
        // overallProgressBar.Text = $"{percentage}%";        
    }

    private void UpdateOverallProgress(FileDownloaderItem item)
    {
        long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);

        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;
        overallProgressBar.Value = percentage;
    }

    public async Task<(bool, string)> GetFileContentAsync(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // 파일 내용을 다운로드
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    return (true, content);
                }
            }
            catch (Exception ex)
            {
                // 오류가 발생하면 예외 메시지를 반환
                return (false, $"다운로드 오류: {ex.Message}");
            }
        }
    }

    // -------------------------------------------------------------------------------------

    private async void btnTest_Click(object sender, EventArgs e)
    {
        string downloadUrl = "http://localhost:3030/newest.txt";

        // string downloadPath = DEFAULT_DOWNLOAD_FOLDER;

        // downloadPath = Path.Combine(txtFolder.Text, Path.GetFileName(downloadUrl));

        // FileDownloader downloader = new FileDownloader();
        //await DownloadFileAsync(downloadUrl, downloadPath);

        // FileContentDownloader downloader = new FileContentDownloader();
        // string content = await GetFileContentAsync(downloadUrl);
        (bool result, string content) = await GetFileContentAsync(downloadUrl);

        // 다운로드한 콘텐츠를 텍스트 박스에 표시
        Logger.ErrorLog($"{result} - {content}"); ;
    }

    // public async Task DownloadFileAsync(string url, string folderPath)
    // {
    //     string fileName = Path.GetFileName(url);
    //     string downloadPath = Path.Combine(folderPath, fileName);
    //     long totalBytesReceived = 0;
    //     long totalFileSize = 0;

    //     using (HttpClient client = new HttpClient())
    //     {
    //         try
    //         {
    //             // 파일 크기를 확인하여 전체 크기 얻기
    //             using (HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)))
    //             {
    //                 response.EnsureSuccessStatusCode();
    //                 totalFileSize = response.Content.Headers.ContentLength ?? 0;
    //             }

    //             // 파일 다운로드
    //             using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
    //             using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
    //             {
    //                 response.EnsureSuccessStatusCode();
    //                 using (Stream responseStream = await response.Content.ReadAsStreamAsync())
    //                 using (FileStream fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None))
    //                 {
    //                     byte[] buffer = new byte[8192];
    //                     int bytesRead;
    //                     while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
    //                     {
    //                         fileStream.Write(buffer, 0, bytesRead);
    //                         totalBytesReceived += bytesRead;

    //                         // 진행률 업데이트
    //                         // int progressPercentage = (int)((double)totalBytesReceived / totalFileSize * 100);
    //                         // progressBar.Value = progressPercentage;
    //                         // statusLabel.Text = $"다운로드 중: {progressPercentage}% ({totalBytesReceived}/{totalFileSize} bytes)";
    //                     }
    //                 }
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //         }
    //     }
    // }    
}
