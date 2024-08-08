namespace FileDownloader7;

using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using Accessibility;

#pragma warning disable CS8600
#pragma warning disable CS8602

public enum UpdateStatus
{
    Error = 0,   // 다운로드 오류
    // ListAdded,   // 다운로드 추가
    Downloading, // 다운로드 진행중
    Complete,   // 다운로드 완료
    AlreadyCompleted,  // 이미 다운로드 완료
}


public partial class FileDownloader : Form
{
    public const string DOWNLOAD_INFO_FILE = "newest.txt";
    public const string DOWNLOAD_LIST_LOCAL_SAVE_FILE = ".newest.json";

    public const string FILE_SERVER_BASE_URL = "http://localhost:3050";
    public const string API_SERVER_BASE_URL = "http://localhost:3050";

    public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
    public const string DEFAULT_API_KEY = "1ab2c3d4e5f61ab2c3d4e5f6";

    public const string PATCH_INFO_API_PATH = "patch/info";

    // public const string PATCH_FILE_DOWNLOAD_PATH = "patch/download__";
    public const string PATCH_FILE_DOWNLOAD_PATH = "patch";

    private List<FileDownloaderItem> downloadItems = new List<FileDownloaderItem>();
    private string downloadFolder = DEFAULT_DOWNLOAD_FOLDER;

    private TextProgressBar overallProgressBar = default!; // Overall progress bar

    // List<(Task, CancellationTokenSource)> taskList = new();
    private List<(Task, CancellationTokenSource)> taskList = default!;

    private bool isDownloadStart = false;
    private bool isDownloadComplete = false;


    // 0.1,text01.txt,0,cb08ca4a7bb5f9683c19133a84872ca7,0
    public class DownloadInfo
    {
        public const int DEFAULT_ELEMENT_COUNT = 5;

        public string version { get; set; } = string.Empty;
        public string fileName { get; set; } = string.Empty;
        public long fileSize { get; set; } = 0;
        public string checksum { get; set; } = string.Empty;
        public bool forceUpdate { get; set; } = false;

        // public static List<DownloadInfo> __parseDownloadInfoFileContent(string fileContent)
        // {
        //     string[] lines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //     if (lines.Length == 0)
        //     {
        //         return [];
        //     }

        //     var files = new List<DownloadInfo>();

        //     foreach (var str in lines)
        //     {
        //         var (version, file, fileSize, checksum, forseUpdate) = str.Split(',') switch
        //         {
        //             var s when s.Length == 5 => (s[0], s[1], long.Parse(s[2]), s[3], Utils.StringToBoolean(s[4])),
        //             _ => (string.Empty, string.Empty, 0, string.Empty, false)
        //         };

        //         files.Add(new DownloadInfo { version = version, fileName = file, fileSize = fileSize, checksum = checksum, forceUpdate = forseUpdate });
        //         // Logger.Log($"file:{file}, checksum:{checksum}, fileSize:{fileSize}, forseUpdate:{forseUpdate}");
        //     }

        //     return files;
        // }

        public static (bool, string, List<DownloadInfo>) parseDownloadInfoFileContent(string fileContent)
        {
            string[] lines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return (false, "Split Fail", []);
            }

            var files = new List<DownloadInfo>();

            bool isvalid = false;
            string message = "Success";

            int lineCount = 1;

