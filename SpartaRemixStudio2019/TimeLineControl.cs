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
    [Browsable(false)]
    public partial class TimeLineControl : UserControl
    {
        public Form1 parentForm;
        //Viewport Management
        public float ViewPortBeats
        {
            get
            {
                return viewPortBeats;
            }
            set
            {
                if (value <= 1 / 128f) viewPortBeats = 1 / 128f;
                else if (value >= 1048576f) viewPortBeats = 1048576f;
                else viewPortBeats = value;
                RecalculateBeatSpacing();
            }
        }
        public float ViewPortStart
        {
            get
            {
                return viewPortStart;
            }
            set
            {
                if (value <= 0) viewPortStart = 0;
                else viewPortStart = value;
                RecalculateBeatSpacing();
            }
        }
        private float SignificantBeatSpacing = 1.000f;
        private int SignificantBeatSpacingDenominator = 0;
        private int SignificantSpacingCount = 4;
        private float viewPortBeats = 4.000f;
        public float viewPortStart = 0.000f;
        public int sideWidth = 70;
        public int labelSpacing = 100;
        private int GetPixel(float beat)
        {
            float p = (beat - viewPortStart) / viewPortBeats;
            return sideWidth + (int)(p * (Width - sideWidth));
        }
        private float GetBeat(int pixel)
        {
            float p = (pixel - sideWidth) / (float)(Width - sideWidth);
            return ViewPortStart + viewPortBeats * p;
        }

        private int MaxBeat = 1280;
        private int TotalHeight = 30 * 100;
        private int YOfset = 0;

        private int SnapBeatUnits = 96;
        private float MediaSideBeat = 0.100f;
        //GFX
        Brush[] DataTypeColorDark = new Brush[]{
        new SolidBrush(Color.FromArgb(0,0,192)),
        new SolidBrush(Color.FromArgb(0,192,0)),
        new SolidBrush(Color.FromArgb(96,192,0)),
        new SolidBrush(Color.FromArgb(192,192,0)),
        new SolidBrush(Color.FromArgb(192,96,0)),
        new SolidBrush(Color.FromArgb(96,0,192)),
        new SolidBrush(Color.FromArgb(0,192,192)),
        new SolidBrush(Color.FromArgb(192,0,0)),
        new SolidBrush(Color.FromArgb(192,0,192)),
        };
        Pen OutlinePen = new Pen(Color.Black, 1);

        public TimeLineControl()
        {
            InitializeComponent();
            DoubleBuffered = true;
            this.MouseWheel += (s, e) => TimeLineControl_MouseWheel(s, e);

            RecalculateBeatSpacing();
            RecalculateRangeLimit();
            if (Form1.Project != null) RearangeList();
        }

        private void TimeLineControl_Paint(object sender, PaintEventArgs e)
        {
            int totalHeight = -YOfset + 20;
            Graphics g = e.Graphics;


            

            int itemIndex = 0;
            foreach (TLItem item in ListOfItems)
            {
                if (!((totalHeight < 0 && totalHeight + item.Height < 0) || (totalHeight > this.Height && totalHeight + item.Height > this.Height)))
                {
                    if (item.Type == 0)
                    {
                        g.FillRectangle(DataTypeColorDark[0], new Rectangle(0, totalHeight, sideWidth, (int)item.Height));
                        g.DrawString(item.Index.ToString(), new Font("Arial", 13), Brushes.Yellow, new Point(0, totalHeight));

                        g.DrawString(Form1.Project.Tracks[item.Index].Volume.ToString("+0.00'dB';-0.00'dB';'-'0.00'dB'"), new Font("Arial", 7), Brushes.Yellow, new Point(30, totalHeight + 10));
                        g.DrawString(Form1.Project.Tracks[item.Index].Pan.ToString("'L-'0.00;'R-'0.00;'C-0.00'"), new Font("Arial", 7), Brushes.Yellow, new Point(30, totalHeight + 20));

                        g.DrawRectangle(OutlinePen, new Rectangle(0, totalHeight, sideWidth, (int)item.Height));

                        DrawMediaOfTrack(g, item.Index, totalHeight, (int)item.Height);
                    }
                    if (item.Type == 1)
                    {
                        g.FillRectangle(DataTypeColorDark[1], new Rectangle(0, totalHeight, sideWidth, (int)item.Height));
                        g.DrawString(item.Index.ToString(), new Font("Arial", 13), Brushes.Yellow, new Point(0, totalHeight));
                        g.DrawRectangle(OutlinePen, new Rectangle(0, totalHeight, sideWidth, (int)item.Height));

                        DrawMediaOfNumberTrack(g, item.Index, totalHeight, (int)item.Height);
                    }
                    else if (item.Type == 10)
                    {
                        g.FillRectangle(DataTypeColorDark[1], new Rectangle(0, totalHeight, Width, (int)item.Height));
                        g.DrawString(item.Name, new Font("Arial", 9), Brushes.Yellow, new Point(0, totalHeight));
                    }
                }
                totalHeight += (int)item.Height;
                itemIndex++;
            }
            itemIndex = 0;
            totalHeight = -YOfset;
            totalHeight += 20;
            foreach (TLItem item in ListOfItems)
            {
                if (!((totalHeight < 0 && totalHeight + item.Height < 0) || (totalHeight > this.Height && totalHeight + item.Height > this.Height)))
                {

                    if (ListOfAddTo[itemIndex] != uint.MaxValue)
                    {
                        g.FillRectangle(Brushes.Green, new Rectangle(5, totalHeight + (int)item.Height - 8, 16, 16));
                        g.DrawString("+", new Font("Arial", 9), Brushes.Black, new Point(5, totalHeight + (int)item.Height - 8));
                    }
                    if (ListOfAddTo[itemIndex] != uint.MaxValue && copiedTrackStyle == SelectedMediaStyle.SingleTrack)
                    {
                        g.FillRectangle(Brushes.Blue, new Rectangle(25, totalHeight + (int)item.Height - 8, 16, 16));
                        g.DrawString("P", new Font("Arial", 9), Brushes.Black, new Point(25, totalHeight + (int)item.Height - 8));
                    }
                    if (ListOfAddTo[itemIndex] != uint.MaxValue && copiedTrackStyle == SelectedMediaStyle.STCut)
                    {
                        g.FillRectangle(Brushes.Orange, new Rectangle(25, totalHeight + (int)item.Height - 8, 16, 16));
                        g.DrawString("I", new Font("Arial", 9), Brushes.Black, new Point(25, totalHeight + (int)item.Height - 8));
                    }
                }
                totalHeight += (int)item.Height;
                itemIndex++;
            }
             //top bar
            g.FillRectangle(Brushes.DarkGray, new Rectangle(sideWidth, 0, Width - sideWidth, 20));
            totalHeight += 20;

            float b0 = (float)(Math.Round(viewPortStart / SignificantBeatSpacing) * SignificantBeatSpacing);
            for (int i = 0; i < SignificantSpacingCount + 1; i++)
            {
                float beat = b0 + SignificantBeatSpacing * i;
                int pos = GetPixel(beat);
                if (pos >= sideWidth)
                {
                    if (SignificantBeatSpacing >= 1) g.DrawString((beat).ToString(), new Font("Arial", 10), Brushes.Black, new Point(pos, 0));
                    else g.DrawString(((int)beat).ToString() + " " + ((int)(Math.Round((beat % 1) * SignificantBeatSpacingDenominator))).ToString() + "/" + SignificantBeatSpacingDenominator, new Font("Arial", 10), Brushes.Black, new Point(pos, 0));
                    g.DrawLine(Pens.Black, pos, 0, pos, 20);
                }
            }
            //ukazatel
        }
        private void DrawMediaOfTrack(Graphics g, uint track, int Y, int height)
        {
            foreach (uint u in Form1.Project.Tracks[track].Media)
            {
                TimelineMedia tlm = Form1.Project.ProjectMedia[u];

                DrawTLM(tlm, g, track, Y, height);
            }
        }
        private void DrawMediaOfNumberTrack(Graphics g, uint track, int Y, int height)
        {
            foreach (uint u in Form1.Project.NumberTracks[track].Media)
            {
                TimelineMedia tlm = Form1.Project.ProjectMedia[u];

                DrawTLM(tlm, g, track, Y, height);
            }
        }
        private void DrawTLM(TimelineMedia tlm, Graphics g, uint track, int Y, int height)
        {
            long tb = 192;
            if (GetPixel(tlm.TimeFrom / (float)tb) <= Width && GetPixel(tlm.TimeEnd / (float)tb) >= sideWidth)
            {
                int pf = GetPixel(tlm.TimeFrom / (float)tb);
                int pt = GetPixel(tlm.TimeEnd / (float)tb);

                if (pf < sideWidth) pf = sideWidth;
                if (pt > Width) pt = Width;
                if (pt - pf < 2) pt = pf + 2;

                //NAHRADIT SAMPL
                if (tlm.Property is TLSample)
                {
                    uint smp = (tlm.Property as TLSample).Sample;
                    IPitchSample ips = null;
                    if (Form1.Project.ProjectSamples.ContainsKey(smp)) ips = Form1.Project.ProjectSamples[smp].ips;
                    IVideoSample ivs = null;
                    if (Form1.Project.ProjectSamples.ContainsKey(smp)) ivs = Form1.Project.ProjectSamples[smp].ivs;

                    if (ips == null)
                    {
                        g.FillRectangle(Brushes.Yellow, new Rectangle(pf, Y, pt - pf, height));
                        g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));
                    }
                    else if (ips is VideoSourceS)
                    {
                        float p0 = (GetBeat(pf) - tlm.TimeFrom / 192f);
                        float p1 = (GetBeat(pt) - tlm.TimeFrom / 192f);
                        float t0 = p0 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch + tlm.SecondsIn;
                        float t1 = p1 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch + tlm.SecondsIn;

                        g.FillRectangle(Brushes.Yellow, new Rectangle(pf, Y, pt - pf, height));
                        for (int i = pf; i < pt - 40; i += 40)
                        {
                            Bitmap b = (ips as VideoSourceS).GetKeyFrame(t0 + (t1 - t0) * ((i - (float)pf) / (pt - pf)));
                            if (b != null) g.DrawImage(b, new Point(i, Y));
                        }
                        g.FillRectangle(new SolidBrush(Color.FromArgb(90, 255, 255, 255)), new Rectangle(pf, Y, pt - pf, height));
                        (ips as VideoSourceS).DrawWaveForm(pf, Y, pt - pf, height, t0, t1 - t0, Color.Yellow, Color.Black, g);
                        g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));


                    }
                    else
                    {
                        /*float p0 = (GetBeat(pf) - tlm.TimeFrom / 192f);
                        float p1 = (GetBeat(pt) - tlm.TimeFrom / 192f);
                        float t0 = p0 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch + tlm.SecondsIn;
                        float t1 = p1 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch + tlm.SecondsIn;*/

                        float p0 = (GetBeat(pf) - tlm.TimeFrom / 192f);
                        float p1 = (GetBeat(pt) - tlm.TimeFrom / 192f);

                        float t0 = p0 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch;
                        float t1 = p1 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch;

                        g.FillRectangle(Brushes.Yellow, new Rectangle(pf, Y, pt - pf, height));

                        AudioPrerenderManager.DrawWaveForm(new SampleDesriptor(tlm),pf, Y, pt - pf, height, t0, t1 - t0, Color.Yellow, Color.Black, g);

                        //if (ivs != null) g.DrawImage(ivs.PreviewBitmap, pf, Y, 40, 30);
                        g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));
                    }

                    if (ips != null)
                    {
                        if (ips.Possibilities.HasFlag(PitchSampleFlags.Pitch) && tlm.Pitch != 0)
                        {
                            g.DrawString("P:" + tlm.Pitch.ToString(), new Font("Arial", 8), Brushes.Black, pf, Y);
                        }
                        if (tlm.Volume != 0)
                        {
                            g.DrawString("V:" + tlm.Volume.ToString("0.0") + "dB", new Font("Arial", 8), Brushes.Black, pf + 20, Y);
                        }
                    }
                }
                else if (tlm.Property is TLPattern)
                {
                    float p0 = (GetBeat(pf) - tlm.TimeFrom / 192f);
                    float p1 = (GetBeat(pt) - tlm.TimeFrom / 192f);
                    float t0 = p0 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch;
                    float t1 = p1 * (Form1.Project.GetBeatDurationSec(tlm.TimeFrom / 192f)) * tlm.Stretch;

                    g.FillRectangle(Brushes.Red, new Rectangle(pf, Y, pt - pf, height));
                    g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));
                    AudioPrerenderManager.DrawWaveForm(new PatternDesriptor(tlm), pf, Y, pt - pf, height, t0, t1 - t0, Color.Red, Color.White, g);
                    string s = (tlm.Property as TLPattern).Samples.Count > 0 ? ("[" + (tlm.Property as TLPattern).Samples[0] + ((tlm.Property as TLPattern).Samples.Count > 1 ? ",...]" : "]")) : "[]";
                    g.DrawString("PAT " + (tlm.Property as TLPattern).Pattern + s, new Font("Arial", 8), Brushes.Black, new Point(pf, Y));
                    if (tlm.Pitch != 0) g.DrawString("P:" + tlm.Pitch.ToString(), new Font("Arial", 8), Brushes.Black, pf, Y + 10);
                    if (tlm.Volume != 0) g.DrawString("V:" + tlm.Volume.ToString("0.00") + "dB", new Font("Arial", 8), Brushes.Black, pf + 16, Y + 10);
                }
                else if (tlm.Property is TLNumber)
                {
                    g.FillRectangle(Brushes.Blue, new Rectangle(pf, Y, pt - pf, height));
                    g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));

                    g.DrawString((tlm.Property as TLNumber).Number.ToString(), new Font("Arial", 18), Brushes.White, new Point(pf, Y));
                }
                else if (tlm.Property is TLNumberTrans)
                {
                    g.FillRectangle(Brushes.Blue, new Rectangle(pf, Y, pt - pf, height));
                    g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));

                    if ((tlm.Property as TLNumberTrans).Number0 < (tlm.Property as TLNumberTrans).Number1) g.DrawLine(OutlinePen, pf, Y + height, pt, Y);
                    else g.DrawLine(OutlinePen, pf, Y, pt, Y + height);

                    g.DrawString((tlm.Property as TLNumberTrans).Number0.ToString(), new Font("Arial", 18), Brushes.White, new Point(pf, Y));
                    g.DrawString((tlm.Property as TLNumberTrans).Number1.ToString(), new Font("Arial", 18), Brushes.White, new Point(pt - 30, Y));
                }


                else
                {
                    g.FillRectangle(Brushes.Yellow, new Rectangle(pf, Y, pt - pf, height));
                    g.DrawRectangle(OutlinePen, new Rectangle(pf, Y, pt - pf, height));
                }
            }
        }

        public void UpdateTimeLine()
        {
            RearangeList();
            Invalidate();
        }
        public void UpdateIndicatorPos()
        {
            if (Form1.Project != null)
            {
                int mpx = GetPixel(Form1.Project.m.BeatPos);
                if (mpx < sideWidth || mpx > Width - 20) mpx = -20;
                pictureBox1.Location = new Point(mpx, 0);
                pictureBox1.Size = new Size(3, Height - 20);
            }
        }

        private void TimeLineControl_Resize(object sender, EventArgs e)
        {
            RecalculateBeatSpacing();
        }
        private void TimeLineControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.X < sideWidth)
            {
                int totalHeight = -YOfset + 20;
                int posY = e.Y;
                int posX = e.X;

                int itemIndex = 0;
                foreach (TLItem item in ListOfItems)
                {
                    if (posY >= totalHeight && posY < totalHeight + item.Height)
                    {
                        if (posX > 30 && posX < 60 && posY > totalHeight + 10 && posY < totalHeight + 20)
                        {
                            float mult = 0.05f;
                            if (ModifierKeys.HasFlag(Keys.Shift)) mult = 0.25f;
                            Form1.Project.Tracks[item.Index].Volume += e.Delta / 120f * mult;
                        }
                        if (posX > 30 && posX < 60 && posY > totalHeight + 20 && posY < totalHeight + 30) Form1.Project.Tracks[item.Index].Pan += e.Delta / 120f * 0.05f;
                        break;
                    }
                    totalHeight += (int)item.Height;
                    itemIndex++;
                }
            }
            else
            {
                if (ModifierKeys.HasFlag(Keys.Shift))
                {
                    int pv = hScrollBar1.Value + hScrollBar1.LargeChange * (int)(e.Delta / 120f);
                    if (pv <= 0) hScrollBar1.Value = 0;
                    else if (pv > hScrollBar1.Maximum) hScrollBar1.Value = hScrollBar1.Maximum;
                    else hScrollBar1.Value = pv;
                }
                else if (ModifierKeys.HasFlag(Keys.Control))
                {
                    int pv = vScrollBar1.Value + vScrollBar1.LargeChange * (int)(e.Delta / 120f);
                    if (pv <= 0) vScrollBar1.Value = 0;
                    else if (pv > vScrollBar1.Maximum) vScrollBar1.Value = vScrollBar1.Maximum;
                    else vScrollBar1.Value = pv;
                }
                else if (keyVdown)
                {
                    if (selectedMediaStyle == SelectedMediaStyle.Single)
                    {
                          Form1.Project.ProjectMedia[SingleSelectedMedia].Volume+=0.1f * (int)(e.Delta / 120f);
                    }
                    // Sube sube sube,sube  el volumen
                }
                else
                {
                    if (Form1.Project.m.BeatPos == 0)
                    {
                        ViewPortBeats *= (float)Math.Pow(1.05f, e.Delta / 120f);
                    }
                    else
                    {
                        float mid = Form1.Project.m.BeatPos;
                        ViewPortBeats *= (float)Math.Pow(1.05f, e.Delta / 120f);
                        viewPortStart = mid - viewPortBeats / 2;
                        if (viewPortStart < 0) viewPortStart = 0;

                        hScrollBar1.Value = (int)(ViewPortStart * 100000f);
                    }
                    Invalidate();
                }
            }
            Invalidate();
        }
        bool keyVdown = false;

        private void RecalculateBeatSpacing()
        {
            float count = Width - sideWidth;
            if (count <= 0) SignificantSpacingCount = 0;
            else
            {
                count /= labelSpacing;

                float aproxLenght = ViewPortBeats / count;
                aproxLenght = (float)Math.Log(aproxLenght, 2);
                aproxLenght = (float)Math.Round(aproxLenght);
                SignificantBeatSpacingDenominator = (int)Math.Pow(2, -aproxLenght);
                SignificantBeatSpacing = (float)Math.Pow(2, aproxLenght);

                SignificantSpacingCount = (int)(ViewPortBeats / SignificantBeatSpacing);

                int h0 = (int)((viewPortBeats / Width) * 3000000f);
                hScrollBar1.LargeChange = h0 > 3 ? h0 : 4;
                hScrollBar1.SmallChange = hScrollBar1.LargeChange / 10 + 1;

                SnapBeatUnits = (int)(192 * SignificantBeatSpacing) / 4; //nebo 24
            }

            MediaSideBeat = viewPortBeats * (15f / (Width - sideWidth));
        }
        private void RecalculateRangeLimit()
        {
            hScrollBar1.Maximum = MaxBeat * 100000;
        }
        private void RecalculateTotalHeight()
        {
            TotalHeight = 0;
            foreach (TLItem item in ListOfItems) TotalHeight += (int)item.Height;

            if (TotalHeight - Height + 20  + 200 < 0) vScrollBar1.Maximum = 0;
            else vScrollBar1.Maximum = TotalHeight - this.Height + 200;
        }
        public void RearangeList()
        {
            ListOfItems.Clear();
            ListOfAddTo.Clear();

            uint index = 1;
            foreach (uint u in Form1.Project.TrackList)
            {
                ListOfItems.Add(new TLItem() { Height = 40, Index = u, Type = 0 });
                ListOfAddTo.Add(index);

                if (ShowNT)
                {
                    List<uint> numberTracks = TrackHelper.GetNumberTracks(u);

                    foreach (uint v in numberTracks)
                    {
                        ListOfItems.Add(new TLItem() { Height = 40, Index = v, Type = 1 });
                        ListOfAddTo.Add(uint.MaxValue);
                    }
                }
                index++;
            }

            RecalculateTotalHeight();
        }
        private bool ShowNT = false;
        public void ToggleNT()
        {
            ShowNT = !ShowNT;
            RearangeList();
            Invalidate();
        }

        private List<TLItem> ListOfItems = new List<TLItem>();
        private List<uint> ListOfAddTo = new List<uint>();

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            ViewPortStart = hScrollBar1.Value / 100000f;
            Invalidate();
        }
        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            YOfset = vScrollBar1.Value;
            Invalidate();
        }

        // MANIPULACE
        private enum MouseAction { None, DragLeftTLM, DragRightTLM, MoveTLM }
        private MouseAction ma = MouseAction.None;
        private uint TLMEdited = uint.MaxValue;
        private ulong OrigState0 = 0; //FROM
        private ulong OrigState1 = 0; //LEN
        private ulong OrigState2 = 0; //END
        private ulong OrigState3 = 0; //IN
        private float OrigState4 = 0; //SIN
        private uint OrigState5 = 0; //TIMEBASE
        private float MouseBeatOn = 0;

        private int lastX = 0;
        private int lastY = 0;
        private void TimeLineControl_MouseDown(object sender, MouseEventArgs e)
        {
            int totalHeight = -YOfset + 20;
            int posY = e.Y;
            int posX = e.X;

            int itemIndex = 0;
            if (e.Y < 20)
            {
                Form1.Project.m.BeatPos = (int)GetBeat(posX);
            }
            else if (!Form1.Project.m.Playing) foreach (TLItem item in ListOfItems)
            {
                TrackIndexRightClick = item.Index;
                TrackTypeRightClick = (byte)item.Type;

                if (e.Button == MouseButtons.Left && ListOfAddTo[itemIndex] != uint.MaxValue && posX > 5 && posX < 21 && posY > totalHeight + item.Height - 8 && posY < totalHeight + item.Height + 8)
                {
                    VideoTrack vt = new VideoTrack();
                    uint u = Form1.Project.AddTrack(vt);

                    Form1.Project.TrackList.Insert((int)ListOfAddTo[itemIndex], u);
                    Form1.Project.m.AddNode(u);
                    RearangeList();
                    Invalidate();
                    break;
                }
                else if (e.Button == MouseButtons.Left && ListOfAddTo[itemIndex] != uint.MaxValue && posX > 25 && posX < 41 && posY > totalHeight + item.Height - 8 && posY < totalHeight + item.Height + 8)
                {
                    if (copiedTrackStyle == SelectedMediaStyle.SingleTrack)
                    {
                        VideoTrack tlt = (VideoTrack)Form1.Project.Tracks[SingleCopiedTrack].Clone();
                        uint u = Form1.Project.AddTrack(tlt);

                        Form1.Project.TrackList.Insert((int)ListOfAddTo[itemIndex], u);
                        Form1.Project.m.AddNode(u);
                        RearangeList();
                        Invalidate();
                        break;
                    }
                    if (copiedTrackStyle == SelectedMediaStyle.STCut)
                    {
                        if (Form1.Project.TrackList.Contains(SingleCopiedTrack))
                        {
                            int indexOfTrack = Form1.Project.TrackList.IndexOf(SingleCopiedTrack);
                            int pasteIndex = (int)ListOfAddTo[itemIndex];

                            if (pasteIndex > indexOfTrack)
                            {
                                pasteIndex--;
                            }
                            Form1.Project.TrackList.Remove(SingleCopiedTrack);
                            Form1.Project.TrackList.Insert(pasteIndex, SingleCopiedTrack);

                            RearangeList();
                            Invalidate();
                        }
                        SingleCopiedTrack = uint.MaxValue;
                        break;
                    }
                }

                else if (posY >= totalHeight && posY < totalHeight + item.Height)
                {
                    if (item.Type == 0 || item.Type == 1)
                    {
                        if (posX < sideWidth)
                        {
                            //tlacitka?
                            if (e.Button == MouseButtons.Right)
                            {
                                if (TrackTypeRightClick == 0) GetTrackMenu(item.Index).Show(PointToScreen(e.Location));
                            }
                        }
                        else
                        {
                            float timeBeatMouse = GetBeat(posX);
                            MouseBeatOn = timeBeatMouse;

                            foreach (uint u in (TrackTypeRightClick == 0 ? Form1.Project.Tracks[item.Index].Media : Form1.Project.NumberTracks[item.Index].Media))
                            {
                                TimelineMedia tlm = Form1.Project.ProjectMedia[u];
                                if (tlm.TimeFrom / 192f <= timeBeatMouse && tlm.TimeEnd / 192f > timeBeatMouse)
                                {
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        TLMEdited = u;
                                        if (timeBeatMouse - tlm.TimeFrom / 192f < MediaSideBeat) ma = MouseAction.DragLeftTLM;
                                        else if (tlm.TimeEnd / 192f - timeBeatMouse < MediaSideBeat) ma = MouseAction.DragRightTLM;
                                        else ma = MouseAction.MoveTLM;
                                        OrigState0 = tlm.TimeFrom;
                                        OrigState1 = tlm.TimeLenght;
                                        OrigState2 = tlm.TimeEnd;
                                        OrigState3 = tlm.BeatIn;
                                        OrigState4 = tlm.SecondsIn;
                                        OrigState5 = item.Index;

                                        SingleSelectedMedia = u;
                                        selectedMediaStyle = SelectedMediaStyle.Single;
                                    }
                                    else if (e.Button == MouseButtons.Right)
                                    {
                                        GetMediaStrip(u).Show(PointToScreen(e.Location));
                                    }
                                    else if (e.Button == MouseButtons.Middle)
                                    {
                                        SingleCopiedMedia = u;
                                        copiedMediaStyle = SelectedMediaStyle.Single;
                                        placingMediaStyle = MediaPlaceStyle.FromCopiedSingle;
                                    }
                                    break;
                                }
                            }
                            if (ma == MouseAction.None && e.Button == MouseButtons.Left)
                            {
                                if (placingMediaStyle == MediaPlaceStyle.FromCopiedSingle && SingleCopiedMedia != uint.MaxValue)
                                {
                                    TimelineMedia tlmc = (TimelineMedia)Form1.Project.ProjectMedia[SingleCopiedMedia].Clone();
                                    tlmc.TimeFrom = (ulong)((long)Math.Round(timeBeatMouse * 192f / SnapBeatUnits) * SnapBeatUnits);
                                    uint num = Form1.Project.AddMedia(tlmc);

                                    if (TrackTypeRightClick == 0) Form1.Project.Tracks[item.Index].Media.Add(num);
                                    if (TrackTypeRightClick == 1) Form1.Project.NumberTracks[item.Index].Media.Add(num);
                                }
                                if (placingMediaStyle == MediaPlaceStyle.FromMediaLibrary)
                                {
                                    TimelineMediaType tlmt = parentForm.getTLM();

                                    bool beatSync = false;
                                    if (tlmt == null) break;
                                    if (tlmt is TLPattern) beatSync = true;
                                    if (tlmt is TLSample)
                                    {
                                        if (Form1.Project.ProjectSamples[(tlmt as TLSample).Sample].ips == null) beatSync = false;
                                        else beatSync = Form1.Project.ProjectSamples[(tlmt as TLSample).Sample].ips.BeatSync;
                                    }

                                    TimelineMedia tlmc = new TimelineMedia(tlmt, false, 0);
                                    tlmc.TimeFrom = (ulong)((long)Math.Round(timeBeatMouse * 192f / SnapBeatUnits) * SnapBeatUnits);
                                    tlmc.TimeLenght = (ulong)SnapBeatUnits;
                                    uint num = Form1.Project.AddMedia(tlmc);

                                    if (TrackTypeRightClick == 0) Form1.Project.Tracks[item.Index].Media.Add(num);
                                    if (TrackTypeRightClick == 1) Form1.Project.NumberTracks[item.Index].Media.Add(num);
                                }
                            }
                        }
                    }
                    break;
                }
                totalHeight += (int)item.Height;
                itemIndex++;
            }
        }
        private bool AltDown = false;
        private void TimeLineControl_MouseMove(object sender, MouseEventArgs e)
        {
            lastX = e.X;
            lastY = e.Y;
            if (TLMEdited != uint.MaxValue)
            {
                if (ma == MouseAction.MoveTLM)
                {
                    float tbm = GetBeat(e.X);
                    float diff = tbm - MouseBeatOn;

                    float change = diff * 192f;
                    if (!AltDown) change = (float)Math.Round(change / SnapBeatUnits) * SnapBeatUnits;

                    if ((long)OrigState0 + (long)(change) >= 0) Form1.Project.ProjectMedia[TLMEdited].TimeFrom = (ulong)((long)OrigState0 + (change));
                }
                else if (ma == MouseAction.DragRightTLM)
                {
                    float tbm = GetBeat(e.X);
                    float diff = tbm - MouseBeatOn;

                    float change = diff * 192f;
                    if (!AltDown) change = (float)Math.Round(change / SnapBeatUnits) * SnapBeatUnits;

                    if ((long)OrigState1 + (long)(change) >= 0) Form1.Project.ProjectMedia[TLMEdited].TimeLenght = (ulong)((long)OrigState1 + (change));
                    else Form1.Project.ProjectMedia[TLMEdited].TimeLenght = 0;
                }
                else if (ma == MouseAction.DragLeftTLM)
                {
                    float tbm = GetBeat(e.X);
                    float diff = tbm - MouseBeatOn;
                    if (diff * 192f >= OrigState1) diff = (OrigState1 / 192f);
                    if ((long)OrigState0 + (long)(diff * 192f) < 0) diff = -(float)OrigState0 / 192f;

                    float change = diff * 192f;
                    if (!AltDown) change = (float)Math.Round(change / SnapBeatUnits) * SnapBeatUnits;

                    //------------------------
                    Form1.Project.ProjectMedia[TLMEdited].BeatIn = (ulong)((long)OrigState3 + (long)(change));
                    Form1.Project.ProjectMedia[TLMEdited].SecondsIn = OrigState4 + Form1.Project.GetBeatDurationSec(OrigState0) * change / 192f;

                    Form1.Project.ProjectMedia[TLMEdited].TimeFrom = (ulong)((long)OrigState0 + (long)(change));
                    Form1.Project.ProjectMedia[TLMEdited].TimeLenght = (ulong)((long)OrigState1 - (long)(change));
                }

                Invalidate();
            }
        }
        private void TimeLineControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (TrackTypeRightClick == 0) if ((ma == MouseAction.DragRightTLM || ma == MouseAction.DragLeftTLM || ma == MouseAction.MoveTLM) &&
                (Form1.Project.ProjectMedia[TLMEdited].TimeLenght == 0 || Form1.Project.ProjectMedia[TLMEdited].TimeLenght > 10000000000000000000 || Form1.Project.ProjectMedia[TLMEdited].TimeFrom > 10000000000000000000))
            {
                Form1.Project.Tracks[OrigState5].Media.Remove(TLMEdited);
            }
            if (TrackTypeRightClick == 1) if ((ma == MouseAction.DragRightTLM || ma == MouseAction.DragLeftTLM || ma == MouseAction.MoveTLM) &&
                (Form1.Project.ProjectMedia[TLMEdited].TimeLenght == 0 || Form1.Project.ProjectMedia[TLMEdited].TimeLenght > 10000000000000000000 || Form1.Project.ProjectMedia[TLMEdited].TimeFrom > 10000000000000000000))
                {
                    Form1.Project.NumberTracks[OrigState5].Media.Remove(TLMEdited);
                }

            if (ma != MouseAction.None) foreach (KeyValuePair<uint, VideoTrack> k in Form1.Project.Tracks) for (int i = 0; i < k.Value.Media.Count; i++)
            {
                TimelineMedia tlm = Form1.Project.ProjectMedia[k.Value.Media[i]];
                        if (tlm.TimeFrom > 9000000000000000000 || tlm.TimeLenght > 9000000000000000000 || tlm.TimeLenght == 0)
                        {
                            k.Value.Media.RemoveAt(i);
                            break;
                        }
            }
            //9223372036854859685

            ma = MouseAction.None;
            TLMEdited = uint.MaxValue;

            Invalidate();
        }

        private uint TrackIndexRightClick = 0;
        private byte TrackTypeRightClick = 0;

        // Nahradit defaultem
        private ContextMenuStrip GetTrackMenu(uint Track)
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("Copy media", null, (s, e) => {
                SingleCopiedMedia = uint.MaxValue;
                copiedMediaStyle = SelectedMediaStyle.SingleTrack;
                GroupOfCopiedMedia.Clear();
                foreach(uint u in Form1.Project.Tracks[Track].Media)
                {
                    GroupOfCopiedMedia.Add(u);
                }
            });
            cms.Items.Add("Copy settings", null, (s, e) =>
            {
                SingleCopiedSettings = Track;

                SingleCopiedMedia = uint.MaxValue;
                copiedMediaStyle = SelectedMediaStyle.SingleTrack;
                GroupOfCopiedMedia.Clear();
                foreach (uint u in Form1.Project.Tracks[Track].Media)
                {
                    GroupOfCopiedMedia.Add(u);
                }

            });
            cms.Items.Add("Copy contents + settings", null, (s, e) =>
            {
                SingleCopiedSettings = Track;
            });
            cms.Items.Add("Copy track", null, (s, e) =>
            {
                copiedTrackStyle = SelectedMediaStyle.SingleTrack;
                SingleCopiedTrack = Track;
                Invalidate();
            });
            cms.Items.Add("Cut track", null, (s, e) =>
            {
                copiedTrackStyle = SelectedMediaStyle.STCut;
                SingleCopiedTrack = Track;
                Invalidate();
            });
            cms.Items.Add("Delete track", null, (s, e) =>
            {
                Form1.Project.TrackList.Remove(Track);
                foreach (MixerLayer ml in Form1.Project.m.Layers)
                {
                    int index = 0;
                    int found = -1;
                    foreach (var t in ml.ListOfLinks)
                    {
                        if (t.Links.Item1 == Track)
                        {
                            found = index;
                            break;
                        }
                        index++;
                    }
                    if (found != -1) ml.ListOfLinks.RemoveAt(found);
                }
                RearangeList();
                Invalidate();
            });

            cms.Items.Add("Paste-add media", null, (s, e) =>
            {
                foreach (uint u in GroupOfCopiedMedia)
                {
                    TimelineMedia media = (TimelineMedia)Form1.Project.ProjectMedia[u].Clone();
                    uint num = Form1.Project.AddMedia(media);
                    Form1.Project.Tracks[Track].Media.Add(num);
                }
                Invalidate();
            }
            );
            cms.Items.Add("Paste-replace media", null, (s, e) =>
            {
                Form1.Project.Tracks[Track].Media.Clear();
                foreach (uint u in GroupOfCopiedMedia)
                {
                    TimelineMedia media = (TimelineMedia)Form1.Project.ProjectMedia[u].Clone();
                    uint num = Form1.Project.AddMedia(media);
                    Form1.Project.Tracks[Track].Media.Add(num);
                }
                Invalidate();
            }
            );
            cms.Items.Add("Paste settings", null, (s, e) => {
                if (SingleCopiedSettings != uint.MaxValue)
                {
                    if (Form1.Project.Tracks.ContainsKey(SingleCopiedSettings))
                    {
                        Form1.Project.Tracks[Track].PasteSettings(Form1.Project.Tracks[SingleCopiedSettings]);
                    }
                }
                Invalidate();
            }
            );
            cms.Items.Add("Paste-replace contents + settings", null, (s, e) =>
            {
                // SETTINGS
                Form1.Project.Tracks[Track].Media.Clear();
                foreach (uint u in GroupOfCopiedMedia)
                {
                    TimelineMedia media = (TimelineMedia)Form1.Project.ProjectMedia[u].Clone();
                    uint num = Form1.Project.AddMedia(media);
                    Form1.Project.Tracks[Track].Media.Add(num);
                }
            if (SingleCopiedSettings != uint.MaxValue)
                {
                    if (Form1.Project.Tracks.ContainsKey(SingleCopiedSettings))
                    {
                        Form1.Project.Tracks[Track].PasteSettings(Form1.Project.Tracks[SingleCopiedSettings]);
                    }
                }
                Invalidate();
            }
            );

            cms.Items.Add("Edit AFX", null, (s, e) => { Form1.Project.Tracks[Track].ShowAFXForm(); });
            cms.Items.Add("Edit VFX", null, (s, e) => { Form1.Project.Tracks[Track].ShowVFXForm(); });

            return cms;
        }
        private ContextMenuStrip GetMediaStrip(uint Media)
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            
            cms.Items.Add("Delete", null, (s, e) => {
                uint M = 0;
                long T = -1;
                uint TR = 0;
                int TP = 0;

                int totalHeight = -YOfset + 20;

                int itemIndex = 0;
                foreach (TLItem item in ListOfItems)
                {
                    TR = item.Index;
                    TP = (byte)item.Type;
                    if (item.Type == 0 || item.Type == 1)
                    {
                        if ((TP == 0 ? Form1.Project.Tracks[item.Index].Media : Form1.Project.NumberTracks[item.Index].Media).Contains(Media))
                            (TP == 0 ? Form1.Project.Tracks[item.Index].Media : Form1.Project.NumberTracks[item.Index].Media).Remove(Media);
                    }
                    
                    totalHeight += (int)item.Height;
                    itemIndex++;
                }
                Invalidate();
            });
            cms.Items.Add("---------------", null, (s, e) => { });

            if (Form1.Project.ProjectMedia[Media].Property is TLSample) GetCMSTLSample(Media, cms);

            return cms;
        }
        private void GetCMSBySelectedGroup(ContextMenuStrip cms)
        {
            bool isAllDefaultPitch = true;
            bool isAllSameSample = true;
            bool isAllAnySample = true;
            long Snumber = -1;

            foreach (uint u in GroupOfSelectedMedia)
            {
                if (Form1.Project.ProjectMedia[u] == null)
                {
                    isAllDefaultPitch = false;
                    isAllAnySample = false;
                    isAllSameSample = false;
                }
                else
                {
                    if (!(Form1.Project.ProjectMedia[u].Property is TLSample))
                    {
                        isAllDefaultPitch = false;
                        isAllAnySample = false;
                        isAllSameSample = false;
                    }
                    else
                    {
                        TLSample tls = Form1.Project.ProjectMedia[u].Property as TLSample;
                        if (Form1.Project.ProjectSamples?[tls.Sample].ips != null)
                        {
                            if (!(Form1.Project.ProjectSamples?[tls.Sample].ips is VideoSourceS))
                            {
                                isAllDefaultPitch = false;
                            }
                        }
                        else
                        {
                            isAllDefaultPitch = false;
                            isAllAnySample = false;
                        }
                        if (Snumber == -1)
                        {
                            Snumber = tls.Sample;
                        }
                        else if (tls.Sample != Snumber) isAllSameSample = false;
                    }
                }
            }

            // PRIDAT
        }
        private void GetCMSTLSample(uint Media, ContextMenuStrip cms)
        {
            uint num = (Form1.Project.ProjectMedia[Media].Property as TLSample).Sample;
            if (Form1.Project.ProjectSamples.ContainsKey(num)) if (Form1.Project.ProjectSamples[num].ips != null)
                {
                    if (Form1.Project.ProjectSamples[num].ips is VideoSourceS)
                    {
                        cms.Items.Add("Quick Correct (Default)", null, (s, e) =>
                        {
                            CorrectedPitchSample cps = new CorrectedPitchSample(PitchHelper.GeneratePTC(PitchHelper.GetMonoL(Form1.Project.ProjectSamples[num].ips.GetReader(Form1.Project.ProjectMedia[Media].SecondsIn), 24000)), 440f);
                            uint u = Form1.Project.AddSample(new SampleAV() { ivs = new QuickLoadVideoSample(num, 15, true, Form1.Project.ProjectMedia[Media].SecondsIn, 0.5f), ips = cps });
                            (Form1.Project.ProjectMedia[Media].Property as TLSample).Sample = u;

                            Form1.Project.ProjectMedia[Media].SecondsIn = 0;
                            Form1.Project.ProjectMedia[Media].BeatIn = 0;

                            Invalidate();
                        }
                        );
                        cms.Items.Add("Quick Correct (Formant-P)", null, (s, e) =>
                        {
                            Tuple<List<float[]>, List<int>> tt = PitchHelper.GenerateFMT(PitchHelper.GetMonoL(Form1.Project.ProjectSamples[num].ips.GetReader(Form1.Project.ProjectMedia[Media].SecondsIn), 24000));
                            GranuleSample cps = new GranuleSample(tt.Item1, tt.Item2, 0.5f, 440f);
                            uint u = Form1.Project.AddSample(new SampleAV() { ivs = new QuickLoadVideoSample(num, 15, true, Form1.Project.ProjectMedia[Media].SecondsIn, 0.5f), ips = cps });
                            (Form1.Project.ProjectMedia[Media].Property as TLSample).Sample = u;

                            Form1.Project.ProjectMedia[Media].SecondsIn = 0;
                            Form1.Project.ProjectMedia[Media].BeatIn = 0;

                            Invalidate();
                        });
                        cms.Items.Add("Quick PAD[20]", null, (s, e) =>
                        {
                            PitchBufferSample2 pbs = new PitchBufferSample2(PitchHelper.GeneratePAD(PitchHelper.GetMonoL(Form1.Project.ProjectSamples[num].ips.GetReader(Form1.Project.ProjectMedia[Media].SecondsIn), 48000),200, 20f), 220);
                            pbs.A = 0.03f;
                            pbs.D = 0.0f;
                            pbs.S = 1f;
                            pbs.R = 0.03f;
                            uint u = Form1.Project.AddSample(new SampleAV() { ivs = new QuickLoadVideoSample(num, 15, true, Form1.Project.ProjectMedia[Media].SecondsIn, 0.5f) , ips = pbs });
                            (Form1.Project.ProjectMedia[Media].Property as TLSample).Sample = u;

                            Form1.Project.ProjectMedia[Media].SecondsIn = 0;
                            Form1.Project.ProjectMedia[Media].BeatIn = 0;

                            Invalidate();
                        });
                        cms.Items.Add("Quick PAD[4]", null, (s, e) =>
                        {
                            PitchBufferSample2 pbs = new PitchBufferSample2(PitchHelper.GeneratePAD(PitchHelper.GetMonoL(Form1.Project.ProjectSamples[num].ips.GetReader(Form1.Project.ProjectMedia[Media].SecondsIn), 48000), 200, 4f), 220);
                            pbs.A = 0.03f;
                            pbs.D = 0.0f;
                            pbs.S = 1f;
                            pbs.R = 0.03f;
                            uint u = Form1.Project.AddSample(new SampleAV() { ivs = new QuickLoadVideoSample(num, 15, true, Form1.Project.ProjectMedia[Media].SecondsIn, 0.5f), ips = pbs });
                            (Form1.Project.ProjectMedia[Media].Property as TLSample).Sample = u;

                            Form1.Project.ProjectMedia[Media].SecondsIn = 0;
                            Form1.Project.ProjectMedia[Media].BeatIn = 0;

                            Invalidate();
                        });
                        cms.Items.Add("Open Pitch Editor", null, (s, e) =>
                        {
                            SampleCutter sc = new SampleCutter(Form1.Project.ProjectSamples[num].ips as VideoSourceS, num, Form1.Project.ProjectMedia[Media].SecondsIn, TrackIndexRightClick, Media);
                            sc.Show();
                        });
                        cms.Items.Add("Open Perc Editor", null, (s, e) =>
                        {
                             PercMakeForm sc = new PercMakeForm(Form1.Project.ProjectSamples[num].ips as VideoSourceS, num, Form1.Project.ProjectMedia[Media].SecondsIn, TrackIndexRightClick, Media);
                            sc.Show();
                        });
                    }
                    if (Form1.Project.ProjectSamples[num].ips is CorrectedPitchSample)
                    {
                        cms.Items.Add("Open CPS Editor", null, (s, e) =>
                        {
                            CorrectedPitchSampleEditor gse = new CorrectedPitchSampleEditor(Form1.Project.ProjectSamples[num].ips as CorrectedPitchSample, num);
                            gse.Show();
                        });
                    }
                    if (Form1.Project.ProjectSamples[num].ips is GranuleSample)
                    {
                        cms.Items.Add("Open GS Editor", null, (s, e) =>
                        {
                            GranuleSampleEditor gse = new GranuleSampleEditor(Form1.Project.ProjectSamples[num].ips as GranuleSample, num);
                            gse.Show();
                        });
                    }
                    if (Form1.Project.ProjectSamples[num].ips is AudioCutSample)
                    {
                        cms.Items.Add("Open ACS Editor", null, (s, e) =>
                        {
                            AudioCutSampleEditor acse = new AudioCutSampleEditor(Form1.Project.ProjectSamples[num].ips as AudioCutSample, num);
                            acse.Show();
                        });
                    }
                    if (Form1.Project.ProjectSamples[num].ips is PitchBufferSample2)
                    {
                        cms.Items.Add("Open PBS Editor", null, (s, e) =>
                        {
                            PitchBufferSampleEditor pbse = new PitchBufferSampleEditor(Form1.Project.ProjectSamples[num].ips as PitchBufferSample2, num);
                            pbse.Show();
                        });
                    }
                    // JINE DRUHY
                }
            cms.Items.Add("Flip Horizontal", null, (s, e) =>
            {
                bool found = false;
                for (int i = 0; i < (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.Count; i++)
                {
                    if ((Form1.Project.ProjectMedia[Media].Property as TLSample).VFX[i] is VFX_S_UVFlipX)
                    {
                        (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                if (!found) (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.Insert(0, new VFX_S_UVFlipX());

                Invalidate();
            });
            cms.Items.Add("Flip Vertical", null, (s, e) =>
            {
                bool found = false;
                for (int i = 0; i < (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.Count; i++)
                {
                    if ((Form1.Project.ProjectMedia[Media].Property as TLSample).VFX[i] is VFX_S_UVFlipY)
                    {
                        (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.RemoveAt(i);
                        found = true;
                        break;
                    }
                }
                if (!found) (Form1.Project.ProjectMedia[Media].Property as TLSample).VFX.Insert(0, new VFX_S_UVFlipY());

                Invalidate();
            });
        }

        private void SliceMedia(uint Media, uint Time, uint Track, byte TrackType)
        {
            Time = (uint)((long)Math.Round(((double)Time / SnapBeatUnits)) * SnapBeatUnits);
            if (Time > Form1.Project.ProjectMedia[Media].TimeFrom && Time < Form1.Project.ProjectMedia[Media].TimeFrom + Form1.Project.ProjectMedia[Media].TimeLenght)
            {
                ulong beatin = Time - Form1.Project.ProjectMedia[Media].TimeFrom;

                TimelineMedia tlm2 = (TimelineMedia)Form1.Project.ProjectMedia[Media].Clone();
                tlm2.TimeFrom = Time;
                tlm2.TimeLenght = Form1.Project.ProjectMedia[Media].TimeLenght - (Time - Form1.Project.ProjectMedia[Media].TimeFrom);
                Form1.Project.ProjectMedia[Media].TimeLenght = Time - Form1.Project.ProjectMedia[Media].TimeFrom;

                if (tlm2.Flags0.HasFlag(MediaFlags.UseBeatIn))
                {
                    tlm2.BeatIn += beatin;
                }
                else
                {
                    tlm2.SecondsIn += Form1.Project.GetBeatDurationSec(0) * (beatin / 192f);
                }

                uint u = Form1.Project.AddMedia(tlm2);

                if (TrackType == 0)
                {
                    Form1.Project.Tracks[Track].Media.Add(u);
                }
                else if (TrackType == 1)
                {
                    Form1.Project.NumberTracks[Track].Media.Add(u);
                }
            }
        }

        // Placing
        public MediaPlaceStyle placingMediaStyle = MediaPlaceStyle.None;
        public enum MediaPlaceStyle { None, FromCopiedSingle, FromMediaLibrary }

        // Upravy
        private uint SingleSelectedMedia = uint.MaxValue;
        private List<uint> GroupOfSelectedMedia = new List<uint>();
        private SelectedMediaStyle selectedMediaStyle = SelectedMediaStyle.None;

        private uint SingleCopiedMedia = uint.MaxValue;
        private List<uint> GroupOfCopiedMedia = new List<uint>();
        private SelectedMediaStyle copiedMediaStyle = SelectedMediaStyle.None;

        // Track upravy
        private uint SingleSelectedTrack = uint.MaxValue;
        private List<uint> GroupOfSelectedTrack = new List<uint>();
        private SelectedMediaStyle selectedTrackStyle = SelectedMediaStyle.None;

        private uint SingleCopiedTrack = uint.MaxValue;
        private List<uint> GroupOfCopiedTrack = new List<uint>();
        private SelectedMediaStyle copiedTrackStyle = SelectedMediaStyle.None;

        private uint SingleCopiedSettings = uint.MaxValue;

        public void KeyUpMedia(Keys k)
        {
            if (k == Keys.V) keyVdown = false;
            if (selectedMediaStyle == SelectedMediaStyle.Single)
            {
                if (k == Keys.Menu)
                {
                    AltDown = false;
                }
                if (k == Keys.Oemplus)
                {
                    if (Form1.Project.ProjectMedia.ContainsKey(SingleSelectedMedia))
                    {
                        Form1.Project.ProjectMedia[SingleSelectedMedia].Pitch++;
                    }
                }
                if (k == Keys.OemMinus)
                {
                    if (Form1.Project.ProjectMedia.ContainsKey(SingleSelectedMedia))
                    {
                        Form1.Project.ProjectMedia[SingleSelectedMedia].Pitch--;
                    }
                }
                
            }
            if (k == Keys.C)
            {
                uint M = 0;
                long T = -1;
                uint TR = 0;
                int TP = 0;

                int totalHeight = -YOfset + 20;
                int posY = lastY;
                int posX = lastX;

                int itemIndex = 0;
                foreach (TLItem item in ListOfItems)
                {
                    TR = item.Index;
                    TP = (byte)item.Type;

                    if (posY >= totalHeight && posY < totalHeight + item.Height)
                    {
                        if (item.Type == 0 || item.Type == 1)
                        {
                            T = (long)(GetBeat(posX) * 192f);
                            foreach (uint u in (TP == 0 ? Form1.Project.Tracks[item.Index].Media : Form1.Project.NumberTracks[item.Index].Media))
                            {
                                TimelineMedia tlm = Form1.Project.ProjectMedia[u];
                                if ((long)tlm.TimeFrom <= T && (long)tlm.TimeEnd > T)
                                {
                                    M = u;
                                }
                            }
                        }
                        break;
                    }
                    totalHeight += (int)item.Height;
                    itemIndex++;
                }

                if (T != -1) SliceMedia(M, (uint)T, TR, (byte)TP);
            }
            Invalidate();
        }
        public void KeyDownMedia(Keys k)
        {
            if (k == Keys.V) keyVdown = true;
            if (k == Keys.Menu)
            {
                AltDown = true;
            }
        }

        private enum SelectedMediaStyle { None, Single, SingleTrack, MultiTrack, SingleCut, STCut, MTCut }
    }

    public struct TLItem
    {
        public uint Height;

        public uint Index; //-
        public uint Type; //0
        public string Name;

        public static TLItem FromTrackIndex(uint index) => new TLItem() { Height = 40, Index = index, Type = 0, Name = null };
        public static TLItem AddLabel(string s) => new TLItem() { Height = 20, Index = 0, Type = 1, Name = s };
    }
}
