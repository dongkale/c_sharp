namespace FileDownloader7;

using System.Drawing;
using System.Text.Json;

#pragma warning disable CS8600
#pragma warning disable CS8602

public partial class FileDownloader : Form
{
    public const string DEFAULT_FILE_SERVER_BASE_URL = "http://localhost:3050";
    public const string DEFAULT_API_SERVER_BASE_URL = "http://localhost:3050";
    public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
    public const int DEFAULT_FILE_PART_COUNT = 4;
    public const string DEFAULT_API_KEY = "1ab2c3d4e5f61ab2c3d4e5f6";

    public const string DOWNLOAD_INFO_API_URL = "download-file/info";

    private List<FileDownloaderItem> downloadItems = new List<FileDownloaderItem>();
    private string downloadFolder = DEFAULT_DOWNLOAD_FOLDER;

    private TextProgressBar overallProgressBar = default!; // Overall progress bar

    public class FilePathData
    {
        public string filePath { get; set; } = string.Empty;
        public string checksum { get; set; } = string.Empty;
        public bool updateStatus { get; set; } = false;
    }


    public FileDownloader()
    {
        InitializeComponent();

        Directory.CreateDirectory(downloadFolder);

        // Initialize the overall progress bar
        // overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new Point(15, 45) };
        // this.Controls.Add(overallProgressBar);
    }