            try
            {
                foreach (var line in lines)
                {
                    var splitString = line.Split(',');
                    if (splitString.Length != DownloadInfo.DEFAULT_ELEMENT_COUNT)
                    {
                        isvalid = false;
                        message = $"Invalid Argument. [line:{lineCount}]";
                        break;
                    }

                    var version = splitString[0];
                    if (String.IsNullOrEmpty(version))
                    {
                        isvalid = false;
                        message = $"Invalid Version. [line:{lineCount}]";
                        break;
                    }

                    var file = splitString[1];
                    if (String.IsNullOrEmpty(file))
                    {
                        isvalid = false;
                        message = $"Invalid File Name. [line:{lineCount}]";
                        break;
                    }

                    if (!long.TryParse(splitString[2], out long fileSize))
                    {
                        isvalid = false;
                        message = $"Invalid File Size. [line:{lineCount}]";
                        break;
                    }

                    var checksum = splitString[3];
                    if (String.IsNullOrEmpty(checksum))
                    {
                        isvalid = false;
                        message = $"Invalid Checksum. [line:{lineCount}]";
                        break;
                    }

                    var forceUpdate = Utils.StringToBoolean(splitString[4]);

                    files.Add(new DownloadInfo { version = version, fileName = file, fileSize = fileSize, checksum = checksum, forceUpdate = forceUpdate });
                    lineCount++;
                }
            }
            // catch (FormatException)
            // {
            //     isvalid = false;
            //     message = $"Invalid format for conversion. [line:{lineCount}]";
            //     Console.WriteLine(message);
            // }
            // catch (OverflowException)
            // {
            //     isvalid = false;
            //     message = $"The number is too large for a long. [line:{lineCount}]";
            //     Console.WriteLine(message);
            // }
            catch (Exception e)
            {
                isvalid = false;
                message = $"An exception occurred: {e.Message}. [line:{lineCount}]";
                Console.WriteLine(message);
            }

