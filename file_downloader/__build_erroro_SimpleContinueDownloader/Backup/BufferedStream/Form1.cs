using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;


namespace BufferedStreamTest
{
    public partial class Form1 : Form
    {
        private WebRequest request;
        private WebResponse response;
        private BufferedStream bs = null;
        private FileStream fs = null;
        private Thread Th1 =null;
        private byte[] buffer = new byte[512];
        private int Count = 0;
        
        public Form1()
        {
            InitializeComponent();
        }

     

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "일시중지(&P)")
            {
                //전송을 잠깐 멈추고 
                Th1.Suspend();
                
                button1.Text = "이어받기(&R)";
                button1.Enabled = true;
                button2.Text = "닫 기(&C)";
                
            }
            else
            {
                if (Th1 != null)
                {


                    if (Th1.ThreadState == ThreadState.Suspended)
                        Th1.Resume();
                    
                    Th1.Abort();
                    Th1 = null;
                }

                if (bs != null)
                {
                    bs.Close();
                    bs = null;
                }
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                
                Close();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "이어받기(&R)")
            {
                button1.Enabled = false;
                button2.Text = "일시중지(&P)";
                Th1.Resume();
                return;
            }
            
            //TextBox 유효성 체크
            if(Th1 != null)
            {
                Th1.Abort();
                Th1 = null;
            }