    private async void btnUpdate_Click(object sender, EventArgs e)
    {
        btnUpdate.Text = "업데이트 검사...";
        btnUpdate.Enabled = false;

        (bool isResult, string message) = await SetupDownload();

        btnUpdate.Text = "업데이트";
        btnUpdate.Enabled = true;

        if (!isResult)
        {
            MessageBox.Show(message);
            return;
        }

        // this.Controls.Clear();

        overallProgressBar = new TextProgressBar
        {
            Width = 480,
            Height = 20,
            VisualMode = ProgressBarDisplayMode.Percentage,
            Location = new Point(15, 45),
            Maximum = 100,
            Value = 0
        };

        this.Controls.Add(overallProgressBar);
        // this.ResumeLayout(false);

        btnDownload.Enabled = true;
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                downloadFolder = folderDialog.SelectedPath;
                Logger.Log($"[btnSettings_Click] 다운로드 폴더 변경: {downloadFolder}");
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

        // overallProgressBar.Maximum = 100;
        // overallProgressBar.Value = 0;

        btnDownload.Enabled = false;

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

    //private async Task<bool> FileDownloader_Load()
    private async Task<(bool, string)> SetupDownload()
    {
        string apiKey = DEFAULT_API_KEY;


        var files = new List<FilePathData>
        {
            new FilePathData { filePath = "patch/text01.txt", checksum = "cb08ca4a7bb5f9683c19133a84872ca7", updateStatus = true },
            new FilePathData { filePath = "patch/text02.txt", checksum = "f38c26a09c89158123f77b474221cc8a", updateStatus = true },
            new FilePathData { filePath = "patch/text03.txt", checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", updateStatus = true },
            new FilePathData { filePath = "image-15/image_15.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", updateStatus = true },
            // new FilePathData { filePath = "image-12/image_12.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15" },
            // new FilePathData { filePath = "image-11/image_11.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15" },
            // new FilePathData { filePath = "image-10/image_10.bin", checksum = "cb3fffcc0e7c5b2874c639a4107b3a6a" },
            new FilePathData { filePath = "image-09/image_09.bin", checksum = "30eb7e71f05abc3a12ce3fcd589debd6", updateStatus = true },
            // new FilePathData { filePath = "image-08/image_08.bin", checksum = "38db288725fa54ccbf0b92a39e69b78a" },
            // new FilePathData { filePath = "image-07/image_07.bin", checksum = "15d24a1d77ccd2f3983a09dec2374004" },
            // new FilePathData { filePath = "image-06/image_06.bin", checksum = "1f7e5a19cb4ace806a37cd72f3cb6172" },                        
        };

        //////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        // var urls = new List<string>();

        // string apiUrl = $"{DEFAULT_API_SERVER_BASE_URL}/{DOWNLOAD_INFO_API_URL}";
        // string jsonContent = JsonSerializer.Serialize(
        //     new
        //     {
        //         key1 = "items1",
        //         key2 = "items2"
        //     }
        // );


        // (bool isResult, string message, List<FilePathData> files) = await Utils.CallApi<List<FilePathData>>(apiUrl, jsonContent, apiKey);
        // if (!isResult)
        // {
        //     MessageBox.Show("서버에서 다운로드 정보를 가져올 수 없습니다.");
        //     return false;
        // }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////// 

        // 1. url 유효하지 않으면 다운로드 중단
        // 2. checksum이 일치하지 않으면 다운로드 진행(변경된 파일)
        //        --> 기존에 있는 화일은 오버라이트 ? 아니면 삭제
        // 3. 

        if (!Directory.Exists(downloadFolder))
        {
            Logger.ErrorLog($"[SetupDownload] 다운로드 폴더가 존재하지 않습니다. {downloadFolder}");
            return (false, $"다운로드 폴더가 존재하지 않습니다. {downloadFolder}");
        }

        var isValidUrl = false;
        var inValidUrl = "";
        foreach (var file in files)
        {
            string url = $"{DEFAULT_FILE_SERVER_BASE_URL}/{file.filePath}";
            isValidUrl = await Utils.IsDownloadableAsync(url, apiKey);
            if (!isValidUrl)
            {
                inValidUrl = url;
                break;
            }
        }

        if (!isValidUrl)
        {
            Logger.ErrorLog($"[SetupDownload] 유효하지 않은 URL이 포함되어 있습니다. {inValidUrl}");
            return (false, $"유효하지 않은 URL이 포함되어 있습니다.");
        }

        downloadItems.Clear();
        panelDownloadList.Controls.Clear();

        foreach (var file in files)
        {
            string url = $"{DEFAULT_FILE_SERVER_BASE_URL}/{file.filePath}";
            if (downloadItems.Any(x => x.Url == url))
            {
                Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
                continue;
            }

            var downloadFilePath = Path.Combine(downloadFolder, Path.GetFileName(url));

            if (!file.updateStatus)  // 무조건 업데이트가 아니라면
            {
                var isChecksum = IsChecksum(downloadFilePath, file.checksum);
                Logger.Log($"[SetupDownload] {url} - {downloadFilePath} -> Checksum: {isChecksum}[{file.checksum}]");

                if (!isChecksum) // 체크섬이 맞지 않으면 새로 다운로드 받기 위해 삭제한다
                {
                    Utils.DeleteFile(downloadFilePath);
                }

                // 현재 존재하는 화일
                if (File.Exists(downloadFilePath))
                {
                    Logger.Log($"[SetupDownload] {url} - {downloadFilePath} -> Exist");
                    continue;
                }
            }

            // AddDownloadItem(url);
            FileDownloaderItem item = new FileDownloaderItem(url, downloadFolder);

            // 화일 크기 얻어서 셋팅 하는 부분이 필요
            // 화일 받다가 중지 된 경우 이어 받기 하기 위해 InitializeTotalBytesReceived()(TotalBytesReceived 셋팅) 실행이 되고
            //   TotalFileSize 는 셋팅이 안되서 UpdateOverallProgress() 에러가남 계산이 안맞음
            // Utils.GetFileSizeAsync();

            item.ProgressUpdated += UpdateOverallProgress;
            item.StatusUpdated += (sender, status) => Logger.Log($"{url} - {status}");

            downloadItems.Add(item);
            panelDownloadList.Controls.Add(item.UI.Panel);

            Logger.Log($"[SetupDownload] {url} - 다운로드 추가");
        }

        // // Initialize the overall progress bar
        // overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new Point(15, 45) };
        // this.Controls.Add(overallProgressBar);

        // btnUpdate.Text = "업데이트";
        // btnUpdate.Enabled = true;

        if (downloadItems.Count <= 0)
        {
            return (false, $"모두 업데이트 완료 됐습니다.");
        }

        return (true, "Success");
    }

    // private bool AddDownloadItem(string url)
    // {
    //     if (downloadItems.Any(x => x.Url == url))
    //     {
    //         // MessageBox.Show("이미 추가된 URL입니다.");
    //         Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
    //         return false;
    //     }

    //     FileDownloaderItem item = new FileDownloaderItem(url, downloadFolder);

    //     item.ProgressUpdated += UpdateOverallProgress;
    //     item.StatusUpdated += (sender, status) => Logger.Log($"{url} - {status}");

    //     downloadItems.Add(item);
    //     panelDownloadList.Controls.Add(item.UI.Panel);

    //     Logger.Log($"[AddDownloadItem] {url} - 다운로드 추가");

    //     return true;
    // }

    private bool IsChecksum(string downloadFilePath, string checksum)
    {
        // return Utils.CalculateMD5FromFile(Path.Combine(downloadFolder, Path.GetFileName(url))) == checksum;
        return Utils.CalculateMD5FromFile(downloadFilePath) == checksum;
    }

    private void StartDownload(FileDownloaderItem item)
    {
        string apiKey = DEFAULT_API_KEY;

        Task.Run(() => DownloadHelper.DownloadFileAsync(apiKey, item, UpdateUI));
    }

    private void UpdateOverallProgress()
    {
        long totalBytesReceived = downloadItems.Where(c => c.TotalFileSize > 0).Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Where(c => c.TotalFileSize > 0).Sum(item => item.TotalFileSize);

        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;

        // overallProgressBar.Maximum = 100;
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
            string url = DEFAULT_API_SERVER_BASE_URL + "/download-file/info";

            // case 1
            // string jsonContent = "{\"key1\":\"value\", \"key\":\"value2\"}";

            // case 2
            // Dictionary<string, object> jsonDict = new Dictionary<string, object>();
            // jsonDict.Add("key1", "items1");
            // jsonDict.Add("key2", "items2");

            // case 3
            // string jsonContent = JsonSerializer.Serialize(jsonDict);

            // case 4
            string jsonContent = JsonSerializer.Serialize(
                new
                {
                    key1 = "items1",
                    key2 = "items2"
                }
            );

            string apiKey = DEFAULT_API_KEY;

            // --- case 1
            // (bool result, string response) = await Utils.PostAsync(url, jsonContent, apiKey);
            // if (!result)
            // {
            //     Console.WriteLine($"Response from GET request: {response}");
            //     Logger.Log($"{response}");
            // }

            // // null 리터럴 또는 가능한 null 값을 null을 허용하지 않는 형식으로 변환하는 중입니다.
            // ResponseData<FilePathData> r = JsonSerializer.Deserialize<ResponseData<FilePathData>>(response);
            // if (!result)
            // {
            //     Console.WriteLine("Response is null");
            //     return;
            // }


            // foreach (var item in r.resultData)
            // {
            //     Console.WriteLine($" {item.filePath} - {item.checksum}");
            // }
            // ---

            // --- case 2
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

            // -- case 3
            (bool isResult, string resuleMessage, List<FilePathData> resultData) = await Utils.CallApi<List<FilePathData>>(url, jsonContent, apiKey);
            if (!isResult)
            {
                MessageBox.Show("서버에서 다운로드 정보를 가져올 수 없습니다.");
                Console.WriteLine("Response is false");
                return;
            }

            foreach (var item in resultData)
            {
                Console.WriteLine($" {item.filePath} - {item.checksum}");
                Logger.Log($" {item.filePath} - {item.checksum}");
            }

            // Logger.Log($"{response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