            return (isvalid, message, files);
        }
    }

    public FileDownloader()
    {
        InitializeComponent();

        Directory.CreateDirectory(downloadFolder);

        // Initialize the overall progress bar
        // overallProgressBar = new TextProgressBar { Width = 480, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new Point(15, 45) };
        // this.Controls.Add(overallProgressBar);
    }

    public static string GetDownloadListSaveFile()
    {
        return $"{DEFAULT_DOWNLOAD_FOLDER}/{DOWNLOAD_LIST_LOCAL_SAVE_FILE}";
    }

    public static string GetDownloadInfoFile()
    {
        return $"{FILE_SERVER_BASE_URL}/{DOWNLOAD_INFO_FILE}";
    }

    public static string GetDownloadFilePath(string fileName)
    {
        return $"{FILE_SERVER_BASE_URL}/{PATCH_FILE_DOWNLOAD_PATH}/{fileName}";
    }

    public static string GetApiServerUrl()
    {
        return $"{API_SERVER_BASE_URL}/{PATCH_INFO_API_PATH}";
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

        if (overallProgressBar == null)
        {
            overallProgressBar = new TextProgressBar
            {
                Width = 480,
                Height = 20,
                VisualMode = ProgressBarDisplayMode.Percentage,
                Location = new Point(15, 45),
                Maximum = 100,
                Value = 0
            };
        }

        overallProgressBar.Maximum = 100;
        overallProgressBar.Value = 0;

        this.Controls.Add(overallProgressBar);

        // this.ResumeLayout(false);

        btnDownload.Enabled = true;
        btnDownload.Text = BTN_DOWNLOAD_START_TEXT;
        btnDownload.Focus();
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
        // if (downloadItems.Count == 0 || string.IsNullOrEmpty(downloadFolder))
        // {
        //     MessageBox.Show("다운로드 폴더를 지정해주세요.");
        //     return;
        // }

        // // overallProgressBar.Maximum = 100;
        // // overallProgressBar.Value = 0;

        // // btnUpdate.Enabled = false;
        // // btnDownload.Enabled = false;
        // btnSettings.Enabled = false;

        // // btnDownload.Text = "다운로드 중단";

        // // List<Task> taskList = new List<Task>();

        // foreach (var item in downloadItems)
        // {
        //     var (task, cts) = StartDownload(item);

        //     taskList.Add(Tuple.Create(task, cts));
        // }

        // await Task.WhenAll(taskList.Select(t => t.Item1).ToArray());

        (int indexSwitch, btnDownload.Text) = Utils.ButtonTextSwitch(btnDownload.Text, BTN_DOWNLOAD_START_TEXT, BTN_DOWNLOAD_PAUSE_TEXT);

        if (isDownloadStart)
        {
            if (indexSwitch == 1)
            {
                DownloadHelper.PauseDownload();
            }
            else
            {
                DownloadHelper.ResumeDownload();
            }
        }
        else
        {
            DownloadProc();
        }
    }

    // private void FileDownloader_Load(object sender, EventArgs e)
    private void Loader(object sender, EventArgs e)
    {
        Logger.Log($"[Load] Start");
    }

    private async void DownloadProc()
    {
        string apiKey = DEFAULT_API_KEY;

        if (downloadItems.Count == 0 || string.IsNullOrEmpty(downloadFolder))
        {
            MessageBox.Show("다운로드 폴더를 지정해주세요.");
            return;
        }

        btnUpdate.Enabled = false;
        // btnDownload.Enabled = false;
        btnSettings.Enabled = false;

        taskList = downloadItems.Select(item => item.ExecuteTask(apiKey, UpdateUI)).ToList();
        // foreach (var item in downloadItems)
        // {
        //     // var (task, cts) = StartDownload(item);
        //     var (task, cts) = item.ExecuteTask(apiKey, UpdateUI);

        //     taskList.Add((task, cts));
        // }

        isDownloadStart = true;
        isDownloadComplete = false;

        await Task.WhenAll(taskList.Select(t => t.Item1).ToArray());

        isDownloadStart = false;

        btnUpdate.Enabled = true;
        btnSettings.Enabled = true;
        btnDownload.Enabled = false;

        Console.WriteLine("All tasks completed.");
        Logger.Log($"[FileDownloader] All tasks completed.");

        MessageBox.Show("다운로드가 완료 됐습니다.");
    }

    // private void InitDownload()
    // {
    //     // overallProgressBar.Maximum = 100;
    //     // overallProgressBar.Value = 0;

    //     btnUpdate.Enabled = false;
    //     // btnDownload.Enabled = false;
    //     btnSettings.Enabled = false;

    //     taskList = downloadItems.Select(item => item.ExecuteTask(DEFAULT_API_KEY, UpdateUI)).ToList();
    //     // foreach (var item in downloadItems)
    //     // {
    //     //     var (task, cts) = StartDownload(item);

    //     //     taskList.Add((task, cts));
    //     // }

    //     isDownloadStart = true;
    // }

    // private void EndDownload()
    // {
    //     isDownloadStart = false;

    //     btnUpdate.Enabled = true;
    //     btnSettings.Enabled = true;
    //     btnDownload.Enabled = false;
    // }

    private async Task<(bool, string)> SetupDownload()
    {
        string apiKey = DEFAULT_API_KEY;

        var files = new List<DownloadInfo>
        {
            new DownloadInfo { version = "0.1", fileName = "text01.txt", fileSize = 4, checksum = "cb08ca4a7bb5f9683c19133a84872ca7", forceUpdate = true },
            new DownloadInfo { version = "0.1", fileName = "text02.txt", fileSize = 48, checksum = "f38c26a09c89158123f77b474221cc8a", forceUpdate = true },
            new DownloadInfo { version = "0.1", fileName = "text03.txt", fileSize = 334564, checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", forceUpdate = true },
            // new DownloadInfo { version = "0.1", fileName = "text04.txt", fileSize = 334564, checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", forceUpdate = true },
            new DownloadInfo { version = "0.1", fileName = "image_11.bin", fileSize = 675888392, checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", forceUpdate = true },
            new DownloadInfo { version = "0.1", fileName = "image_09.bin", fileSize = 80876324, checksum = "30eb7e71f05abc3a12ce3fcd589debd6", forceUpdate = true },
            new DownloadInfo { version = "0.1", fileName = "image_12.bin", fileSize = 675888392, checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", forceUpdate = true },



            // new DownloadInfo { filePath = "image-15/image_15.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", forceUpdate = true },
            // new DownloadInfo { filePath = "image-12/image_12.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15" },
            // new DownloadInfo { filePath = "image-11/image_11.bin", checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15" },
            // new DownloadInfo { filePath = "image-10/image_10.bin", checksum = "cb3fffcc0e7c5b2874c639a4107b3a6a" },
            // new DownloadInfo { fileName = "image-09/image_09.bin", checksum = "30eb7e71f05abc3a12ce3fcd589debd6", forceUpdate = true },
            // new DownloadInfo { filePath = "image-08/image_08.bin", checksum = "38db288725fa54ccbf0b92a39e69b78a" },
            // new DownloadInfo { filePath = "image-07/image_07.bin", checksum = "15d24a1d77ccd2f3983a09dec2374004" },
            // new DownloadInfo { filePath = "image-06/image_06.bin", checksum = "1f7e5a19cb4ace806a37cd72f3cb6172" },                        
        };

        var saveDownloadInfo = DownloadHelper.ReadFromJsonFile<List<DownloadInfo>>(FileDownloader.GetDownloadListSaveFile());
        if (saveDownloadInfo.Count > 0)
        {
            Console.WriteLine(Utils.ToJsonString(saveDownloadInfo));
            Logger.Log($"[SetupDownload] SavedDownloadInfo: {Utils.ToJsonString(saveDownloadInfo)}");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        var urls = new List<string>();

        // string apiUrl = $"{API_SERVER_BASE_URL}/{PATCH_INFO_API_PATH}";
        string apiUrl = FileDownloader.GetApiServerUrl();
        string jsonContent = JsonSerializer.Serialize(
            new
            {
                key1 = "items1",
                key2 = "items2"
            }
        );


        (bool isResult, string message, List<DownloadInfo> files__) = await Utils.CallApi<List<DownloadInfo>>(apiUrl, jsonContent, apiKey);
        if (!isResult || files__.Count == 0)
        {
            // MessageBox.Show("서버에서 다운로드 정보를 가져올 수 없습니다.");            
            return (false, $"서버에서 다운로드 정보를 가져올 수 없습니다. {downloadFolder}");
        }
        //////////////////////////////////////////////////////////////////////////////////////////////////////////// 
        ///
        DownloadHelper.WriteToJsonFile<List<DownloadInfo>>(FileDownloader.GetDownloadListSaveFile(), files);
        ///

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
            // string url = $"{FILE_SERVER_BASE_URL}/{PATCH_FILE_DOWNLOAD_PATH}/{file.fileName}";
            string url = FileDownloader.GetDownloadFilePath(file.fileName);

            isValidUrl = await DownloadHelper.IsDownloadableAsync(url, apiKey);
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
            // string url = $"{FILE_SERVER_BASE_URL}/{PATCH_FILE_DOWNLOAD_PATH}/{file.fileName}";
            string url = FileDownloader.GetDownloadFilePath(file.fileName);
            if (downloadItems.Any(x => x.Url == url))
            {
                Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
                continue;
            }

            var downloadFilePath = Path.Combine(downloadFolder, Path.GetFileName(url));

            if (!file.forceUpdate)  // 무조건 업데이트가 아니라면
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

            item.InitializeTotalBytesReceived();
            item.InitializeTotalFileSize(apiKey);

            // item.TotalFileSize = file.fileSize;

            // 화일 크기 얻어서 셋팅 하는 부분이 필요
            // 화일 받다가 중지 된 경우 이어 받기 하기 위해 InitializeTotalBytesReceived()(TotalBytesReceived 셋팅) 실행이 되고
            //   TotalFileSize 는 셋팅이 안되서 UpdateOverallProgress() 에러가남 계산이 안맞음
            // _ = DownloadHelper.GetFileSizeAsync(url, apiKey).ContinueWith(task =>
            // {
            //     item.TotalFileSize = task.Result;
            // });

            item.ProgressUpdated += UpdateOverallProgress;
            item.StatusUpdated += (sender, status) => Logger.Log($"[StatusUpdated][{url}] {status}");

            downloadItems.Add(item);
            panelDownloadList.Controls.Add(item.UI.Panel);

            Logger.Log($"[SetupDownload][{url}] 다운로드 추가");
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

    private (Task, CancellationTokenSource) StartDownload(FileDownloaderItem item)
    {
        string apiKey = DEFAULT_API_KEY;
        CancellationTokenSource cts = new();

        var task = Task.Run(() => DownloadHelper.DownloadFileAsync(apiKey, item, UpdateUI, cts));
        return (task, cts);
    }

    private void UpdateOverallProgress()
    {
        // long totalBytesReceived = downloadItems.Where(item => item.TotalBytesReceived < item.TotalFileSize).Sum(item => item.TotalBytesReceived);
        // long totalFileSize = downloadItems.Where(item => item.TotalBytesReceived < item.TotalFileSize).Sum(item => item.TotalFileSize);

        long totalBytesReceived = downloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = downloadItems.Sum(item => item.TotalFileSize);

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
        string downloadUrl = $"{FILE_SERVER_BASE_URL}/{DOWNLOAD_INFO_FILE}"; // 5a1000e8775ccb62e436468adcbfb243
        string apiKey = DEFAULT_API_KEY;
        // // string downloadUrl = "http://localhost:3030/image-12/image_12.bin"; // a34ee55dbb3aa4ac993eb7454b1f4d15

        // string s = Utils.CalculateMD5FromFile("C:\\downloadedFiles\\image_10.bin");

        (bool result, string content) = await DownloadHelper.GetFileContentAsync(downloadUrl, apiKey);

        // string s2 = Utils.CalculateMD5FromString(content);

        // // 다운로드한 콘텐츠를 텍스트 박스에 표시        
        string[] lines = content.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var (file, checksum, fileSize, forseUpdate) = line.Split(',') switch
            {
                var s when s.Length == 4 => (s[0], s[1], long.Parse(s[2]), Utils.StringToBoolean(s[3])), // bool.Parse(s[3])),
                _ => (string.Empty, string.Empty, 0, false)
            };

            Logger.Log($"file:{file}, checksum:{checksum}, fileSize:{fileSize}, forseUpdate:{forseUpdate}");
        }

        var lists = DownloadInfo.parseDownloadInfoFileContent(content);

        // DownloadHelper.WriteToJsonFile<List<DownloadInfo>>(FileDownloader.GetDownloadListSaveFile(), lists.Item3);

        // var myString = "text01.txt,cb08ca4a7bb5f9683c19133a84872ca7,4,0";
        // var (file, checksum, fileSize, forseUpdate) = myString.Split(',') switch { var a => (a[0], a[1], a[2], a[3]) };

        try
        {
            // string urlApiServer = API_SERVER_BASE_URL + "/" + PATCH_INFO_API_PATH;
            string urlApiServer = FileDownloader.GetApiServerUrl();

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

            // --- case 1
            // (bool result, string response) = await Utils.PostAsync(url, jsonContent, apiKey);
            // if (!result)
            // {
            //     Console.WriteLine($"Response from GET request: {response}");
            //     Logger.Log($"{response}");
            // }

            // // null 리터럴 또는 가능한 null 값을 null을 허용하지 않는 형식으로 변환하는 중입니다.
            // ResponseData<DownloadInfo> r = JsonSerializer.Deserialize<ResponseData<DownloadInfo>>(response);
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
            // (bool is__, string message__, List<DownloadInfo> resultData) = await Utils.CallApi<DownloadInfo>(url, jsonContent, apiKey);

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

            // -------------
            // -- case 3 ---
            // -------------
            // (bool isResult, string resuleMessage, List<DownloadInfo> resultData) = await Utils.CallApi<List<DownloadInfo>>(urlApiServer, jsonContent, apiKey);
            // if (!isResult)
            // {
            //     MessageBox.Show("서버에서 다운로드 정보를 가져올 수 없습니다.");
            //     Console.WriteLine("Response is false");
            //     return;
            // }

            // foreach (var item in resultData)
            // {
            //     string urlDownloadServer = $"{FILE_SERVER_BASE_URL}/{PATCH_FILE_DOWNLOAD_PATH}/{item.fileName}";
            //     bool isValidUrl = await DownloadHelper.IsDownloadableAsync(urlDownloadServer, apiKey);

            //     // string urlDownloadServer__ = $"{FILE_SERVER_BASE_URL}/patch/{item.fileName}";
            //     long TotalFileSize = await DownloadHelper.GetFileSizeAsync(urlDownloadServer, apiKey);

            //     // Console.WriteLine($"{item.version},  {item.fileName}, {item.checksum}, {item.forceUpdate}");
            //     Logger.Log($"version: {item.version},  fileName: {item.fileName}, checksum: {item.checksum}, forceUpdate: {item.forceUpdate}, isdownload: {isValidUrl}");
            // }


            // ----            
            // var f__ = new List<DownloadInfo>();

            // f__.Add(new DownloadInfo
            // {
            //     version = "0.1",
            //     fileName = "a.txt",
            //     fileSize = 123,
            //     checksum = "abc",
            //     forceUpdate = true
            // });

            // f__.Add(new DownloadInfo
            // {
            //     version = "0.1",
            //     fileName = "b.txt",
            //     fileSize = 123,
            //     checksum = "abc",
            //     forceUpdate = true
            // });

            // DownloadHelper.WriteToJsonFile<List<DownloadInfo>>(DEFAULT_DOWNLOAD_FOLDER + "/.myObjects.json", f__);

            // var v__ = DownloadHelper.ReadFromJsonFile<List<DownloadInfo>>(FileDownloader.GetDownloadListSaveFile());

            // StackTrace st = new StackTrace(new StackFrame(true));

            // Console.WriteLine(" Stack trace for current level: {0}", st.ToString());

            // StackFrame sf = st.GetFrame(0);
            // Console.WriteLine(" File: {0}", sf.GetFileName());
            // Console.WriteLine(" Method: {0}", sf.GetMethod().Name);
            // Console.WriteLine(" Line Number: {0}", sf.GetFileLineNumber());
            // Console.WriteLine(" Column Number: {0}", sf.GetFileColumnNumber());

            // FunctionC();

            // taskList.Select(t => t.Item1).ToArray();

            // taskList.ForEach(t => t.Item2.Cancel());

            // taskList.ForEach(t => t.Item1.Pause());

            // DownloadHelper.CancelDownload();
            DownloadHelper.CancelDownloads(taskList.Select(t => t.Item2).ToArray());

            // if (isSwitch)
            // {
            //     DownloadHelper.PauseDownload();
            // }
            // else
            // {
            //     DownloadHelper.ResumeDownload();
            // }

            // (int indexSwitch, btnTest.Text) = Utils.ButtonTextSwitch(btnTest.Text, "테스트", "테스트2");
            // // btnTest.Text = textSwitch;

            // Logger.Log($"=== {indexSwitch} -> {btnTest.Text}");

            Logger.Log($"===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void FunctionC()
    {
        Console.WriteLine("FunctionC start...\n");

        // pdb 파일을 참조하는 StackTrace 인스턴스
        StackTrace st = new StackTrace(true);

        Logger.Log($"    프레임 수: " + st.FrameCount);
        Logger.Log($"현  재 메서드: " + st.GetFrame(0).GetMethod().Name);
        Logger.Log($"호출한 메서드: " + st.GetFrame(1).GetMethod().Name);
        Logger.Log($"진  입 메서드: " + st.GetFrame(st.FrameCount - 1).GetMethod().Name);
        // Console.WriteLine();

        Console.WriteLine("-- 호출 스택 --");
        foreach (StackFrame sf in st.GetFrames())
        {
            Logger.Log($"  파  일: " + sf.GetFileName());
            Logger.Log($"      행: " + sf.GetFileLineNumber());
            Logger.Log($"      열: " + sf.GetFileColumnNumber());
            Logger.Log($"  오프셋: " + sf.GetILOffset());
            Logger.Log($"  메서드: " + sf.GetMethod().Name);
        }
        // Console.WriteLine();

        Logger.Log($"FunctionC end...");
    }


}

