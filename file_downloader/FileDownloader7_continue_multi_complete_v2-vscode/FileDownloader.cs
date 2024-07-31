namespace FileDownloader7;

using System.Text.Json;

#pragma warning disable CS8600
#pragma warning disable CS8602

public partial class FileDownloader : Form
{
    public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
    public const int DEFAULT_FILE_PART_COUNT = 4;

    private List<FileDownloaderItem> downloadItems = new List<FileDownloaderItem>();
    private string downloadFolder = DEFAULT_DOWNLOAD_FOLDER;

    private TextProgressBar overallProgressBar; // Overall progress bar

    public FileDownloader()
    {
        InitializeComponent();

        Directory.CreateDirectory(downloadFolder);

        // Initialize the overall progress bar
        // overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(15, 45) };
        // this.Controls.Add(overallProgressBar);
    }

    private void btnUpdate_Click(object sender, EventArgs e)
    {
        if (FileDownloader_Load())
        {
            btnDownload.Enabled = true;
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

    private void btnDownload_Click(object sender, EventArgs e)
    {
        if (downloadItems.Count == 0 || string.IsNullOrEmpty(downloadFolder))
        {
            MessageBox.Show("다운로드 폴더를 지정해주세요.");
            return;
        }

        overallProgressBar.Maximum = 100;
        overallProgressBar.Value = 0;

        foreach (var item in downloadItems)
        {
            StartDownload(item);
        }
    }

    // private void FileDownloader_Load(object sender, EventArgs e)
    private void Loader(object sender, EventArgs e)
    {
        Logger.Log($"[Load] Start");
    }

    private bool FileDownloader_Load()
    {
        // Load URLs from internal list
        var urls = new List<string>
        {
            "http://localhost:3030/image-15/image_15.bin",
            // "http://localhost:3030/image-12/image_12.bin",
            // "http://localhost:3030/image-11/image_11.bin",
            "http://localhost:3030/image-10/image_10.bin",
            // "http://localhost:3030/image-09/image_09.bin",
            // "http://localhost:3030/image-08/image_08.bin",
            // "http://localhost:3030/image-07/image_07.bin",
        };

        foreach (var url in urls)
        {
            AddDownloadItem(url);
        }

        // Initialize the overall progress bar
        overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(15, 45) };
        this.Controls.Add(overallProgressBar);

        return true;
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
        item.ProgressUpdated += UpdateOverallProgress;
        item.StatusUpdated += (sender, status) => Logger.Log($"{url} - {status}");

        downloadItems.Add(item);
        flowLayoutPanel.Controls.Add(item.Panel);

        Logger.Log($"{url} - 다운로드 추가");
    }

    private void StartDownload(FileDownloaderItem item)
    {
        Task.Run(() => DownloadHelper.DownloadFileAsync(item, UpdateUI));
    }

    private void UpdateOverallProgress()
    {
        long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);

        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;

        overallProgressBar.Value = percentage;

        // if (overallProgressBar.InvokeRequired)
        // {
        //     overallProgressBar.Invoke(new Action(() => overallProgressBar.Value = percentage));
        // }
        // else
        // {
        //     overallProgressBar.Value = percentage;
        // }
    }

    private void UpdateUI(Action updateAction)
    {
        Invoke(updateAction);

        // if (InvokeRequired)
        // {
        //     Invoke(updateAction);
        // }
        // else
        // {
        //     updateAction();
        // }
    }



    public class FilePathData
    {
        public string filePath { get; set; } = string.Empty;
        public string checksum { get; set; } = string.Empty;
    }

    public class ResponseData<T>
    {
        public int resultCode { get; set; } = 0;
        public string resultMessage { get; set; } = string.Empty;
        public List<T> resultData { get; set; } = [];
    }

