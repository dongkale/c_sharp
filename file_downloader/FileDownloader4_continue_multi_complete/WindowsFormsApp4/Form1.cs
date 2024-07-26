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

namespace WindowsFormsApp4
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
            DownloadItem item = new DownloadItem(url, txtFolder.Text);
            downloadItems.Add(item);
            flowLayoutPanel1.Controls.Add(item.Panel);
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
                StartDownload(item);
            }
        }

        private void StartDownload(DownloadItem item)
        {
            Task.Run(() => DownloadFileAsync(item));
        }

        private async Task DownloadFileAsync(DownloadItem item)
        {
            long totalBytesReceived = 0;
            long totalFileSize = 0;

            if (File.Exists(item.DownloadPath))
            {
                totalBytesReceived = new FileInfo(item.DownloadPath).Length;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(item.Url);
            request.Method = "HEAD";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    totalFileSize = response.ContentLength;
                }

                if (totalBytesReceived >= totalFileSize)
                {
                    Invoke(new Action(() => item.UpdateStatus("파일이 이미 완전히 다운로드되었습니다.")));
                    return;
                }

                request = (HttpWebRequest)WebRequest.Create(item.Url);
                if (totalBytesReceived > 0)
                {
                    request.AddRange(totalBytesReceived);
                }

                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream responseStream = response.GetResponseStream())
                using (FileStream fileStream = new FileStream(item.DownloadPath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                        totalBytesReceived += bytesRead;
                        Invoke(new Action(() => item.UpdateProgress(totalBytesReceived, totalFileSize)));
                    }
                }

                Invoke(new Action(() => item.UpdateStatus("다운로드 완료")));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() => item.UpdateStatus("다운로드 오류: " + ex.Message)));
            }
        }
    }

    public class DownloadItem
    {
        public string Url { get; private set; }
        public string DownloadPath { get; private set; }
        public ProgressBar ProgressBar { get; private set; }
        public Label StatusLabel { get; private set; }
        public Label UrlLabel { get; private set; }
        public Panel Panel { get; private set; }

        public DownloadItem(string url, string folderPath)
        {
            Url = url;
            DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Panel = new Panel { Width = 600, Height = 80 };
            ProgressBar = new ProgressBar { Width = 500, Location = new System.Drawing.Point(0, 20) };
            StatusLabel = new Label { Width = 500, Location = new System.Drawing.Point(0, 45) };
            UrlLabel = new Label { Width = 500, Location = new System.Drawing.Point(0, 0) };

            // Add ProgressBar and StatusLabel to Panel
            Panel.Controls.Add(ProgressBar);
            Panel.Controls.Add(StatusLabel);

            // Add URL Label to ProgressBar
            Panel.Controls.Add(UrlLabel);
        }

        public void UpdateProgress(long bytesReceived, long totalBytes)
        {
            ProgressBar.Maximum = (int)totalBytes;
            ProgressBar.Value = (int)bytesReceived;
            StatusLabel.Text = $"{bytesReceived} / {totalBytes} bytes";

            // Update URL Label to display within ProgressBar
            UrlLabel.Text = Url;
        }

        public void UpdateStatus(string status)
        {
            StatusLabel.Text = status;
        }
    }
}
