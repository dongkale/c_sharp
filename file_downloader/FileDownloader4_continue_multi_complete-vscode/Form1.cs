using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winforms_src
{   

    public partial class Form1 : Form
    {
        private List<DownloadItem> downloadItems = new List<DownloadItem>();
        
        public Form1()
        {
            InitializeComponent();        
        }        

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void btnAddUrl_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtUrl.Text))
            {
                AddDownloadItem(txtUrl.Text);
                txtUrl.Clear();
            }
        }

        private void AddDownloadItem(string url)
        {
            if (downloadItems.Any(x => x.Url == url))
            {
                MessageBox.Show("이미 추가된 URL입니다.");
                return;
            }

            DownloadItem item = new DownloadItem(url, txtFolder.Text);
            downloadItems.Add(item);
            flowLayoutPanel1.Controls.Add(item.Panel);            
            Logger.Log(String.Format("{0} - 다운로드 추가", url));
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (downloadItems.Count == 0 || string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("URL과 다운로드 폴더를 지정해주세요.");
                return;
            }
            foreach (var item in downloadItems)
            {
                if (item.IsComplete)
                {
                    continue;
                }

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
            int partCount = 4; // number of parts to split the download

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

                    long partSize = totalFileSize / partCount;
                    List<Task> downloadTasks = new List<Task>();

                    for (int i = 0; i < partCount; i++)
                    {
                        long start = i * partSize;
                        long end = (i == partCount - 1) ? totalFileSize - 1 : (start + partSize - 1);
                        downloadTasks.Add(DownloadPartAsync(item, start, end, i));
                    }

                    await Task.WhenAll(downloadTasks);

                    // Ensure parts are combined correctly
                    CombineParts(item, partCount);
                    Invoke(new Action(() => item.UpdateStatus("다운로드 완료")));                    

                    item.IsComplete = true;
                    
                    Logger.Log(String.Format("{0} - 다운로드 완료", item.Url));
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => item.UpdateStatus("다운로드 오류: " + ex.Message)));                    

                    Logger.ErrorLog(String.Format("{0} - 다운로드 오류: {1}", item.Url, ex.Message));                    
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
    }

    public class DownloadItem
    {
        public string Url { get; private set; }
        public string DownloadPath { get; private set; }
        public TextProgressBar ProgressBar { get; private set; }
        public Label StatusLabel { get; private set; }
        public Label UrlLabel { get; private set; }
        public Panel Panel { get; private set; }
        public long TotalBytesReceived { get; set; }
        public long TotalFileSize { get; set; }
        public bool IsComplete { get; set; }

        public DownloadItem(string url, string folderPath)
        {
            IsComplete = false;
            Url = url;
            DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));
            InitializeComponents();
            InitializeTotalBytesReceived();
        }

        private void InitializeComponents()
        {
            Panel = new Panel { Width = 600, Height = 48 };
            ProgressBar = new TextProgressBar { Width = 500, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(0, 15) };
            StatusLabel = new Label { Width = 500, Location = new System.Drawing.Point(0, 35) };
            UrlLabel = new Label { Text = Url, Width = 500, Location = new System.Drawing.Point(0, 0) };

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
            UrlLabel.Text = Url;        

            return;
        }

        public void UpdateStatus(string status)
        {
            StatusLabel.Text = status;
        }
    }    
}