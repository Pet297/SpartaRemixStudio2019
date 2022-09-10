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
    public partial class CorrectedPitchSampleEditor : Form
    {
        public CorrectedPitchSampleEditor(CorrectedPitchSample cps, uint sampleIndex)
        {
            InitializeComponent();

            originalSample = cps;
            workingSample = new CorrectedPitchSample(cps);

            textBox6.Text = originalSample.A.ToString("0.0000");
            textBox7.Text = originalSample.D.ToString("0.0000");
            textBox8.Text = originalSample.S.ToString("0.0000");
            textBox9.Text = originalSample.R.ToString("0.0000");

            smpind = sampleIndex;
        }
        uint smpind;
        CorrectedPitchSample originalSample;
        CorrectedPitchSample workingSample;

        // PLAY - RESTORE - REPLACE - NEW
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
        private void button5_Click(object sender, EventArgs e)
        {
            DebugPlay();
        }
        private void button6_Click(object sender, EventArgs e)
        {
            workingSample = new CorrectedPitchSample(originalSample);

            textBox6.Text = originalSample.A.ToString("0.0000");
            textBox7.Text = originalSample.D.ToString("0.0000");
            textBox8.Text = originalSample.S.ToString("0.0000");
            textBox9.Text = originalSample.R.ToString("0.0000");
        }
        private void button7_Click(object sender, EventArgs e)
        {
            Form1.Project.ProjectSamples[smpind] = new SampleAV() { ips = new CorrectedPitchSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name };
        }
        private void button8_Click(object sender, EventArgs e)
        {
            Form1.Project.AddSample(new SampleAV() { ips = new CorrectedPitchSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name + "(E)" });
        }

        // LOWPASS - HIPASS
        private void button1_Click(object sender, EventArgs e)
        {
            LowPassFilter LPF = new LowPassFilter();
            float freq = 8000;
            float.TryParse(textBox1.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);

            freq /= 4;

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

            freq /= 4;

            float[] newArray = (float[])workingSample.Audio.Clone();
            HPF.Clear();
            HPF.ProccessM(ref newArray, freq);
            workingSample.Audio = newArray;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*//freq
            int mult = 2;
            int.TryParse(textBox3.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out mult);
            float vol = 0.25f;
            float.TryParse(textBox4.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out vol);

            //napitchovat
            float[] newArray = (float[])workingSample.Audio.Clone();
            float[] a2 = new float[newArray.Length * 4];

            IPitchReader ipr = workingSample.GetReader(0);
            ipr.SetProperty(3, (float)Math.Log(workingSample.pitch / 440f * mult, 2) * 12);
            ipr.ReadAdd(ref a2, 1, 0);
            for (int i = 0; i < newArray.Length; i++)
            {
                a2[]
            }
            //upsample 4
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = mult * a2[i * 2];
            }
            workingSample.Audio = newArray;*/
        }
        private void button4_Click(object sender, EventArgs e)
        {
            //dist
            float freq = 5;
            float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);

            float spd0 = (float)Math.Pow(2, freq / 1200f);
            float spd1 = (float)Math.Pow(2, - freq / 1200f);

            float[] newArray = (float[])workingSample.Audio.Clone();         
            for (int i = 0; i < newArray.Length; i++)
            {
                float pos0 = i * spd0;
                float pos1 = i * spd1;
                if (pos0 < workingSample.Audio.Length - 1)
                {
                    float smp0 = (1 - pos0 % 1f) * workingSample.Audio[(int)pos0] + (pos0 % 1f) * workingSample.Audio[(int)pos0 + 1];
                    newArray[i] += smp0;
                }
                if (pos1 < workingSample.Audio.Length - 1)
                {
                    float smp1 = (1 - pos1 % 1f) * workingSample.Audio[(int)pos1] + (pos1 % 1f) * workingSample.Audio[(int)pos1 + 1];
                    newArray[i] += smp1;
                }
                newArray[i] += workingSample.Audio[i];
                newArray[i] /= 3f;
            }
            workingSample.Audio = newArray;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox6.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out workingSample.A);
        }
        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox7.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out workingSample.D);
        }
        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox8.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out workingSample.S);
        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox9.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out workingSample.R);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            float[] newArray = (float[])workingSample.Audio.Clone();

            float max = 0;
            for (int i = 0; i < newArray.Length; i++) if (Math.Abs(newArray[i]) > max) max = Math.Abs(newArray[i]);
            if (max != 0) for (int i = 0; i < newArray.Length; i++) newArray[i] /= max;

            workingSample.Audio = newArray;
        }
    }
}
