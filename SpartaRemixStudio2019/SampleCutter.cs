using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public partial class SampleCutter : Form
    {
        public SampleCutter(VideoSourceS source, uint sourceIndex, float timeOffset, uint timeLineTrack, uint timeLineMedia)
        {
            InitializeComponent();

            this.source = source;
            this.timeOffset = timeOffset;
            this.timeLineTrack = timeLineTrack;
            this.timeLineMedia = timeLineMedia;
            this.sourceIndex = sourceIndex;

            LoadTheSource();

            pictureBox1.Invalidate();
            
        }
        private void LoadTheSource()
        {
            audio25 = new float[120000];
            float[] audioTemp = new float[240000];
            IPitchReader ipr = (source as IPitchSample).GetReader(timeOffset);
            ipr.ReadAdd(ref audioTemp, 1 / 2f, 1 / 2f);
            for (int i = 0; i < 120000; i++)
            {
                audio25[i] = (audioTemp[2 * i + 0] + audioTemp[2 * i + 1]) / 2;
            }
        }

        VideoSourceS source;
        float timeOffset;
        uint timeLineTrack;
        uint timeLineMedia;
        uint sourceIndex;

        float[] audio25 = new float[0];
        FloatArraySource fas;

        List<Tuple<int, float>>[] Peaks = new List<Tuple<int, float>>[0];
        int[] PeakSelected = new int[0];
        float[] outputSound;
        List<float[]> graunleBuffer = new List<float[]>();

        private void DebugPlay(float[] f)
        {
            if (fas != null)
            {
                Form1.so.providers.Remove(fas);
            }
            fas = new FloatArraySource(f, 48000);
            Form1.so.providers.Add(fas);
        }
        private void DebugStop()
        {
            if (fas != null)
            {
                Form1.so.providers.Remove(fas);
            }
            fas = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int from = (int)(markedFrom * audio25.Length / (float)pictureBox1.Width);
            int to = (int)(markedTo * audio25.Length / (float)pictureBox1.Width);
            if (to < from)
            {
                int pom = to;
                to = from;
                from = pom;
            }
            outputSound = new float[to - from];
            for (int i = 0; i < outputSound.Length; i++)
            {
                if (i + from < audio25.Length) outputSound[i] = audio25[i + from];
            }
            DebugPlay(outputSound);
        }
        private void button25_Click(object sender, EventArgs e)
        {
            int from = (int)(markedFrom * audio25.Length / (float)pictureBox1.Width);
            int to = (int)(markedTo * audio25.Length / (float)pictureBox1.Width);
            outputSound = new float[to - from];
            for (int i = 0; i < outputSound.Length; i++)
            {
                if (i + from < audio25.Length) outputSound[i] = audio25[i + from];
            }
            AudioCutSample gs = new AudioCutSample(outputSound, false, false,1, 0.03f,0,1,0.03f, false);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = gs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + from / 48000f, 1f) });
            PlaceOnTimeLine(u);
        }

        // GEN PAD
        private int smpsel = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            float det = 20f;
            float.TryParse(textBox2.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out det);
            outputSound = PitchHelper.GeneratePAD(audio25,smpsel, det);

            PitchBufferSample2 pbs = new PitchBufferSample2(outputSound, 220f,0.03f,0.01f,1,0.03f);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = pbs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + smpsel / 48000f, 0.5f) });
            PlaceOnTimeLine(u);
        }

        bool[] placedRight = new bool[0];

        private void DetermineCertainity()
        {
            placedRight = new bool[Peaks.Length];

            for (int i = 0; i < Peaks.Length; i++)
            {
                placedRight[i] = true;
                for (int j = Math.Max(0, i - 2); j <= i + 2 && j < Peaks.Length; j++)
                {
                    if (Math.Abs(PeakSelected[i] - PeakSelected[j]) > 10) placedRight[i] = false;
                }
            }
        }
        private void TryEnclosingHoles()
        {
            int moved = 1;
            while (moved > 0)
            {
                moved = 0;
                for (int i = 0; i < Peaks.Length; i++)
                {
                    if (!placedRight[i])
                    {
                        if (i > 0)
                        {
                            if (placedRight[i - 1])
                            {
                                if (Math.Abs(PeakSelected[i] - PeakSelected[i - 1]) <= 10)
                                {
                                    placedRight[i] = true;
                                    moved++;
                                }
                            }
                        }
                        if (i < Peaks.Length - 1)
                        {
                            if (placedRight[i + 1])
                            {
                                if (Math.Abs(PeakSelected[i] - PeakSelected[i + 1]) <= 10)
                                {
                                    placedRight[i] = true;
                                    moved++;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void TryFillingIntervals()
        {
            for (int i = 0; i < Peaks.Length; i++)
            {
                if (!placedRight[i])
                {
                    int startIndex = i;
                    for (int j = startIndex; j < Peaks.Length; j++)
                    {
                        if (placedRight[j])
                        {
                            ResolveInterval(startIndex, j - 1);
                            i = j - 1;
                            break;
                        }
                    }
                }
            }
        }
        private void ResolveInterval(int from, int to)
        {
            if (from == 0)
            {
                for (int i = to; i >= 0; i--)
                {
                    int index = -1;
                    float min = 999;
                    for (int j = 0; j < Peaks[i].Count; j++)
                    {
                        if (Math.Abs(Peaks[i][j].Item1 - PeakSelected[i + 1]) / Peaks[i][j].Item2 < min)
                        {
                            min = Math.Abs(Peaks[i][j].Item1 - PeakSelected[i + 1]) / Peaks[i][j].Item2;
                            index = Peaks[i][j].Item1;
                        }
                    }
                    if (index == -1) break;
                    PeakSelected[i] = index;
                    placedRight[i] = true;
                }
            }
            else if (to == Peaks.Length - 1)
            {
                for (int i = from; i < Peaks.Length; i++)
                {
                    int index = -1;
                    float min = 999;
                    for (int j = 0; j < Peaks[i].Count; j++)
                    {
                        if (Math.Abs(Peaks[i][j].Item1 - PeakSelected[i - 1]) / Peaks[i][j].Item2 < min)
                        {
                            min = Math.Abs(Peaks[i][j].Item1 - PeakSelected[i - 1]) / Peaks[i][j].Item2;
                            index = Peaks[i][j].Item1;
                        }
                    }
                    if (index == -1) break;
                    PeakSelected[i] = index;
                    placedRight[i] = true;
                }
            }
            else
            {
                while (from <= to)
                {
                    int index0 = -1;
                    float min0 = 999;
                    for (int j = 0; j < Peaks[from].Count; j++)
                    {
                        if (Math.Abs(Peaks[from][j].Item1 - PeakSelected[from - 1]) / Peaks[from][j].Item2 < min0)
                        {
                            min0 = Math.Abs(Peaks[from][j].Item1 - PeakSelected[from - 1]) / Peaks[from][j].Item2;
                            index0 = Peaks[from][j].Item1;
                        }
                    }

                    int index1 = -1;
                    float min1 = 999;
                    for (int j = 0; j < Peaks[to].Count; j++)
                    {
                        if (Math.Abs(Peaks[to][j].Item1 - PeakSelected[to + 1]) / Peaks[to][j].Item2 < min1)
                        {
                            min1 = Math.Abs(Peaks[to][j].Item1 - PeakSelected[to + 1]) / Peaks[to][j].Item2;
                            index1 = Peaks[to][j].Item1;
                        }
                    }

                    if (index1 == -1 && index0 == -1) break;
                    else if (index1 == -1 && index0 != -1)
                    {
                        PeakSelected[from] = index0;
                        placedRight[from] = true;
                        from++;
                    }
                    else if (index1 != -1 && index0 == -1)
                    {
                        PeakSelected[to] = index1;
                        placedRight[to] = true;
                        to--;
                    }
                    else
                    {
                        if (min0 >= min1)
                        {
                            PeakSelected[from] = index0;
                            placedRight[from] = true;
                            from++;
                        }
                        else
                        {
                            PeakSelected[to] = index1;
                            placedRight[to] = true;
                            to--;
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Peaks = PitchHelper.ExtractPitchInfo(audio25);
            PeakSelected = new int[Peaks.Length];
            for (int i = 0; i < Peaks.Length; i++)
            {
                float max = 0;
                for (int j = 0; j < Peaks[i].Count; j++)
                {
                    if (Peaks[i][j].Item2 > max)
                    {
                        max = Peaks[i][j].Item2;
                        PeakSelected[i] = Peaks[i][j].Item1;
                    }
                }
            }

            DetermineCertainity();
            TryEnclosingHoles();
            TryFillingIntervals();

            panel2.Invalidate();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            int indexx = 0;
            foreach (Tuple<int, int> t in SelectedIntervals)
            {
                int x0 = t.Item1 * pictureBox1.Width / 120000;
                int x1 = t.Item2 * pictureBox1.Width / 120000;
                e.Graphics.FillRectangle(Brushes.LightGreen, new Rectangle(x0, 0, x1 - x0, panel2.Height));
                e.Graphics.DrawString(indexx.ToString(), new Font("Arial", 10), Brushes.Green, x0, 0);
                indexx++;
            }
            if (selectingRegion)
            {
                int x0 = RegionFrom * pictureBox1.Width / 120000;
                int x1 = RegionTo * pictureBox1.Width / 120000;
                e.Graphics.FillRectangle(Brushes.LightGreen, new Rectangle(x0, 0, x1 - x0, panel2.Height));
                e.Graphics.DrawString("P", new Font("Arial", 10), Brushes.Green, x0, 0);
            }
            for (int i = 0; i < Peaks.Length; i++)
            {
                float x = i * panel2.Width / 250f;
                foreach (Tuple<int, float> t in Peaks[i])
                {
                    if (t.Item1 > 60)
                    {
                        float y = (t.Item1 - 60) * panel2.Height / (960f - 60);
                        int c = (int)(256 * t.Item2);
                        c = Math.Min(255, Math.Max(0, c));
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb((PeakSelected[i] != t.Item1) ? c : 0, (placedRight[i]) ? c : 0, c)), new RectangleF(x - 2, y - 2, 5, 5));
                    }
                }
            }
        }

        bool selectingRegion = false;
        bool testingRegion = false;
        bool settingDetectedPitch = false;
        int RegionFrom = 0;
        int RegionTo = 0;
        List<Tuple<int, int>> SelectedIntervals = new List<Tuple<int, int>>();
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Control))
            {
                settingDetectedPitch = true;
            }
            else
            {
                if (ModifierKeys.HasFlag(Keys.Alt)) testingRegion = true;
                if (!ModifierKeys.HasFlag(Keys.Control) && !testingRegion) SelectedIntervals = new List<Tuple<int, int>>();

                selectingRegion = true;
                RegionFrom = e.X * 120000 / panel2.Width;
            }
        }
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectingRegion)
            {
                RegionTo = e.X * 120000 / panel2.Width;
                panel2.Invalidate();
            }
            if (settingDetectedPitch)
            {
                int timeBin = (int)(e.X * 250f / (float)panel2.Width);
                int peaksPerPixel = (int)(250f/ panel2.Width) + 1;
                int mouseY = 60 + e.Y * 900 / panel2.Height;

                for (int i = 0; i < peaksPerPixel; i++)
                {
                    int bestY = 0;
                    float bestNorm = -1000000f;
                    foreach (Tuple<int, float> t in Peaks[i + timeBin])
                    {
                        //float d = 1/(float)Math.Abs(Math.Log(2, t.Item1 / (float)mouseY));
                        float d = -Math.Abs(t.Item1 - (float)mouseY);
                        float norm = d / t.Item2;
                        if (norm > bestNorm)
                        {
                            bestNorm = norm;
                            bestY = t.Item1;
                        }
                    }
                    PeakSelected[i + timeBin] = bestY; 
                }
                panel2.Invalidate();
            }
        }
        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            if (selectingRegion)
            {
                RegionTo = e.X * 120000 / panel2.Width;
                selectingRegion = false;

                if (!testingRegion)
                {
                    if (RegionTo < RegionFrom)
                    {
                        int pom = RegionFrom;
                        RegionFrom = RegionTo;
                        RegionTo = pom;
                    }

                    SelectedIntervals.Add(new Tuple<int, int>(RegionFrom, RegionTo));
                }
                else
                {
                    if (RegionTo < RegionFrom)
                    {
                        int pom = RegionFrom;
                        RegionFrom = RegionTo;
                        RegionTo = pom;
                    }
                    float[] f = new float[RegionTo - RegionFrom];
                    for (int i = 0; i < f.Length; i++) f[i] = audio25[i + RegionFrom];
                    DebugPlay(f);
                    testingRegion = false;
                }
                panel2.Invalidate();
            }
            if (settingDetectedPitch) settingDetectedPitch = false;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            float spd = 0.4f;
            float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out spd);
            if (SelectedIntervals.Count > 0) GenPTCPitchCor(SelectedIntervals[0].Item1, SelectedIntervals[0].Item2, spd);
            DebugPlay(outputSound);
        }
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                float spd = 0.4f;
                float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out spd);
                if (SelectedIntervals.Count > 0) GenPTCFormCor(SelectedIntervals[0].Item1, SelectedIntervals[0].Item2, spd, int.Parse(textBox1.Text));
                DebugPlay(outputSound);
            }
            catch { }
        }
        private void button10_Click(object sender, EventArgs e)
        {

        }
        private void GenPTCPitchCor(int smpFrom, int smpTo, float spd)
        {
            int lenght = (int)((smpTo - smpFrom) / spd);
            outputSound = new float[lenght];

            float initialPitchLenght = PeakSelected[smpFrom / 480];

            float max = -1;
            int index = -1;
            for (int i = smpFrom; i < smpFrom + initialPitchLenght; i++)
            {
                if (audio25[i] > max)
                {
                    max = audio25[i];
                    index = i;
                }
            }


            float position = index;
            int pitchPoint0 = index;
            int pitchPoint1 = index;
            int pitchPoint2 = index;

            void findNextPtchPt()
            {
                pitchPoint0 = pitchPoint1;
                pitchPoint1 = pitchPoint2;
                max = -1;
                for (int i = pitchPoint1 + PeakSelected[pitchPoint1 / 480] - 10; i < pitchPoint1 + PeakSelected[pitchPoint1 / 480] + 10; i++)
                {
                    if (audio25[i] > max)
                    {
                        max = audio25[i];
                        pitchPoint2 = i;
                    }
                }
            }

            findNextPtchPt();
            findNextPtchPt();
            float relPos = 0;
            for (int i = 0; i < outputSound.Length; i++)
            {
                relPos += 440f / 48000f;
                if (relPos > 1) relPos %= 1;
                float pos1 = (1 - relPos) * pitchPoint0 + relPos * pitchPoint1;
                float pos2 = (1 - relPos) * pitchPoint1 + relPos * pitchPoint2;
                float trans = position % 1f;

                outputSound[i] = audio25[(int)pos1] * (1 - pos1 % 1) * (1 - trans) + audio25[(int)pos1 + 1] * (pos1 % 1) * (1 - trans) +
                   audio25[(int)pos2] * (1 - pos2 % 1) * (trans) + audio25[(int)pos2 + 1] * (pos2 % 1) * (trans);

                position += spd;
                if (position > pitchPoint1) findNextPtchPt();
            }

            LowPassFilter lpf = new LowPassFilter();
            lpf.ProccessM(ref outputSound, 9000f);
        }
        private void GenPTCFormCor(int smpFrom, int smpTo, float spd, float center)
        {
            int lenght = (int)((smpTo - smpFrom) / spd);
            outputSound = new float[lenght];

            float initialPitchLenght = PeakSelected[smpFrom / 480];

            float max = -1;
            int index = -1;
            for (int i = smpFrom; i < smpFrom + initialPitchLenght; i++)
            {
                if (audio25[i] > max)
                {
                    max = audio25[i];
                    index = i;
                }
            }


            float position = index;
            int pitchPoint0 = index;
            int pitchPoint1 = index;
            int pitchPoint2 = index;

            void findNextPtchPt()
            {
                pitchPoint0 = pitchPoint1;
                pitchPoint1 = pitchPoint2;
                max = -1;
                for (int i = pitchPoint1 + PeakSelected[pitchPoint1 / 480] - 10; i < pitchPoint1 + PeakSelected[pitchPoint1 / 480] + 10; i++)
                {
                    if (audio25[i] > max)
                    {
                        max = audio25[i];
                        pitchPoint2 = i;
                    }
                }
            }

            findNextPtchPt();
            findNextPtchPt();
            float relPos = 0;
            for (int i = 0; i < outputSound.Length; i++)
            {
                relPos += center / 48000f;
                if (relPos > 1)
                {
                    relPos %= 1;

                    for (int j = 0; j <= (pitchPoint1 - pitchPoint0); j++)
                    {
                        if (i - (pitchPoint1 - pitchPoint0) + j > 0) outputSound[i - (pitchPoint1 - pitchPoint0) + j] += (float)Math.Pow(Math.Sin(Math.PI / 2 * (j / (float)(pitchPoint1 - pitchPoint0))), 2) * audio25[pitchPoint0 + j];
                    }
                    for (int j = 1; j <= (pitchPoint2 - pitchPoint1); j++)
                    {
                        if (i + j < outputSound.Length) outputSound[i + j] += (float)Math.Pow(Math.Cos(Math.PI / 2 * (j / (float)(pitchPoint2 - pitchPoint1))), 2) * audio25[pitchPoint1 + j];
                    }
                }

                position += spd;
                if (position > pitchPoint1) findNextPtchPt();
            }
        }
        private void GenFMT(int smpFrom, int smpTo)
        {
            int lenght = (int)((smpTo - smpFrom));

            float initialPitchLenght = PeakSelected[smpFrom / 480];
            graunleBuffer = new List<float[]>();

            float max = -1;
            int index = -1;
            for (int i = smpFrom; i < smpFrom + initialPitchLenght; i++)
            {
                if (audio25[i] > max)
                {
                    max = audio25[i];
                    index = i;
                }
            }

            float position = index;
            int pitchPoint0 = index;
            int pitchPoint1 = index;
            int pitchPoint2 = index;

            GranuleTiming = new List<int>() { 0 };

            void findNextPtchPt()
            {
                pitchPoint0 = pitchPoint1;
                pitchPoint1 = pitchPoint2;
                max = -1;
                for (int i = pitchPoint1 + PeakSelected[pitchPoint1 / 480] - 10; i < pitchPoint1 + PeakSelected[pitchPoint1 / 480] + 10; i++)
                {
                    if (audio25[i] > max)
                    {
                        max = audio25[i];
                        pitchPoint2 = i;
                    }
                }
                GranuleTiming.Add(pitchPoint2 - smpFrom);
            }

            findNextPtchPt();
            findNextPtchPt();
            float relPos = 0;
            for (int i = 0; i < outputSound.Length; i++)
            {
                position += 1;
                if (position > pitchPoint1)
                {
                    float[] granule = new float[pitchPoint2 - pitchPoint0];

                    if (pitchPoint2 == pitchPoint1) break;

                    for (int j = 0; j <= (pitchPoint1 - pitchPoint0); j++)
                    {
                        if (i - (pitchPoint1 - pitchPoint0) + j > 0) granule[j] += (float)Math.Pow(Math.Sin(Math.PI / 2 * (j / (float)(pitchPoint1 - pitchPoint0))), 2) * audio25[pitchPoint0 + j];
                    }
                    for (int j = 1; j <= (pitchPoint2 - pitchPoint1 - 1); j++)
                    {
                        if (i + j < outputSound.Length) granule[(pitchPoint1 - pitchPoint0) + j] += (float)Math.Pow(Math.Cos(Math.PI / 2 * (j / (float)(pitchPoint2 - pitchPoint1))), 2) * audio25[pitchPoint1 + j];
                    }

                    graunleBuffer.Add(granule);

                    findNextPtchPt();
                }
            }
        }
        List<int> GranuleTiming = new List<int>();

        int placing = 0;
        int[] autoTune = new int[16] { -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2, -2 };
        private void button15_Click(object sender, EventArgs e) => placing = 0;
        private void button16_Click(object sender, EventArgs e) => placing = 1;
        private void button17_Click(object sender, EventArgs e) => placing = 2;
        private void button18_Click(object sender, EventArgs e) => placing = 3;
        private void button19_Click(object sender, EventArgs e) => placing = 4;
        private void button20_Click(object sender, EventArgs e) => placing = 5;
        private void button21_Click(object sender, EventArgs e) => placing = 6;
        private void button22_Click(object sender, EventArgs e) => placing = 7;

        private void panel3_MouseUp(object sender, MouseEventArgs e)
        {
            int posX = 16 * e.X / panel3.Width;
            if (posX >= 0 && posX < 16)
            {
                if (e.Button == MouseButtons.Left)
                {
                    autoTune[posX] = placing;
                }
                if (e.Button == MouseButtons.Right)
                {
                    autoTune[posX] = -1;
                }
                if (e.Button == MouseButtons.Middle)
                {
                    autoTune[posX] = -2;
                }
            }
            panel3.Invalidate();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < 16; i++)
            {
                int x0 = i * panel3.Width / 16;
                int x1 = (i + 1) * panel3.Width / 16;

                Brush b = Brushes.DarkBlue;
                if (autoTune[i] < 0) b = Brushes.Black;

                e.Graphics.FillRectangle(b, x0 + 1, 1, x1 - x0 - 2, panel3.Height - 2);
                e.Graphics.DrawString(autoTune[i] < 0 ? (autoTune[i] == -1 ? "_" : "*") : autoTune[i].ToString(), new Font("Arial", 9), Brushes.White, x0 + 3, 3);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                GenPTCAuto();
            }
            DebugPlay(outputSound);
        }
        private void button14_Click(object sender, EventArgs e)
        {
            // generate autotune FMT
        }
        private void GenPTCAuto()
        {
            float lenghtPer16 = 48000f * 240f / Form1.Project.GetBPM(0) / 16f;

            float[] realOutput = new float[(int)(16 * lenghtPer16) + 1];

            for (int i = 0; i < 16; i++)
            {
                if (autoTune[i] > -1)
                {
                    int sound = autoTune[i];
                    int lenght = 1;
                    int j = i + 1;
                    while (true)
                    {
                        if (j < 16)
                        {
                            if (autoTune[j] == -2)
                            {
                                j++;
                                lenght++;
                            }
                            else break;
                        }
                        else break;
                    }

                    int from = (int)(lenghtPer16 * i);
                    int to = (int)(lenghtPer16 * (i + lenght));

                    if (sound < SelectedIntervals.Count)
                    {
                        int sfrom = SelectedIntervals[sound].Item1;
                        int sto = SelectedIntervals[sound].Item2;
                        float spda = (sto - sfrom) / (float)(to - from);
                        GenPTCFormCor(sfrom, sto, spda, 440f);

                        for (int k = 0; k < to - from - 1; k++)
                        {
                            if (k + from < realOutput.Length) realOutput[k + from] = outputSound[k];
                        }
                    }
                }
            }

            outputSound = new float[realOutput.Length];
            for (int i = 0; i < outputSound.Length; i++)
            {
                outputSound[i] = realOutput[i];
            }
        }

        private void SampleCutter_FormClosing(object sender, FormClosingEventArgs e)
        {
            DebugStop();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                float spd = 0.4f;
                float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out spd);

                GenPTCPitchCor(SelectedIntervals[0].Item1, SelectedIntervals[0].Item2, spd);
                CorrectedPitchSample cps = new CorrectedPitchSample(outputSound, 440f, spd);
                uint u = Form1.Project.AddSample(new SampleAV() { ips = cps, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, spd) });
                PlaceOnTimeLine(u);
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                try
                {
                    float spd = 0.4f;
                    float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out spd);

                    GenPTCFormCor(SelectedIntervals[0].Item1, SelectedIntervals[0].Item2, spd, int.Parse(textBox1.Text));
                    CorrectedPitchSample cps = new CorrectedPitchSample(outputSound, int.Parse(textBox1.Text), spd);
                    uint u = Form1.Project.AddSample(new SampleAV() { ips = cps, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, spd) });
                    PlaceOnTimeLine(u);
                }
                catch
                {

                }
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                float spd = 0.4f;
                float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out spd);

                GenFMT(SelectedIntervals[0].Item1, SelectedIntervals[0].Item2);
                GranuleSample gs = new GranuleSample(graunleBuffer, GranuleTiming, spd, 0.03f);
                uint u = Form1.Project.AddSample(new SampleAV() { ips = gs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, spd) });
                PlaceOnTimeLine(u);
            }
        }
        private void PlaceOnTimeLine(uint sampleIndex)
        {
            uint n = Form1.Project.AddMedia(new TimelineMedia(new TLSample() { Sample = sampleIndex }, false, 1));
            if (Form1.Project.ProjectMedia[timeLineMedia].TimeFrom >= 48) Form1.Project.ProjectMedia[n].TimeFrom = Form1.Project.ProjectMedia[timeLineMedia].TimeFrom - 48;
            else Form1.Project.ProjectMedia[n].TimeFrom = 0;
            Form1.Project.ProjectMedia[n].TimeLenght = 48;

            Form1.Project.Tracks[timeLineTrack].Media.Add(n);

            Form1.UpdateTimeLine = true;
            lastPlaced = (int)n;
        }
        int lastPlaced = -1;
        private void PlaceOnTimeLine(List<uint> autotune)
        {
            List<uint> media = new List<uint>();
            for (int i = 0; i < autoTune.Length; i++)
            {
                uint n = Form1.Project.AddMedia(new TimelineMedia(new TLSample() { Sample = autotune[i] }, false, 1));
                media.Add(n);
            }
            ulong timeStart = 0;
            if (Form1.Project.ProjectMedia[timeLineMedia].TimeFrom >= 192) timeStart = Form1.Project.ProjectMedia[timeLineMedia].TimeFrom - 192;

            // algoritmus na pridani
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Black, 0, 0, pictureBox1.Width, pictureBox1.Height);
            int from = markedFrom;
            int to = markedTo;
            if (to < from)
            {
                int pom = to;
                to = from;
                from = pom;
            }
            e.Graphics.FillRectangle(Brushes.Red, from, 0, to - from, Height);

            for (int i = 0; i < pictureBox1.Width; i++)
            {
                float max = -1;
                float min = 1;

                for (int j = (int)(i * audio25.Length / (float)pictureBox1.Width); j < (int)((i+1) * audio25.Length / (float)pictureBox1.Width) - 1; j++)
                {
                    if (audio25[j] > max) max = audio25[j];
                    if (audio25[j] < min) min = audio25[j];
                }

                int max2 = (int)(pictureBox1.Height * (max + 1) / 2f);
                int min2 = (int)(pictureBox1.Height * (min + 1) / 2f);

                e.Graphics.DrawLine(Pens.Green, i, max2, i, min2);
            }
            int posX = (int)((smpsel / (float)audio25.Length) * pictureBox1.Width);
            e.Graphics.DrawLine(Pens.Red,posX, 0,posX, pictureBox1.Height);
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            smpsel = (int)((audio25.Length / (float)pictureBox1.Width) * e.X);
            pictureBox1.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float det = 20f;
            float.TryParse(textBox2.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out det);
            outputSound = PitchHelper.GeneratePAD(audio25, smpsel, det);
            DebugPlay(outputSound);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                GenLoop();
            }
            outputSound = new float[intro.Length + loop.Length * 3];
            for (int i = 0; i < intro.Length; i++)
            {
                outputSound[i] = intro[i];
            }
            for (int i = 0; i < loop.Length; i++)
            {
                outputSound[i + intro.Length] = loop[i];
                outputSound[i + intro.Length + loop.Length] = loop[i];
                outputSound[i + intro.Length + loop.Length + loop.Length] = loop[i];
            }
            DebugPlay(outputSound);
        }
        private void button24_Click(object sender, EventArgs e)
        {
            if (SelectedIntervals.Count > 0)
            {
                GenLoop();
            }
            //SAMPLE
        }
        private float[] intro;
        private float[] loop;
        private float loopAvgPitch = 0;
        private void GenLoop()
        {
            // 480 na pp
            int introStart = smpsel;
            int loopStart = SelectedIntervals[0].Item1 / 480 + 1;
            int loopEnd = SelectedIntervals[0].Item2 / 480;

            loopAvgPitch = 0;

            for (int i = 0; i <= loopEnd - loopStart; i++)
            {
                loopAvgPitch += PeakSelected[i + loopStart];
            }
            loopAvgPitch /= (loopEnd - loopStart + 1);
            loopAvgPitch = 48000 / loopAvgPitch;

            loopStart *= 480;
            loopEnd *= 480;

           int crossFadeLenght = (loopEnd - loopStart) / 2;

            float maxMagn = 0;
            float ACresult = 0;
            int bestIndex = 0;
            for (int i = 0; i < crossFadeLenght * 2; i++)
            {
                ACresult = 0;
                for (int j = 0; j < crossFadeLenght; j++)
                {
                    ACresult += audio25[loopStart + j] * audio25[loopEnd - crossFadeLenght + i + j];
                }
                if (ACresult > maxMagn)
                {
                    maxMagn = ACresult;
                    bestIndex = loopEnd - crossFadeLenght + i;
                }
            }
            

            loop = new float[bestIndex - loopStart + 1];
            for (int i = 0; i < loop.Length; i++)
            {
                if (i < loop.Length - crossFadeLenght) loop[i] = audio25[loopStart + crossFadeLenght + i];
                else
                {
                    float fade = (i - (loop.Length - crossFadeLenght)) / (float)crossFadeLenght;
                    loop[i] = (1-fade) * audio25[loopStart + crossFadeLenght + i] + fade * audio25[loopStart + (i - (loop.Length - crossFadeLenght))];
                }
            }

            

            // TROLL LOOP FOUND

            if (introStart < loopStart)
            {
                intro = new float[loopStart - introStart + crossFadeLenght];

                for (int i = 0; i < intro.Length; i++)
                {
                    intro[i] = audio25[i + introStart];
                }
            }
            else intro = new float[0];
        }
        private float GenSpd()
        {
            // 480 na pp
            int loopStart = SelectedIntervals[0].Item1 / 480 + 1;
            int loopEnd = SelectedIntervals[0].Item2 / 480;

            loopAvgPitch = 0;

            float pitchMult = 1;
            float.TryParse(textBox6.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pitchMult);

            for (int i = 0; i <= loopEnd - loopStart && i < 31; i++)
            {
                loopAvgPitch += PeakSelected[i + loopStart];
            }
            if ((loopEnd - loopStart + 1) < 31) loopAvgPitch /= (loopEnd - loopStart + 1);
            else loopAvgPitch /= 31f;
            loopAvgPitch = 48000 / loopAvgPitch * pitchMult;

            loopStart *= 480;
            loopEnd *= 480;

            float maxMagn = 0;
            float ACresult = 0;
            int bestIndex = 0;

            loop = new float[loopEnd - loopStart];
            for (int i = 0; i < loop.Length; i++)
            {
                if (loopStart + i < audio25.Length) loop[i] = audio25[loopStart + i];
            }

            return loopAvgPitch;
        }

        int markedFrom = 0;
        int markedTo = 1;
        bool marking = false;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            markedFrom = e.X;
            marking = true;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            marking = false;
            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (marking)
            {
                markedTo = e.X;
                pictureBox1.Invalidate();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (lastPlaced != -1)
            {
                uint tru = (Form1.Project.ProjectMedia[(uint)lastPlaced].Property as TLSample).Sample;
                Form1.Project.ProjectSamples[tru] = new SampleAV()
                {
                    name = textBox4.Text,
                    ips = Form1.Project.ProjectSamples[tru].ips,
                    ivs = Form1.Project.ProjectSamples[tru].ivs
                };
            }
        }

        private void button26_Click(object sender, EventArgs e)
        {
            int from = markedFrom * audio25.Length / Width;
            int to = markedTo * audio25.Length / Width;
            outputSound = new float[to - from];
            for (int i = 0; i < outputSound.Length; i++)
            {
                if (i + from < audio25.Length) outputSound[i] = audio25[i + from];
            }
            RTPSSample ffts = new RTPSSample(2048, 4, 1, 1, outputSound);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = ffts, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + from / 48000f, 1f) });
            PlaceOnTimeLine(u);
        }

        private void button10_Click_1(object sender, EventArgs e)
        {
            float p = GenSpd();

            float[] a2 = new float[loop.Length * 4];

            NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl =
                new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(new FloatArraySource(loop, 48000),48000*4);

            wdl.Read(a2, 0, a2.Length);

            AudioCutSample acs = new AudioCutSample(a2, false, true, 4 * 440f / p, 0.03f, 0, 1, 0.03f, false);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = acs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, 0.5f) });
            PlaceOnTimeLine(u); 
        }
        private void button28_Click(object sender, EventArgs e)
        {
            float p = GenSpd();

            float[] a2 = new float[loop.Length * 4];

            NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl =
                new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(new FloatArraySource(loop, 48000), 48000 * 4);

            wdl.Read(a2, 0, a2.Length);

            AudioCutSample acs = new AudioCutSample(a2, false, true, 4 * 440f, 0.03f, 0, 1, 0.03f, false);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = acs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, 0.5f) });
            PlaceOnTimeLine(u);
        }
        private void button27_Click(object sender, EventArgs e)
        {
            float p = GenSpd();
            float.TryParse(textBox7.Text,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture, out p);

            float[] a2 = new float[loop.Length * 4];

            NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl =
                new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(new FloatArraySource(loop, 48000), 48000 * 4);

            wdl.Read(a2, 0, a2.Length);

            AudioCutSample acs = new AudioCutSample(a2, false, true, 4 * 440f / p, 0.03f, 0, 1, 0.03f, false);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = acs, ivs = new QuickLoadVideoSample(sourceIndex, 15, true, timeOffset + SelectedIntervals[0].Item1 / 48000f, 0.5f) });
            PlaceOnTimeLine(u);
        }

        private void button26_Click_1(object sender, EventArgs e)
        {
            //float p = GenSpd();
        }
    }
}
