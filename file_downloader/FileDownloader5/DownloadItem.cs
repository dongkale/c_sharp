using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FileDownloader5
{
    public class DownloadItem
    {
        public string Url { get; private set; }
        public string DownloadPath { get; private set; }
        public TextProgressBar ProgressBar { get; private set; }
        public Label StatusLabel { get; private set; }
        public Label UrlLabel { get; private set; }
        public Panel Panel { get; private set; }

        public long TotalFileSize { get; set; }
        public long TotalBytesReceived { get; set; }

        public DownloadItem(string url, string folderPath)
        {
            Url = url;
            DownloadPath = Path.Combine(folderPath, Path.GetFileName(url));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Panel = new Panel { Width = 600, Height = 48 };
            ProgressBar = new TextProgressBar { Width = 500, Height = 20, VisualMode = ProgressBarDisplayMode.Percentage, Location = new System.Drawing.Point(0, 15) };
            StatusLabel = new Label { Width = 500, Location = new System.Drawing.Point(0, 35) };
            UrlLabel = new Label { Text = Url, Width = 500, Location = new System.Drawing.Point(0, 0) };

            Panel.Controls.Add(ProgressBar);
            // Panel.Controls.Add(StatusLabel);

            // ProgressBar.Controls.Add(UrlLabel);
        }

        public void UpdateProgress(long bytesReceived, long totalBytes)
        {
            ProgressBar.Maximum = (int)totalBytes;
            ProgressBar.Value = (int)bytesReceived;
            // ProgressBar.Text = $"{(int)((double)bytesReceived / totalBytes * 100)}%";

            UrlLabel.Text = Url;
        }

        public void UpdateStatus(string status)
        {
            StatusLabel.Text = status;
        }
    }
}