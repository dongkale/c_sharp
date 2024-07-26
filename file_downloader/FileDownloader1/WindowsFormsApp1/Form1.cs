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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private WebClient webClient;
        private string downloadUrl;
        private string downloadPath;
        private long totalBytesReceived = 0;
        private long totalFileSize = 0;

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

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrl.Text) || string.IsNullOrEmpty(txtFolder.Text))
            {
                MessageBox.Show("URL과 다운로드 폴더를 지정해주세요.");
                return;
            }

            downloadUrl = txtUrl.Text;
            downloadPath = Path.Combine(txtFolder.Text, Path.GetFileName(downloadUrl));

            if (File.Exists(downloadPath))
            {
                totalBytesReceived = new FileInfo(downloadPath).Length;
            }

            progressBar.Value = 0;
            lblStatus.Text = "다운로드 중...";
            DownloadFile(downloadUrl, downloadPath);
        }

        private void DownloadFile(string url, string path)
        {
            webClient = new WebClient();
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(WebClient_DownloadProgressChanged);
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(WebClient_DownloadFileCompleted);

            if (totalBytesReceived > 0)
            {
                webClient.Headers.Add("Range", "bytes=" + totalBytesReceived + "-");
            }

            try
            {
                webClient.DownloadFileAsync(new Uri(url), path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("다운로드 오류: " + ex.Message);
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (totalFileSize == 0)
            {
                totalFileSize = e.TotalBytesToReceive + totalBytesReceived;
            }

            progressBar.Value = (int)((totalBytesReceived + e.BytesReceived) * 100 / totalFileSize);
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lblStatus.Text = "다운로드 취소됨";
            }
            else if (e.Error != null)
            {
                lblStatus.Text = "다운로드 오류";
            }
            else
            {
                lblStatus.Text = "다운로드 완료";
            }
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

