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
    public partial class MixerControl : UserControl
    {
        public int HorizontalSpacingLayer { get; set; }
        int OffsetX = 0;
        int OffsetY = 0;
        public int NodeRectangleW = 30;
        public int NodeRectangleH = 20;

        public MixerControl()
        {
            InitializeComponent();
            HorizontalSpacingLayer = 100;
        }

        private void MixerControl_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, this.Width, this.Height));
            int curx = -OffsetX;
            int index = 0;
            if (Form1.Project != null)
                foreach (MixerLayer ml in Form1.Project.m.Layers)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(0, 0, 255 - index * 15)), new Rectangle(curx, 0, HorizontalSpacingLayer, this.Height));
                    curx += HorizontalSpacingLayer;
                    index++;
                }
            curx = -OffsetX;
            index = 0;
            if (Form1.Project != null)
                foreach (MixerLayer ml in Form1.Project.m.Layers)
                {
                    foreach (MixerLink t in ml.ListOfLinks)
                    {
                        if (Form1.Project.Tracks.ContainsKey(t.Links.Item1) || Form1.Project.AudioMPs.ContainsKey(t.Links.Item1))
                        {
                            if (t.Links.Item1 >> 20 == 1)
                            {
                                g.FillRectangle(Brushes.DarkGray, new Rectangle(curx + t.MixerPosX, t.MixerPosY - OffsetY, NodeRectangleW, NodeRectangleH));
                                g.DrawString(t.Links.Item1.ToString(), new Font("Arial", 10), Brushes.White, curx + t.MixerPosX, t.MixerPosY - OffsetY);
                            }
                            else
                            {
                                g.FillRectangle(Brushes.Orange, new Rectangle(curx + t.MixerPosX, t.MixerPosY - OffsetY, NodeRectangleW, NodeRectangleH));
                                g.DrawString(t.Links.Item1.ToString(), new Font("Arial", 10), Brushes.Black, curx + t.MixerPosX, t.MixerPosY - OffsetY);
                            }
                        }
                        else
                        {
                            g.FillRectangle(Brushes.Red, new Rectangle(curx + t.MixerPosX, t.MixerPosY - OffsetY, NodeRectangleW, NodeRectangleH));
                            g.DrawString(t.Links.Item1.ToString(), new Font("Arial", 10), Brushes.Black, curx + t.MixerPosX, t.MixerPosY - OffsetY);
                        }
                        foreach (uint u in t.Links.Item2)
                        {
                            uint l = Form1.Project.m.GetNodeLayer(u);
                            if (l < uint.MaxValue)
                            {
                                if (l > index)
                                {
                                    MixerLink ml2 = Form1.Project.m.GetNode(u);
                                    g.DrawLine(Pens.White, curx + t.MixerPosX + NodeRectangleW, t.MixerPosY - OffsetY + NodeRectangleH / 2, curx + ml2.MixerPosX + HorizontalSpacingLayer * (l - index), ml2.MixerPosY - OffsetY + NodeRectangleH / 2);
                                }
                            }
                        }
                    }
                    curx += HorizontalSpacingLayer;
                    index++;
                }
        }

        MixerLink downOn = null;
        int downX = 0;
        int downY = 0;
        int origVal0 = 0;
        int origVal1 = 0;
        private void MixerControl_MouseDown(object sender, MouseEventArgs e)
        {
            int curx = -OffsetX;
            int index = 0;
            if (Form1.Project != null) foreach (MixerLayer ml in Form1.Project.m.Layers)
                {
                    if (e.X > curx && e.X < curx + HorizontalSpacingLayer) foreach (MixerLink t in ml.ListOfLinks)
                    {
                            if (new Rectangle(curx + t.MixerPosX, t.MixerPosY - OffsetY, NodeRectangleW, NodeRectangleH).Contains(e.X, e.Y))
                            {
                                downOn = t;
                                downX = e.X;
                                downY = e.Y;
                                origVal0 = t.MixerPosX;
                                origVal1 = t.MixerPosY;
                            }
                    }
                    curx += HorizontalSpacingLayer;
                    index++;
                }
        }
        private void MixerControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (downOn != null)
            {
                downOn.MixerPosX = origVal0 + (e.X - downX);
                downOn.MixerPosY = origVal1 + (e.Y - downY);
                Invalidate();
            }
        }
        private void MixerControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (downOn != null)
            {
                if (downOn.MixerPosX > HorizontalSpacingLayer - NodeRectangleW) downOn.MixerPosX = HorizontalSpacingLayer - NodeRectangleW;
                if (downOn.MixerPosY < 0) downOn.MixerPosY = 0;
                if (downOn.MixerPosX < 0) downOn.MixerPosX = 0;
            }
            downOn = null;
            Invalidate();
        }
    }
}
