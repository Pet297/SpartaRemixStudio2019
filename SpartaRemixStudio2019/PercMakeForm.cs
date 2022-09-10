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
    public partial class PercMakeForm : Form
    {
        VideoSourceS source;
        float timeOffset;
        uint timeLineTrack;
        uint timeLineMedia;
        uint sourceIndex;
        float[] audio25 = new float[0];
        public PercMakeForm(VideoSourceS source, uint sourceIndex, float timeOffset, uint timeLineTrack, uint timeLineMedia)
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
                audio25[i] = (audioTemp[2 * i + 0] + audioTemp[2 * i + 1]);
            }
        }

        private void button5_Click(object sender, EventArgs e) => SetAuto(0.01f, 0.15f, 0.03f, 4f,1,0, 0.03f);
        private void button6_Click(object sender, EventArgs e) => SetAuto(0.00f, 0.00f, 0.35f, 2f,0.2f,1f, 0.50f);
        private void button7_Click(object sender, EventArgs e) => SetAuto(0.00f, 0.01f, 0.10f, 1f,0,1, 0.10f);
        private void button8_Click(object sender, EventArgs e) => SetAuto(0.003f, 0.03f, 0.11f, 1.9f,1,0.8f, 0.22f);
        private void SetAuto(float f1, float f2, float f3, float f4, float f5, float f6, float f7)
        {
            textBox3.Text = f1.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            textBox4.Text = f2.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            textBox5.Text = f3.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            textBox6.Text = f4.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

            trackBar2.Value = (int)(f5 * 100);
            trackBar3.Value = (int)(f6 * 100);

            textBox7.Text = f7.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < pictureBox1.Width; i++)
            {
                float min = 1;
                float max = -1;

                for (int j = 0; j < 48; j++)
                {
                    if (i*48+j < result.Length)
                    {
                        if (result[i * 48 + j] > max) max = result[i * 48 + j];
                        if (result[i * 48 + j] < min) min = result[i * 48 + j];
                    }
                    else
                    {
                        if (0 > max) max = 0;
                        if (0 < min) min = 0;
                    }
                }
                max = (max + 1) * pictureBox1.Height / 2;
                min = (min + 1) * pictureBox1.Height / 2;
                e.Graphics.DrawLine(Pens.Green, i, max, i, min);
            }
        }

        float[] result = new float[0];
        private void Generate(object sender, EventArgs e)
        {
            float ti = 0.0f;
            float.TryParse(textBox8.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out ti);
            int ofset = (int)(ti * 48000);

            float[] lf = new float[120000-ofset];
            int lphz = 400;
            int.TryParse(textBox1.Text, out lphz);
            for (int i = 0; i < 120000 - ofset; i++) lf[i] = audio25[i + ofset];
            LowPassFilter lpf = new LowPassFilter();
            lpf.ProccessM(ref lf, lphz);

            float[] hf = new float[120000 - ofset];
            int hphz = 1000;
            int.TryParse(textBox2.Text, out hphz);
            for (int i = 0; i < 120000 - ofset; i++) hf[i] = audio25[i + ofset];
            HighPassFilter hpf = new HighPassFilter();
            hpf.ProccessL(ref hf, hphz);

            int.TryParse(textBox1.Text, out lphz);

            float[] lf2 = new float[480000 - ofset*4];
            FloatArraySource fas = new FloatArraySource(lf, 48000);
            NAudio.Wave.SampleProviders.WdlResamplingSampleProvider wdl = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(fas, 48000 * 4);
            wdl.Read(lf2, 0, 480000 - ofset*4);

            //------------------------------------------------------------

            float f1 = 0.00f;
            float f2 = 0.01f;
            float f3 = 0.10f;
            float f4 = 0.20f;
            float f5 = 1.00f;
            float.TryParse(textBox3.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f1);
            float.TryParse(textBox4.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f2);
            float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f3);
            float.TryParse(textBox7.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f4);
            float.TryParse(textBox6.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out f5);

            float totalLenght = f1 + f2 + Math.Max(f3, f4);
            result = new float[(int)(totalLenght * 48000)];
            float baseSpd = (float)Math.Pow(2f, ((trackBar1.Value / 100f) - 3f));
            float poslf = 0;

            float voll = trackBar2.Value / 100f;
            float volh = trackBar3.Value / 100f;

            for (int i = 0; i < result.Length; i++)
            {
                float timeSec = i / 48000f;

                if (timeSec < f1)
                {
                    float p = (timeSec) / f1;
                    result[i] = hf[i] * ((1-p) + p * volh);
                    result[i] += lf[i] * (p * voll);
                }
                else if (timeSec < f1 + f2)
                {
                    float p = ((timeSec - f1) / (f2 + f3));
                    float spd = 4 * baseSpd / (1 + p * (f5 - 1));

                    result[i] = hf[i] * volh;
                    if ((int)poslf + 1 < lf.Length) result[i] += (lf[(int)poslf] * (1 - spd % 1) + lf[(int)poslf + 1] * (spd % 1)) * voll;

                    poslf += spd;
                }
                else
                {
                    result[i] = 0;
                    if (timeSec < f1 + f2 + f4)
                    {
                        float p = ((timeSec - f1 - f2) / (f4));

                        result[i] += hf[i] * volh * (1 - p);
                    }
                    if (timeSec < f1 + f2 + f3)
                    {
                        float p = ((timeSec - f1) / (f2 + f3));
                        float spd = 4 * baseSpd / (1 + p * (f5 - 1));

                        float p0 = ((timeSec - f1 - f2) / (f3));


                        if ((int)poslf + 1 < lf.Length) result[i] += (lf[(int)poslf] * (1 - spd % 1) + lf[(int)poslf + 1] * (spd % 1)) * voll * (1 - p0);

                        poslf += spd;
                    }
                }
            }
            pictureBox1.Invalidate();
        }

        FloatArraySource fas;
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

        private void PercMakeForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DebugStop();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DebugPlay(result);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AudioCutSample acs = new AudioCutSample(result, false, false, 1f,0,0,1,0, true);
            uint u = Form1.Project.AddSample(new SampleAV() { ips = acs, ivs = new QuickLoadVideoSample(sourceIndex, 30, true, timeOffset, 1f) });
            PlaceOnTimeLine(u);
        }
        private void PlaceOnTimeLine(uint sampleIndex)
        {
            uint n = Form1.Project.AddMedia(new TimelineMedia(new TLSample() { Sample = sampleIndex }, false, 1));
            if (Form1.Project.ProjectMedia[timeLineMedia].TimeFrom >= 48) Form1.Project.ProjectMedia[n].TimeFrom = Form1.Project.ProjectMedia[timeLineMedia].TimeFrom - 48;
            else Form1.Project.ProjectMedia[n].TimeFrom = 0;
            Form1.Project.ProjectMedia[n].TimeLenght = 48;

            Form1.Project.Tracks[timeLineTrack].Media.Add(n);

            Form1.UpdateTimeLine = true;
        }
    }
}