    private async void btnTest_Click(object sender, EventArgs e)
    {
        // string downloadUrl = "http://localhost:3030/newest.txt"; // 5a1000e8775ccb62e436468adcbfb243
        // // string downloadUrl = "http://localhost:3030/image-12/image_12.bin"; // a34ee55dbb3aa4ac993eb7454b1f4d15

        // string s = Utils.CalculateMD5FromFile("C:\\downloadedFiles\\image_10.bin");

        // (bool result, string content) = await Utils.GetFileContentAsync(downloadUrl);

        // string s2 = Utils.CalculateMD5FromString(content);

        // // 다운로드한 콘텐츠를 텍스트 박스에 표시        
        // string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        // string s3 = await Utils.CalculateMD5FromUrlAsync("http://localhost:3030/image-12/image_12.bin");

        // Logger.Log($"{result} - {string.Join(",", lines)}");



        try
        {
            string url = "http://localhost:3030/download-file/info";

            // string jsonContent = "{\"key1\":\"value\", \"key\":\"value2\"}";


            Dictionary<string, object> jsonDict = new Dictionary<string, object>();
            jsonDict.Add("key1", "items1");
            jsonDict.Add("key2", "items2");

            string jsonContent = JsonSerializer.Serialize(jsonDict);
            string apiKey = "1ab2c3d4e5f61ab2c3d4e5f6";

            // ---
            (bool result, string response) = await Utils.PostAsync(url, jsonContent, apiKey);
            if (!result)
            {
                Console.WriteLine($"Response from GET request: {response}");
                Logger.Log($"{response}");
            }

            // null 리터럴 또는 가능한 null 값을 null을 허용하지 않는 형식으로 변환하는 중입니다.
            ResponseData<FilePathData> r = JsonSerializer.Deserialize<ResponseData<FilePathData>>(response);
            if (!result)
            {
                Console.WriteLine("Response is null");
                return;
            }


            foreach (var item in r.resultData)
            {
                Console.WriteLine($" {item.filePath} - {item.checksum}");
            }
            // ---

            // ---
            // (bool is__, string message__, List<FilePathData> resultData) = await Utils.CallApi<FilePathData>(url, jsonContent, apiKey);

            // foreach (var item in resultData)
            // {
            //     Console.WriteLine($" {item.filePath} - {item.checksum}");
            // }
            // ---

            // using (JsonDocument doc = JsonDocument.Parse(response))
            // {
            //     JsonElement root = doc.RootElement;
            //     int resultCode = root.GetProperty("resultCode").GetInt32();
            //     string resultMessage = root.GetProperty("resultMessage").GetString();
            //     JsonElement resultData = root.GetProperty("resultData");
            //     foreach (JsonElement item in resultData.EnumerateArray())
            //     {
            //         string f__ = item.GetProperty("file").GetString();
            //         string c__ = item.GetProperty("checksum").GetString();

            //         Console.WriteLine($" {f__} - {c__}");
            //     }

            // }

            (bool isResult, string resuleMessage, List<FilePathData> resultData) = await Utils.CallApi<List<FilePathData>>(url, jsonContent, apiKey);
            if (!isResult)
            {
                Console.WriteLine("Response is false");
                return;
            }

            foreach (var item in resultData)
            {
                Console.WriteLine($" {item.filePath} - {item.checksum}");
            }

            // var o = JsonSerializer.Deserialize(response);

            Logger.Log($"{response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

// public partial class FileDownloader : Form
// {
//     public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
//     public const int DEFAULT_FILE_PART_COUNT = 4;

//     private List<FileDownloaderItem> downloadItems = new List<FileDownloaderItem>();
//     private string downloadFolder = DEFAULT_DOWNLOAD_FOLDER;

//     private TextProgressBar overallProgressBar; // Overall progress bar

//     public FileDownloader()
//     {
//         InitializeComponent();

//         Directory.CreateDirectory(downloadFolder);

//         // Initialize the overall progress bar
//         overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(15, 45) };
//         this.Controls.Add(overallProgressBar);
//     }

//     private void FileDownloader_Load(object sender, EventArgs e)
//     {
//         // Load URLs from internal list
//         var urls = new List<string>
//         {
//             "http://localhost:3030/image-15/image_15.bin",
//             // "http://localhost:3030/image-12/image_12.bin",
//             // "http://localhost:3030/image-11/image_11.bin",
//             // "http://localhost:3030/image-10/image_10.bin",
//             // "http://localhost:3030/image-09/image_09.bin",
//             // "http://localhost:3030/image-08/image_08.bin",
//             // "http://localhost:3030/image-07/image_07.bin",
//         };

//         foreach (var url in urls)
//         {
//             AddDownloadItem(url);
//         }
//     }

//     private void btnSettings_Click(object sender, EventArgs e)
//     {
//         using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
//         {
//             if (folderDialog.ShowDialog() == DialogResult.OK)
//             {
//                 downloadFolder = folderDialog.SelectedPath;
//                 Logger.Log($"다운로드 폴더 변경: {downloadFolder}");
//             }
//         }
//     }

//     private void AddDownloadItem(string url)
//     {
//         if (downloadItems.Any(x => x.Url == url))
//         {
//             MessageBox.Show("이미 추가된 URL입니다.");
//             Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
//             return;
//         }

//         FileDownloaderItem item = new FileDownloaderItem(url, downloadFolder);
//         item.ProgressUpdated += UpdateOverallProgress;
//         item.StatusUpdated += (sender, status) => Logger.Log($"{url} - {status}");

//         downloadItems.Add(item);
//         flowLayoutPanel.Controls.Add(item.Panel);

//         Logger.Log($"{url} - 다운로드 추가");
//     }

//     private void btnDownload_Click(object sender, EventArgs e)
//     {
//         if (downloadItems.Count == 0 || string.IsNullOrEmpty(downloadFolder))
//         {
//             MessageBox.Show("다운로드 폴더를 지정해주세요.");
//             return;
//         }

//         overallProgressBar.Maximum = 100;
//         overallProgressBar.Value = 0;

//         foreach (var item in downloadItems)
//         {
//             StartDownload(item);
//         }
//     }

//     private void StartDownload(FileDownloaderItem item)
//     {
//         Task.Run(() => DownloadHelper.DownloadFileAsync(item));
//     }

//     private void UpdateOverallProgress()
//     {
//         long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
//         long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);

//         int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;
//         overallProgressBar.Value = percentage;
//     }

//     private async void btnTest_Click(object sender, EventArgs e)
//     {
//         string downloadUrl = "http://localhost:3030/newest.txt";

//         (bool result, string content) = await DownloadHelper.GetFileContentAsync(downloadUrl);

//         // 다운로드한 콘텐츠를 텍스트 박스에 표시
//         Logger.ErrorLog($"{result} - {content}"); ;
//     }
// }