            if (UrlText.Text == "" || FolderText.Text == "")
            {
                MessageBox.Show("목표 대상 URL 또는 목표 대상 폴더 지역이 공백입니다.!", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            //해당 폴더 유효성 체크
            if (!Directory.Exists(FolderText.Text))
            {
                MessageBox.Show("다운로드 할 위치가 유효하지 않습니다.","경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!FolderText.Text.EndsWith(@"\"))
                FolderText.Text += @"\";
            
            //파일저장할 때 임시 파일로 원래 파일명+CRK를 생성 후 저장시마다 해당 Byte정보 기록
            //파일이 이미 존재하면 파일명+CRK존재 여부를 확인 뒤 없으면 새로 있으면 이어받기
            //여부를 물어보고 이어 받기 여부 취소시 파일명+CRK기존 파일을 덮어쓰고 완료시 역시 삭제함
            
            //실제 파일에 쓰는 부분은 스레드로 뺌
            Th1 = new Thread(new ThreadStart(Receive));
            Th1.Start();

        }

        private void Receive()
        {
            

            //먼저 받으려는 파일이 존재하는지 체크하기 위해
            //FileExist검사->파일이 있다면 CRK파일을 체크
            if (File.Exists(FolderText.Text + Path.GetFileName(UrlText.Text)))
            {
                DialogResult DlgR;
                //파일이 존재 한다면
                //CRK파일이 있다면 이어받으시겠습니까? 이어 받기 / 덮어 씌기 / 취소 메세지 박스 생성
                DlgR = MessageBox.Show("이어받으시겠습니까?", "알 림", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Asterisk);
                if (DlgR == DialogResult.Cancel)
                {
                    MessageBox.Show("전송을 중지 합니다.");
                    return;
                }
                else if (DlgR == DialogResult.Yes)
                {
                    //이어받기 작업
                    //전송중일때 버튼2는 일시중지 기능
                    button1.Enabled = false;
                    button2.Text = "일시중지(&P)";

                   
                    

                    //해당 파일을 이어받기 위해서 받을 파일이름으로 하나 생성
                    fs = new FileStream(@FolderText.Text + Path.GetFileName(UrlText.Text), FileMode.Append, FileAccess.Write);
                    

                    //파일 사이즈 test
                    //------------
                    MessageBox.Show("현재 받아져있는 파일크기 = "+fs.Length.ToString());

                    request = WebRequest.Create(UrlText.Text);
                    response = request.GetResponse();
                    long FileSize = response.ContentLength;
                    //MessageBox.Show("파일 사이즈 정보 = "+ FileSize.ToString());
                    response.Close();
                    request.Abort();
                    //------------------------------------------------------

                    HttpWebRequest Hrequest = (HttpWebRequest)WebRequest.Create(UrlText.Text);
                    Hrequest.AddRange((int)fs.Length+1);
                    response = Hrequest.GetResponse();
                    
                    
                    bs = new BufferedStream(response.GetResponseStream());

                    int a = 0;
                    progressBar.Maximum = (int)FileSize;
                    progressBar.Value = (int)fs.Length;
                    
                    
                    do
                    {
                        //파일 Byte로 저장

                    
                        Count = bs.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, Count);
                        
                       
                        progressBar.Value += Count;
                        

                    } while (Count > 0);

                    
                    MessageBox.Show("다운로드 완료", "알 림", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    
                    fs.Close();
                    fs = null;
                    bs.Close();
                    bs = null;
                    File.Delete(@FolderText.Text + Path.GetFileName(UrlText.Text) + ".CRK");
                    progressBar.Value = 0;
                    button2.Text = "닫 기(&C)";
                    button1.Text = "전 송(&R)";
                    button1.Enabled = true;
                    Hrequest.Abort();
                    response.Close();


                }
                else
                {
                    //전송중일때 버튼2는 일시중지 기능
                    button1.Enabled = false;
                    button2.Text = "일시중지(&P)";


                    request = WebRequest.Create(UrlText.Text);
                    response = request.GetResponse();
                    bs = new BufferedStream(response.GetResponseStream());

                    if (!FolderText.Text.EndsWith(@"\"))
                        FolderText.Text += @"\";

                    //해당 파일을 쓰기 위해서 받을 파일이름으로 하나 생성
                    fs = new FileStream(@FolderText.Text + Path.GetFileName(UrlText.Text), FileMode.Create, FileAccess.Write);
                    
                    progressBar.Maximum = (int)(response.ContentLength);
                    
                    do
                    {
                        //파일 Byte로 저장
                        Count = bs.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, Count);
                        
                        progressBar.Value += Count;
                        
                    } while (Count > 0);
                    
                    MessageBox.Show("다운로드 완료", "알 림", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    
                    fs.Close();
                    fs = null;
                    bs.Close();
                    bs = null;
                    
                    progressBar.Value = 0;
                    button2.Text = "닫 기(&C)";
                    button1.Text = "전 송(&R)";
                    button1.Enabled = true;
                    request.Abort();
                    response.Close();
                    //No - 덮어쓰기 그냥 다운받음
                }
            }
            else
            {
                DialogResult dlg = DialogResult.Yes;

                if (File.Exists(FolderText.Text + @"\" + Path.GetFileName(UrlText.Text)))
                    dlg = MessageBox.Show("같은 파일이 이미 존재합니다. 덮어씌기 하시 겠습니까?", "경 고", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (dlg == DialogResult.Yes)
                    {
                        //전송중일때 버튼2는 일시중지 기능
                        button1.Enabled = false;
                        button2.Text = "일시중지(&P)";

                        request = WebRequest.Create(UrlText.Text);
                        response = request.GetResponse();
                        bs = new BufferedStream(response.GetResponseStream());

                        if (!FolderText.Text.EndsWith(@"\"))
                            FolderText.Text += @"\";

                        //해당 파일을 쓰기 위해서 받을 파일이름으로 하나 생성
                        fs = new FileStream(@FolderText.Text + Path.GetFileName(UrlText.Text), FileMode.Create, FileAccess.Write);

                        progressBar.Maximum = (int)(response.ContentLength);
                        MessageBox.Show(progressBar.Maximum.ToString());

                        do
                        {
                            //파일 Byte로 저장
                            Count = bs.Read(buffer, 0, buffer.Length);
                            fs.Write(buffer, 0, Count);
                            
                            progressBar.Value += (int)Count;                          
                            
                        } while (Count > 0);
                        MessageBox.Show("다운로드 완료", "알 림", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        
                        fs.Close();
                        fs = null;
                        bs.Close();
                        bs = null;
                        
                        progressBar.Value = 0;
                        button2.Text = "닫 기(&C)";
                        button1.Text = "전 송(&R)";
                        button1.Enabled = true;
                        request.Abort();
                        response.Close();
                    }
                    else
                        return;
                
                
                    
                    //그냥 다운받음
            }
            //숫자를 리턴해서 Check
            //Path클래스를 이용 파일이름을 가져온 후 FileStream클래스로 생성
            //작업을 진행 함
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Th1 != null)
            {

                if (Th1.ThreadState == ThreadState.Suspended)
                    Th1.Resume();
                Th1.Abort();
            }
        }

        
    }
}