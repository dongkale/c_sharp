namespace FileDownloader7;

using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using Accessibility;
using Amazon;
using Amazon.S3;
using Microsoft.Win32;

#pragma warning disable CS8600
#pragma warning disable CS8602

public enum UpdateStatus
{
    Error = 0,   // 다운로드 오류    
    Downloading, // 다운로드 진행중
    Complete,   // 다운로드 완료
    AlreadyCompleted,  // 이미 다운로드 완료
}

public partial class FileDownloader : Form
{
    public const string DOWNLOAD_INFO_FILE = "newest.txt";
    public const string DOWNLOAD_LIST_LOCAL_SAVE_FILE = ".newest.json";
    // public const string DOWNLOAD_COMPLETE_LOCAL_SAVE_FILE = ".newest.complete.json";    
    public const string DOWNLOAD_FOLDER_REGISTRY_KEY = "DownloadFolder";

    public const string FILE_SERVER_BASE_URL = "http://localhost:3050";
    public const string API_SERVER_BASE_URL = "http://localhost:3050";

    public const string DEFAULT_DOWNLOAD_FOLDER = "C:\\downloadedFiles";
    public const string DEFAULT_API_KEY = "1ab2c3d4e5f61ab2c3d4e5f6";

    public const string PATCH_INFO_API_PATH = "patch/info";

    // public const string PATCH_FILE_DOWNLOAD_PATH = "patch/download__";
    public const string PATCH_FILE_DOWNLOAD_PATH = "patch";

    // aws 
    public const string AWS_S3_ACCESS_KEY_ID = "__";
    public const string AWS_S3_SECRET_ACCESS_KEY = "__";
    public const string AWS_S3_BUCKET_NAME = "__";
    AmazonS3Client s3Client = new AmazonS3Client(AWS_S3_ACCESS_KEY_ID, AWS_S3_SECRET_ACCESS_KEY, RegionEndpoint.APNortheast2);
    // aws 

    private List<FileDownloaderItem> DownloadItems = new List<FileDownloaderItem>();
    private string DownloadFolder = DEFAULT_DOWNLOAD_FOLDER;

    private TextProgressBar OverallProgressBar = default!;

    private List<(Task, CancellationTokenSource)> TaskList = default!;

    private bool IsDownloadStart = false;
    // private bool isDownloadComplete = false;
    private DateTime DownloadStartDate = default!;


    public class DownloadInfo
    {
        public string Version { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; } = 0;
        public string Checksum { get; set; } = string.Empty;
        public bool ForceUpdate { get; set; } = false;

        public static int GetPropertiesCount()
        {
            return typeof(DownloadInfo).GetProperties().Length;
        }

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

        // var saveDownloadInfo = Utils.ReadFromJsonFile<List<DownloadInfo>>(GetDownloadListSaveFile());

        // public static (bool, string, List<DownloadInfo>) LoadDownloadInfo(GetDownloadListSaveFile())
        // {
        //     return Utils.ReadFromJsonFile<List<DownloadInfo>>();
        // }

        public static (bool, string, List<DownloadInfo>) ParseDownloadInfoFileContent(string fileContent)
        {
            string[] lines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return (false, "Split Fail", []);
            }

            var files = new List<DownloadInfo>();

            bool isvalid = true;
            string message = "Success";

            int lineCount = 1;

            try
            {
                foreach (var line in lines)
                {
                    var splitString = line.Split(',');
                    if (splitString.Length != DownloadInfo.GetPropertiesCount())
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

                    files.Add(new DownloadInfo { Version = version, FileName = file, FileSize = fileSize, Checksum = checksum, ForceUpdate = forceUpdate });
                    lineCount++;
                }
            }
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
        this.InitializeComponent();

        this.InitializeSetting();
    }

