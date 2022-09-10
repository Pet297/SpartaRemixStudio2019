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
    public partial class CollectionEditor : UserControl
    {
        public List<IAudioFX> afx;
        public List<IVideoFX> vfx;
        public List<INumberFX> nfx;

        int offset;
        int itemHeight = 20;

        public CollectionEditor()
        {
            InitializeComponent();
        }

        public int SelectedLoc { get; set; }

        private void CollectionEditor_Paint(object sender, PaintEventArgs e)
        {
            int index = 0;
            e.Graphics.FillRectangle(Brushes.White, new Rectangle(0, 0, Width, Height));
            if (afx != null)
            {
                foreach(IAudioFX fx in afx)
                {
                    if (mp == index) e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(0, index * itemHeight - offset,Width,itemHeight));
                    e.Graphics.DrawString(fx.DisplayName, new Font("Arial", 9) ,Brushes.Black, new Point(5, index* itemHeight - offset));
                    index++;
                }
            }
            else if (vfx != null)
            {
                foreach (IVideoFX fx in vfx)
                {
                    if (mp == index) e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(0, index * itemHeight - offset, Width, itemHeight));
                    e.Graphics.DrawString(fx.DisplayName, new Font("Arial", 9), Brushes.Black, new Point(5, index * itemHeight - offset));
                    index++;
                }
            }
            else if (nfx != null)
            {
                foreach (INumberFX fx in nfx)
                {
                    if (mp == index) e.Graphics.FillRectangle(Brushes.Blue, new Rectangle(0, index * itemHeight - offset, Width, itemHeight));
                    e.Graphics.DrawString(fx.DisplayName, new Font("Arial", 9), Brushes.Black, new Point(5, index * itemHeight - offset));
                    index++;
                }
            }
        }

        int mp = -1;
        bool md = false;
        int pickUpAt = -1;
        private void CollectionEditor_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (afx != null)
                {
                    if (mp >= 0 && mp < afx.Count)
                    {
                        md = true;
                        pickUpAt = mp;
                    }
                }
                if (vfx != null)
                {
                    if (mp >= 0 && mp < vfx.Count)
                    {
                        md = true;
                        pickUpAt = mp;
                    }
                }
                if (nfx != null)
                {
                    if (mp >= 0 && mp < nfx.Count)
                    {
                        md = true;
                        pickUpAt = mp;
                    }
                }
            }
        }
        private void CollectionEditor_MouseMove(object sender, MouseEventArgs e)
        {
            if (!md)
            {
                mp = (e.Y + offset) / itemHeight;
            }
            else
            {
                int newInd = (e.Y + offset) / itemHeight;
                int c = 0;
                if (afx != null) c = afx.Count;
                if (vfx != null) c = vfx.Count;
                if (nfx != null) c = nfx.Count;
                if (newInd != mp && newInd >= 0 && newInd < c)
                {
                    if (afx != null)
                    {
                        IAudioFX a0 = afx[mp];
                        afx.RemoveAt(mp);
                        afx.Insert(newInd, a0);
                        mp = newInd;
                    }
                    if (vfx != null)
                    {
                        IVideoFX a0 = vfx[mp];
                        vfx.RemoveAt(mp);
                        vfx.Insert(newInd, a0);
                        mp = newInd;
                    }
                    if (nfx != null)
                    {
                        INumberFX a0 = nfx[mp];
                        nfx.RemoveAt(mp);
                        nfx.Insert(newInd, a0);
                        mp = newInd;
                    }
                }
            }
            Invalidate();
        }
        private void CollectionEditor_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                SelectedLoc = mp;
                md = false;
            }
            else
            {
                mp = (e.Y + offset) / itemHeight;
                if (afx != null)
                {
                    if (mp >= 0 && mp < afx.Count)
                    {
                        afx.RemoveAt(mp);
                    }
                }
                if (vfx != null)
                {
                    if (mp >= 0 && mp < vfx.Count)
                    {
                        vfx.RemoveAt(mp);
                    }
                }
                if (nfx != null)
                {
                    if (mp >= 0 && mp < nfx.Count)
                    {
                        nfx.RemoveAt(mp);
                    }
                }
            }

            Invalidate();
        }
    }
}
