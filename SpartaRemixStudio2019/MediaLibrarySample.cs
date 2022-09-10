using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public partial class MediaLibrarySample : UserControl
    {
        public event EventHandler ItemPicked;

        public MediaLibrarySample()
        {
            InitializeComponent();
            DoubleBuffered = true;
            UpdateScrollBarHeight();
        }

        public int ItemHeight = 30;
        private int Yoffset = 0;
        private int itemSelected = -1;
        private int itemHighlited = -1;

        private void MediaLibrarySample_Paint(object sender, PaintEventArgs e)
        {
            if (Form1.Project != null)
            {
                UpdateScrollBarHeight();
                int posy = Yoffset;
                int index = 0;
                foreach (KeyValuePair<uint, SampleAV> k in Form1.Project.ProjectSamples)
                {
                    if (itemHighlited == index) e.Graphics.FillRectangle(Brushes.Orange, new Rectangle(0, posy, Width, ItemHeight));
                    else if (itemSelected == index) e.Graphics.FillRectangle(Brushes.LightBlue, new Rectangle(0, posy, Width,ItemHeight));


                    if (k.Value.ivs != null) if (k.Value.ivs.PreviewBitmap != null)
                    {
                        e.Graphics.DrawImage(k.Value.ivs.PreviewBitmap, new Rectangle(0, posy, ItemHeight * 4 / 3, ItemHeight));
                    }
                    e.Graphics.DrawString(k.Key.ToString(), new Font("Arial", 10), Brushes.White, 0, posy);
                    if (k.Value.name != null) e.Graphics.DrawString(k.Value.name, new Font("Arial", 10), Brushes.Black,ItemHeight * 4 / 3 + 3, posy);
                    else e.Graphics.DrawString("(SAMPLE " + k.Key.ToString() + ")", new Font("Arial", 10), Brushes.Black, ItemHeight * 4 / 3 + 3, posy);

                    posy += ItemHeight;
                    index++;
                }
            }
        }
        private void UpdateScrollBarHeight()
        {
            if (Form1.Project != null)
            {
                int i = (Form1.Project.ProjectSamples.Count + 2) * ItemHeight - Height;
                vScrollBar1.Maximum = (i > 0) ? i : 0;
            }
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            Yoffset = -vScrollBar1.Value;
        }

        private void MediaLibrarySample_MouseMove(object sender, MouseEventArgs e)
        {
            itemHighlited = (e.Y - Yoffset) / ItemHeight;
            Invalidate();
        }
        private void MediaLibrarySample_Click(object sender, EventArgs e)
        {
            itemSelected = itemHighlited;
            ItemPicked?.Invoke(this, new EventArgs());
            Invalidate();
        }

        public uint GetSample()
        {
            if (itemSelected < 0) return uint.MaxValue;
            else if (itemSelected >= Form1.Project.ProjectSamples.Count) return uint.MaxValue;
            else
            {
                int left = itemSelected;
                foreach (uint u in Form1.Project.ProjectSamples.Keys)
                {
                    if (left == 0) return u;
                    left--;
                }
                return uint.MaxValue;
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}
