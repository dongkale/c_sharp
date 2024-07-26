using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public class CustomProgressBar : ProgressBar
    {
        private string url = "";

        public string Url
        {
            get { return url; }
            set { url = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw progress bar
            using (SolidBrush brush = new SolidBrush(Color.LightBlue))
            {
                e.Graphics.FillRectangle(brush, 0, 0, (float)Value / Maximum * Width, Height);
            }

            // Draw the URL text
            using (SolidBrush textBrush = new SolidBrush(Color.Black))
            {
                string text = url.Length > 0 ? url : "다운로드 중...";
                using (Font font = new Font("Arial", 8f))
                {
                    SizeF textSize = e.Graphics.MeasureString(text, font);
                    PointF textPosition = new PointF((Width - textSize.Width) / 2, (Height - textSize.Height) / 2);
                    e.Graphics.DrawString(text, font, textBrush, textPosition);
                }
            }
        }
    }
}
