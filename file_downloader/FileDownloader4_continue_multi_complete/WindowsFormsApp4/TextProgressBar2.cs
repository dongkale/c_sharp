using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public class TextProgressBar2 : ProgressBar
    {
        public TextProgressBar2() : base()
        {
            SetStyle(ControlStyles.UserPaint, true);
            DoubleBuffered = true; // remove flicker
        }

        // unhide Text/Font Properties and force changes to re-render control

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                Refresh();
            }
        }

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                if (!string.IsNullOrWhiteSpace(Text)) Refresh();
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            // draw progress bar background
            ProgressBarRenderer.DrawHorizontalBar(pevent.Graphics, new Rectangle(0, 0, Width, Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // draw progress on progress bar
            double percentage = ((double)(Value - Minimum)) / ((double)(Maximum - Minimum));
            ProgressBarRenderer.DrawHorizontalChunks(e.Graphics, new Rectangle(0, 0, (int)(Width * percentage), Height));

            // draw text on progress bar
            using (Brush brush = new SolidBrush(ForeColor))
            {
                // get rendered size of text
                var size = e.Graphics.MeasureString(Text, Font, new SizeF(Width, Height));

                // calculate location to center text on progress bar
                var location = new PointF((Width - size.Width) * 0.5f, (Height - size.Height) * 0.5f);

                // draw text
                e.Graphics.DrawString(Text, Font, brush, new RectangleF(location, size));
            }
        }
    }
}
