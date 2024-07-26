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

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
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
            else
            {
                totalBytesReceived = 0;
            }

            progressBar.Value = 0;
            lblStatus.Text = "다운로드 중...";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);
            request.Method = "HEAD";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    totalFileSize = response.ContentLength;
                }

                if (totalBytesReceived >= totalFileSize)
                {
                    lblStatus.Text = "파일이 이미 완전히 다운로드되었습니다.";
                    return;
                }

                DownloadFile(downloadUrl, downloadPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("헤더 요청 오류: " + ex.Message);
            }
        }

        private void DownloadFile(string url, string path)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (totalBytesReceived > 0)
            {
                request.AddRange(totalBytesReceived);
            }

            request.BeginGetResponse(new AsyncCallback(ResponseCallback), request);
        }

        private void ResponseCallback(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

            Invoke(new Action(() => progressBar.Maximum = (int)totalFileSize));

            using (Stream responseStream = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(downloadPath, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    totalBytesReceived += bytesRead;
                    Invoke(new Action(() => progressBar.Value = (int)totalBytesReceived));
                }
            }

            Invoke(new Action(() =>
            {
                if (totalBytesReceived >= totalFileSize)
                {
                    lblStatus.Text = "다운로드 완료";
                }
                else
                {
                    lblStatus.Text = "다운로드 오류";
                }
            }));
        }
    }
}
