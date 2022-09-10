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
    public partial class AudioCutSampleEditor : Form
    {
        public AudioCutSampleEditor(AudioCutSample cps, uint sampleIndex)
        {
            InitializeComponent();

            originalSample = cps;
            workingSample = new AudioCutSample(cps);

            smpind = sampleIndex;
        }
        uint smpind;
        AudioCutSample originalSample;
        AudioCutSample workingSample;

        //LPF HPF
        private void button1_Click(object sender, EventArgs e)
        {
            LowPassFilter LPF = new LowPassFilter();
            float freq = 8000;
            float.TryParse(textBox1.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);

            float[] newArray = (float[])workingSample.Audio.Clone();
            LPF.Clear();
            LPF.ProccessM(ref newArray, freq);
            workingSample.Audio = newArray;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            HighPassFilter HPF = new HighPassFilter();
            float freq = 200;
            float.TryParse(textBox1.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);

            float[] newArray = (float[])workingSample.Audio.Clone();
            HPF.Clear();
            HPF.ProccessM(ref newArray, freq);
            workingSample.Audio = newArray;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            float from = 0;
            float to = 12;
            float over = 0.25f;
            float.TryParse(textBox3.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out from);
            float.TryParse(textBox4.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out to);
            float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out over);
            over = Form1.Project.GetBeatDurationSamp(0) * over;

            float[] newArray = new float[workingSample.Audio.Length];
            float pos = 0;
            for (int i = 0; i < newArray.Length; i++)
            {
                if (pos + 1 < workingSample.Audio.Length) newArray[i] = (1 - pos % 1) * workingSample.Audio[(int)pos] + (pos % 1) * workingSample.Audio[(int)pos + 1];
                else newArray[i] = 0;

                float spd = 0;
                if (i > over) spd = to;
                else
                {
                    float p = i / over;
                    spd = (1-p) * from + p * to;
                }
                spd = (float)Math.Pow(2, spd / 12f);
                pos += spd;
            }
            workingSample.Audio = newArray;
        }

        // LOADING
        FloatArraySource fas;
        private void DebugPlay()
        {
            float[] smp = new float[48000];
            IPitchReader ipr = workingSample.GetReader(0);
            ipr.ReadAdd(ref smp, 1, 0);
            float[] smp2 = new float[24000];
            for (int i = 0; i < 24000; i++)
            {
                smp2[i] = smp[i * 2];
            }
            DebugPlay(smp2);
        }
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
        private void button4_Click(object sender, EventArgs e)
        {
            DebugPlay();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            workingSample = new AudioCutSample(originalSample);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            Form1.Project.ProjectSamples[smpind] = new SampleAV() { ips = new AudioCutSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name };
        }
        private void button7_Click(object sender, EventArgs e)
        {
            Form1.Project.AddSample(new SampleAV() { ips = new AudioCutSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name + "(E)" });
        }

        private void button8_Click(object sender, EventArgs e)
        {
            float[] newArray = (float[])workingSample.Audio.Clone();

            float max = 0;
            for (int i = 0; i < newArray.Length; i++) if (Math.Abs(newArray[i]) > max) max = Math.Abs(newArray[i]);
            if (max != 0) for (int i = 0; i < newArray.Length; i++) newArray[i] /= max;

            workingSample.Audio = newArray;
        }
    }
}