    public void InitializeSetting()
    {
        var downloadFolder = Utils.GetRegistryKey(Process.GetCurrentProcess().ProcessName, DOWNLOAD_FOLDER_REGISTRY_KEY);
        if (String.IsNullOrEmpty(downloadFolder))
        {
            this.DownloadFolder = DEFAULT_DOWNLOAD_FOLDER;
            Utils.SetRegistryKey(Process.GetCurrentProcess().ProcessName, DOWNLOAD_FOLDER_REGISTRY_KEY, this.DownloadFolder);
        }
        else
        {
            this.DownloadFolder = downloadFolder;
        }

        Directory.CreateDirectory(this.DownloadFolder);
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

    public string GetDownloadListSaveFile()
    {
        return $"{DownloadFolder}/{DOWNLOAD_LIST_LOCAL_SAVE_FILE}";
    }

    private async void __old_btnUpdate_Click(object sender, EventArgs e)
    {
        this.PrintMessage("업데이트 정보 얻어오는중 입니다.");

        btnUpdate.Text = BTN_UPDATE_CHECK_TEXT;
        btnUpdate.Enabled = false;

        (bool isResult, string message) = await this.SetupDownload();

        btnUpdate.Text = BTN_UPDATE_START_TEXT;
        btnUpdate.Enabled = true;

        this.PrintMessage("다운로드를 시작할 수 있습니다.");

        if (!isResult)
        {
            // MessageBox.Show(message);
            PrintMessage(message);
            return;
        }

        if (this.OverallProgressBar == null)
        {
            this.OverallProgressBar = new TextProgressBar
            {
                Width = 500,
                Height = 18,
                VisualMode = ProgressBarDisplayMode.Percentage,
                // Location = new Point(15, 45),
                Location = new Point(15, 75),
                // Maximum = 100,
                // Value = 0
                // LastValue = 0
            };
        }

        this.OverallProgressBar.Maximum = 100;
        this.OverallProgressBar.Value = 0;
        this.OverallProgressBar.LastValue = 0;

        this.Controls.Add(this.OverallProgressBar);

        this.btnDownload.Enabled = true;
        this.btnDownload.Text = BTN_DOWNLOAD_START_TEXT;
        this.btnDownload.Focus();
    }

    private async void btnUpdate_Click(object sender, EventArgs e)
    {
        this.PrintMessage("업데이트 정보 얻어오는중 입니다.");

        this.btnUpdate.Text = BTN_UPDATE_CHECK_TEXT;
        this.btnUpdate.Enabled = false;

        this.Controls.Remove(OverallProgressBar);

        (bool isResult, string message) = await this.SetupDownload();

        this.btnUpdate.Text = BTN_UPDATE_START_TEXT;
        this.btnUpdate.Enabled = true;

        if (!isResult)
        {
            // MessageBox.Show(message);
            this.PrintMessage(message);
            return;
        }

        this.OverallProgressBar = new TextProgressBar
        {
            Name = "OverallProgressBar",
            Width = 500,
            Height = 18,
            VisualMode = ProgressBarDisplayMode.Percentage,
            Location = new Point(15, 75),
            Maximum = 100,
            Value = 0,
            LastValue = 0
        };

        this.Controls.Add(OverallProgressBar);

        this.btnDownload.Enabled = true;
        this.btnDownload.Text = BTN_DOWNLOAD_START_TEXT;
        this.btnDownload.Focus();

        this.PrintMessage("다운로드를 시작할 수 있습니다.");
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
        {
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                this.DownloadFolder = folderDialog.SelectedPath;

                Utils.SetRegistryKey(Process.GetCurrentProcess().ProcessName, DOWNLOAD_FOLDER_REGISTRY_KEY, this.DownloadFolder);
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

        // // List<Task> TaskList = new List<Task>();

        // foreach (var item in downloadItems)
        // {
        //     var (task, cts) = StartDownload(item);

        //     TaskList.Add(Tuple.Create(task, cts));
        // }

        // await Task.WhenAll(taskList.Select(t => t.Item1).ToArray());

        (int indexSwitch, btnDownload.Text) = Utils.ButtonTextSwitch(btnDownload.Text, BTN_DOWNLOAD_START_TEXT, BTN_DOWNLOAD_PAUSE_TEXT);

        if (this.IsDownloadStart)
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
            this.DownloadProc();
        }
    }

    private void Loader(object sender, EventArgs e)
    {
        Logger.Log($"[Load] Start");
    }

    private async void DownloadProc()
    {
        string apiKey = DEFAULT_API_KEY;

        if (this.DownloadItems.Count == 0 || string.IsNullOrEmpty(this.DownloadFolder))
        {
            MessageBox.Show("다운로드 폴더를 지정해주세요.");
            return;
        }

        this.btnUpdate.Enabled = false;
        // btnDownload.Enabled = false; // 다운로드 중단/진행 버튼 표시을 위해 활성화 한다
        this.btnSettings.Enabled = false;

        this.IsDownloadStart = true;
        this.DownloadStartDate = DateTime.Now;

        // this.TaskList = this.DownloadItems.Select(item => item.ExecuteTask(apiKey, UpdateUI)).ToList();
        // this.TaskList = this.DownloadItems.Select(item => item.ExecuteTask(s3Client, AWS_S3_BUCKET_NAME, UpdateUI)).ToList();
        this.TaskList = this.DownloadItems.Select(item => item.ExecuteTask(UpdateUI)).ToList();
        // foreach (var item in DownloadItems)
        // {
        //     // var (task, cts) = StartDownload(item);
        //     var (task, cts) = item.ExecuteTask(apiKey, UpdateUI);

        //     this.TaskList.Add((task, cts));
        // }        

        await Task.WhenAll(this.TaskList.Select(t => t.Item1).ToArray());

        var countComplete = this.DownloadItems.Sum(item => item.IsComplete ? 1 : 0);

        var elapsedTimeString = Utils.FormatTimeSpan(DateTime.Now - this.DownloadStartDate);

        this.IsDownloadStart = false;

        this.btnUpdate.Enabled = true;
        this.btnSettings.Enabled = true;
        this.btnDownload.Enabled = false;

        Logger.Log($"[FileDownloader] All tasks completed.(elapsedTime: {elapsedTimeString}), complete:({countComplete})");

        PrintMessage($"다운로드가 완료 됐습니다.(소요시간: {elapsedTimeString})");
    }

    private async Task<(bool, string)> SetupDownload()
    {
        string apiKey = DEFAULT_API_KEY;

        // 기존 저장 되어 있는 다운로드 정보 읽어오기, 없으면 그냥 skip...
        var saveDownloadInfo = Utils.ReadFromJsonFile<List<DownloadInfo>>(GetDownloadListSaveFile());
        if (saveDownloadInfo.Count > 0)
        {
            Console.WriteLine(Utils.ToJsonString(saveDownloadInfo));
            Logger.Log($"[SetupDownload] SavedDownloadInfo: {Utils.ToJsonString(saveDownloadInfo)}");
        }

        // ---------
        // 테스트용으로 로컬에서 정보를 가져온다
        (bool isResult1, string message1, List<DownloadInfo> files) = this.__GetDownloadInfoFromLocalFile(apiKey);
        if (!isResult1)
        {
            return (false, message1);
        }
        // ---------

        Logger.Log($"[SetupDownload] LoaddDownloadInfo(Local): {Utils.ToJsonString(files)}");

        (bool isResult2, string message2, List<DownloadInfo> files__2) = await this.GetDownloadInfoFromPatchServer(apiKey);
        if (!isResult2)
        {
            return (false, message2);
        }

        Logger.Log($"[SetupDownload] LoaddDownloadInfo(Server): {Utils.ToJsonString(files__2)}");

        (bool isResult3, string message3, List<DownloadInfo> files__3) = await this.GetDownloadInfoFromPatchFile(apiKey);
        if (!isResult3)
        {
            return (false, message3);
        }

        Logger.Log($"[SetupDownload] LoaddDownloadInfo(File): {Utils.ToJsonString(files__3)}");
        // ---------

        // 다운로드 정보를 저장, 다운로드가 완료되면 저장 ??????
        Utils.WriteToJsonFile<List<DownloadInfo>>(this.GetDownloadListSaveFile(), files);

        if (!Directory.Exists(this.DownloadFolder))
        {
            Logger.ErrorLog($"[SetupDownload] 다운로드 폴더가 존재하지 않습니다. {this.DownloadFolder}");
            return (false, $"다운로드 폴더가 존재하지 않습니다. {this.DownloadFolder}");
        }

        var isValidUrl = false;
        var inValidUrl = "";
        foreach (var file in files)
        {
            string url = FileDownloader.GetDownloadFilePath(file.FileName);

            // isValidUrl = await DownloadHelper.IsDownloadableAsync_S3(s3Client, AWS_S3_BUCKET_NAME, Utils.ExtractFilePathFromUrl(url));
            isValidUrl = await DownloadHelper.IsDownloadableAsync(apiKey, url);
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

        this.DownloadItems.Clear();
        this.panelDownloadList.Controls.Clear();

        foreach (var file in files)
        {
            string url = FileDownloader.GetDownloadFilePath(file.FileName);
            if (this.DownloadItems.Any(x => x.Url == url))
            {
                Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
                continue;
            }

            // var downloadFilePath = Path.Combine(DownloadFolder, Path.GetFileName(url));
            var downloadFilePath = Path.Combine(this.DownloadFolder, file.FileName);

            if (!file.ForceUpdate)  // 무조건 업데이트가 아니라면
            {
                // 화일이 없을 경우... 무조건 다운로드 한다
                // 화일이 있을 경우... 체크섬이 맞지 않으면 다운로드 받는다
                if (File.Exists(downloadFilePath))
                {
                    var isChecksum = IsChecksum(downloadFilePath, file.Checksum);
                    Logger.Log($"[SetupDownload] {url} - {downloadFilePath} -> Checksum: {isChecksum}[{file.Checksum}]");

                    if (isChecksum)
                    {
                        Logger.Log($"[SetupDownload] {url} - {downloadFilePath} -> Exist");
                        continue;
                    }
                    else  // 체크섬이 맞지 않으면 새로 다운로드 받기 위해 삭제한다
                    {
                        Utils.DeleteFile(downloadFilePath);

                        // DownloadHelper.DeleteAllPartFiles__(DownloadFolder, file.FileName);   // *part* 삭제
                        DownloadHelper.DeleteAllPartFiles(downloadFilePath);   // *part* 삭제
                    }

                    // // 현재 존재하는 화일은 skip
                    // if (File.Exists(downloadFilePath))
                    // {
                    //     Logger.Log($"[SetupDownload] {url} - {downloadFilePath} -> Exist");
                    //     continue;
                    // }
                }
            }

            // FileDownloaderItem item = new FileDownloaderItem(url, this.DownloadFolder, ServerType.AwsS3);
            FileDownloaderItem item = new FileDownloaderItem(url, this.DownloadFolder, ServerType.FileServer);

            // item.SetS3(s3Client, AWS_S3_BUCKET_NAME);
            item.SetFileServer(apiKey);

            item.InitializeTotalBytesReceived();
            if (!item.InitializeTotalFileSize())
            {
                Logger.ErrorLog($"[SetupDownload][{url}] invlaid file size");
                continue;
            }

            // item.InitializeTotalFileSize(apiKey);
            // item.InitializeTotalFileSize(s3Client, AWS_S3_BUCKET_NAME);

            item.ProgressUpdated += UpdateOverallProgress;
            item.StatusUpdated += (sender, status) => Logger.Log($"[StatusUpdated][{url}] {status}");

            this.DownloadItems.Add(item);
            this.panelDownloadList.Controls.Add(item.UI.Panel);

            Logger.Log($"[SetupDownload][{url}] 다운로드 추가");
        }

        if (this.DownloadItems.Count <= 0)
        {
            return (false, "모두 업데이트 완료 됐습니다.");
        }

        return (true, "Success");
    }

    private void PrintMessage(string message)
    {
        this.labelStatus.Text = message;
    }

    private async Task<(bool, string, List<DownloadInfo>)> GetDownloadInfoFromPatchServer(string apiKey)
    {
        // string apiUrl = $"{API_SERVER_BASE_URL}/{PATCH_INFO_API_PATH}";
        string apiUrl = FileDownloader.GetApiServerUrl();
        string jsonContent = JsonSerializer.Serialize(
            new
            {
                key1 = "items1",
                key2 = "items2"
            }
        );

        (bool isResult, string message, List<DownloadInfo> downloadInfo) = await Utils.CallApi<List<DownloadInfo>>(apiUrl, jsonContent, apiKey);
        if (!isResult || downloadInfo.Count == 0)
        {
            Logger.ErrorLog($"[GetDownloadInfoFromPatchServer] {message}");
            return (false, $"서버에서 다운로드 정보를 가져올 수 없습니다. {apiUrl}", []);
        }

        return (true, "Success", downloadInfo);
    }

    private async Task<(bool, string, List<DownloadInfo>)> GetDownloadInfoFromPatchFile(string apiKey)
    {
        string downloadUrl = FileDownloader.GetDownloadInfoFile();

        (bool isResult, string content) = await DownloadHelper.GetFileContentAsync(apiKey, downloadUrl);
        if (!isResult || String.IsNullOrEmpty(content))
        {
            Logger.ErrorLog($"[GetDownloadInfoFromPatchFile] {content}");
            return (false, $"서버에서 다운로드 정보를 가져올 수 없습니다. {downloadUrl}", []);
        }

        return DownloadInfo.ParseDownloadInfoFileContent(content);
    }

    private (bool, string, List<DownloadInfo>) __GetDownloadInfoFromLocalFile(string apiKey)
    {
        var downloadInfo = new List<DownloadInfo>
        {
            new() { Version = "0.1", FileName = "text01.txt", FileSize = 4, Checksum = "cb08ca4a7bb5f9683c19133a84872ca7", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text02.txt", FileSize = 48, Checksum = "f38c26a09c89158123f77b474221cc8a", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text03.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text04.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text05.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text06.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            new() { Version = "0.1", FileName = "text07.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            // new() { Version = "0.1", FileName = "text08.txt", FileSize = 334564, Checksum = "cdd50a3cc4c11350b4f7a97b9c83b569", ForceUpdate = true },
            // new() { Version = "0.1", FileName = "image_09.bin", FileSize = 80876324,  Checksum = "30eb7e71f05abc3a12ce3fcd589debd6", ForceUpdate = true },
            // new() { Version = "0.1", FileName = "image_11.bin", FileSize = 675888392, Checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", ForceUpdate = true },
            // new()  { Version = "0.1", FileName = "image_12.bin", FileSize = 675888392, Checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", ForceUpdate = false },
            // new() { Version = "0.1", FileName = "image_13.bin", FileSize = 675888392, Checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", ForceUpdate = false },
            // new() { Version = "0.1", FileName = "image_14.bin", FileSize = 675888392, Checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", ForceUpdate = false },
            // new() { Version = "0.1", FileName = "image_15.bin", FileSize = 675888392, Checksum = "a34ee55dbb3aa4ac993eb7454b1f4d15", ForceUpdate = false },
        };

        return (true, "Success", downloadInfo);
    }

    // private bool AddDownloadItem(string url)
    // {
    //     if (DownloadItems.Any(x => x.Url == url))
    //     {
    //         // MessageBox.Show("이미 추가된 URL입니다.");
    //         Logger.ErrorLog($"{url} - 이미 추가된 URL입니다.");
    //         return false;
    //     }

    //     FileDownloaderItem item = new FileDownloaderItem(url, downloadFolder);

    //     item.ProgressUpdated += UpdateOverallProgress;
    //     item.StatusUpdated += (sender, status) => Logger.Log($"{url} - {status}");

    //     DownloadItems.Add(item);
    //     panelDownloadList.Controls.Add(item.UI.Panel);

    //     Logger.Log($"[AddDownloadItem] {url} - 다운로드 추가");

    //     return true;
    // }

    private bool IsChecksum(string downloadFilePath, string checksum)
    {
        // return Utils.CalculateMD5FromFile(Path.Combine(downloadFolder, Path.GetFileName(url))) == checksum;
        return Utils.CalculateMD5FromFile(downloadFilePath) == checksum;
    }

    private void UpdateOverallProgress()
    {
        // long totalBytesReceived = DownloadItems.Where(item => item.TotalBytesReceived < item.TotalFileSize).Sum(item => item.TotalBytesReceived);
        // long totalFileSize = DownloadItems.Where(item => item.TotalBytesReceived < item.TotalFileSize).Sum(item => item.TotalFileSize);

        long totalBytesReceived = this.DownloadItems.Sum(item => item.TotalBytesReceived);
        long totalFileSize = this.DownloadItems.Sum(item => item.TotalFileSize);

        int percentage = totalFileSize > 0 ? (int)((double)totalBytesReceived / totalFileSize * 100) : 0;

        // overallProgressBar.Maximum = 100;
        // overallProgressBar.Value = percentage;

        if (percentage != this.OverallProgressBar.Value)
        {
            // OverallProgressBar.Value = percentage;
            // OverallProgressBar.LastValue = percentage;

            // OverallProgressBar.Invoke(new Action(() => { OverallProgressBar.Value = percentage; OverallProgressBar.LastValue = percentage; }));
            this.OverallProgressBar.Invoke(new Action(() => this.OverallProgressBar.Value = percentage));
        }

        // if (OverallProgressBar.InvokeRequired)
        // {
        //     OverallProgressBar.Invoke(new Action(() => OverallProgressBar.Value = percentage));
        // }
        // else
        // {
        //     OverallProgressBar.Value = percentage;
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

    // ------------------------------------------------------------------------------------------------------------------------

    private async void btnTest_Click(object sender, EventArgs e)
    {
        // var props = typeof(DownloadInfo).GetProperties().Length;
        // var v = props.Count();

        // string downloadUrl = $"{FILE_SERVER_BASE_URL}/{DOWNLOAD_INFO_FILE}"; // 5a1000e8775ccb62e436468adcbfb243
        string downloadUrl = FileDownloader.GetDownloadInfoFile();
        string apiKey = DEFAULT_API_KEY;
        // // string downloadUrl = "http://localhost:3030/image-12/image_12.bin"; // a34ee55dbb3aa4ac993eb7454b1f4d15

        // string s = Utils.CalculateMD5FromFile("C:\\downloadedFiles\\image_10.bin");

        (bool result, string content) = await DownloadHelper.GetFileContentAsync(apiKey, downloadUrl);

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

        var lists = DownloadInfo.ParseDownloadInfoFileContent(content);

        // Utils.WriteToJsonFile<List<DownloadInfo>>(FileDownloader.GetDownloadListSaveFile(), lists.Item3);

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
            // string jsonContent = JsonSerializer.Serialize(
            //     new
            //     {
            //         key1 = "items1",
            //         key2 = "items2"
            //     }
            // );

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
            //     bool isValidUrl = await DownloadHelper.IsDownloadableAsync(apiKey, urlDownloadServer);

            //     // string urlDownloadServer__ = $"{FILE_SERVER_BASE_URL}/patch/{item.fileName}";
            //     long TotalFileSize = await DownloadHelper.GetFileSizeAsync(apiKey, urlDownloadServer);

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

            // Utils.WriteToJsonFile<List<DownloadInfo>>(DEFAULT_DOWNLOAD_FOLDER + "/.myObjects.json", f__);

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
            // DownloadHelper.CancelDownloads(taskList.Select(t => t.Item2).ToArray());

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


            // --------- S3             

            string url = "http://localhost:3050/patch/text01.txt";
            var __v = Utils.ExtractFilePathFromUrl(url);

            string accessKeyId = "___";
            string secretAccessKey = "___";
            string bucketFolderName = "test/newest.txt";

            string bucketName = "___";
            string s3Folder = bucketFolderName;

            using (AmazonS3Client s3Client__ = new AmazonS3Client(accessKeyId, secretAccessKey, RegionEndpoint.APNortheast2))
            {
                // var r__ = await DownloadHelperS3.GetFileContentFromAsync(s3Client, bucketName, s3Folder);
                // Console.WriteLine("content: " + r__);

                // var r2__ = await DownloadHelperS3.IsDownloadableAsync(s3Client, bucketName, s3Folder);
                // Console.WriteLine("content: " + r2__);

                // var r3__ = await DownloadHelperS3.GetFileSizeAsync(s3Client, bucketName, s3Folder);
                // Console.WriteLine("content: " + r3__);

                // --------- S3
                this.OverallProgressBar = new TextProgressBar
                {
                    Name = "OverallProgressBar",
                    Width = 500,
                    Height = 18,
                    VisualMode = ProgressBarDisplayMode.Percentage,
                    Location = new Point(15, 75),
                    Maximum = 100,
                    Value = 0,
                    LastValue = 0
                };

                CancellationTokenSource cts = new();

                FileDownloaderItem item = new FileDownloaderItem("http://AWS/" + s3Folder, this.DownloadFolder, ServerType.AwsS3);

                item.InitializeTotalBytesReceived();

                // item.InitializeTotalFileSize(this.s3Client, AWS_S3_BUCKET_NAME);

                string s3FilePath = Utils.ExtractFilePathFromUrl(item.Url);

                // await DownloadHelper.GetFileSizeAsync_S3(s3Client__, bucketName, s3FilePath).ContinueWith(task =>
                // {
                //     item.TotalFileSize = task.Result;
                // });

                // item.InitializeTotalFileSize(apiKey);
                // await DownloadHelperS3.GetFileSizeAsync(s3Client, bucketName, s3Folder).ContinueWith(task =>
                // {
                //     item.TotalFileSize = task.Result;
                // });

                // await DownloadHelperS3.GetFileSizeAsync(s3Client__, bucketName, s3Folder).ContinueWith(task =>
                // {
                //     item.TotalFileSize = task.Result;
                // });

                // item.ProgressUpdated += UpdateOverallProgress;
                // item.StatusUpdated += (sender, status) => Logger.Log($"[StatusUpdated][{url}] {status}");

                // this.DownloadItems.Add(item);

                // await DownloadHelperS3.DownloadFileAsync(s3Client, bucketName, item, UpdateUI, cts);
            }

            // --------- S3 

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

