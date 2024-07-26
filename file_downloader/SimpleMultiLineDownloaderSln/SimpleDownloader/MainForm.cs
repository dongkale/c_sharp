using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Windows.Forms;

namespace SimpleDownloader
{
    public partial class MainForm : Form
    {
        private delegate void CSafeSetText(string text);
        private delegate void CSafeSetMaximum(Int32 value);
        private delegate void CSafeSetValue(Int32 value);

        private CSafeSetText csst;
        private CSafeSetMaximum cssm;
        private CSafeSetValue cssv;
        private WebClient wc;
        private Boolean setBaseSize;
        private Boolean nowDownloading;

        public MainForm()
        {

            // 대리자를 초기화한다.
            csst = new CSafeSetText(CrossSafeSetTextMethod);
            cssm = new CSafeSetMaximum(CrossSafeSetMaximumMethod);
            cssv = new CSafeSetValue(CrossSafeSetValueMethod);

            // 웹 클라이언트 개체를 초기화하고,
            wc = new WebClient();

            InitializeComponent();
        }

        List<string> addressList = new List<string>();
        int currentIndex = 0;
        void DownloadOne()
        {
            if (nowDownloading)
            {
                MessageBox.Show("이미 다운로드가 진행 중입니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            String remoteAddress = txtAddress.Text.Trim();
            if (String.IsNullOrEmpty(remoteAddress))
            {
                MessageBox.Show("주소가 입력되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // 파일이 저장될 위치를 저장한다.
            string fileName = txtFolder.Text + "\\"+System.IO.Path.GetFileName(remoteAddress);

            // 폴더가 존재하지 않는다면 폴더를 생성한다.
            if (!System.IO.Directory.Exists(txtFolder.Text))
                System.IO.Directory.CreateDirectory(txtFolder.Text);

            try
            {

                // C 드라이브 밑의 downloadFiles 폴더에 파일 이름대로 저장한다.
                wc.DownloadFileAsync(new Uri(remoteAddress), fileName);

                // 다운로드 중이라는걸 알리기 위한 값을 설정하고,
                // 프로그레스바의 크기를 0으로 만든다.
                prgDownloadOne.Value = 0;
                setBaseSize = false;
                nowDownloading = true;
                btnStart_.Enabled = false;
                txtAddress.Enabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        void CrossSafeSetValueMethod(Int32 value)
        {
            if (prgDownloadOne.InvokeRequired)
                prgDownloadOne.Invoke(cssm, value);
            else
                prgDownloadOne.Value = value;
        }
        void CrossSafeSetMaximumMethod(Int32 value)
        {
            if (prgDownloadOne.InvokeRequired)
                prgDownloadOne.Invoke(cssm, value);
            else
                prgDownloadOne.Maximum = value;
        }
        void CrossSafeSetTextMethod(String text)
        {
            if (this.InvokeRequired)
                this.Invoke(csst, text);
            else
                this.Text = text;
        }

        void MainFormLoad(object sender, EventArgs e)
        {
            // 이벤트를 연결한다.
            wc.DownloadFileCompleted += new AsyncCompletedEventHandler(fileDownloadCompleted);
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(fileDownloadProgressChanged);
        }

        void fileDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

            // e.BytesReceived
            //   받은 데이터의 크기를 저장합니다.

            // e.TotalBytesToReceive
            //   받아야 할 모든 데이터의 크기를 저장합니다.

            // 프로그레스바의 최대 크기가 정해지지 않은 경우,
            // 받아야 할 최대 데이터 량으로 설정한다.
            if (!setBaseSize)
            {
                CrossSafeSetMaximumMethod((int)e.TotalBytesToReceive);
                setBaseSize = true;
            }

            // 받은 데이터 량을 나타낸다.
            CrossSafeSetValueMethod((int)e.BytesReceived);

            // 받은 데이터 / 받아야할 데이터 (퍼센트) 로 나타낸다.
            CrossSafeSetTextMethod(String.Format("{0:N0} / {1:N0} ({2:P})", e.BytesReceived, e.TotalBytesToReceive, (Double)e.BytesReceived / (Double)e.TotalBytesToReceive));
        }

        void fileDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            nowDownloading = false;
            btnStart_.Enabled = true;
            txtAddress.Enabled = true;

            currentIndex++;
            if (addressList.Count <= currentIndex) return;
            txtAddress.Text = addressList[currentIndex];
            DownloadOne();
        }

        private void btnStart__Click(object sender, EventArgs e)
        {
            addressList.Clear();
            foreach (string line in txtAddressList.Lines)
            {
                addressList.Add(line);
            }

            currentIndex = 0;
            if (addressList.Count <= currentIndex) return;
            txtAddress.Text = addressList[currentIndex];
            DownloadOne();
        }
    }
}