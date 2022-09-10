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
    public partial class GranuleSampleEditor : Form
    {
        public GranuleSampleEditor(GranuleSample gs, uint sampleIndex)
        {
            InitializeComponent();

            originalSample = gs;
            workingSample = new GranuleSample(gs);

            smpind = sampleIndex;
        }
        uint smpind;
        GranuleSample originalSample;
        GranuleSample workingSample;

        //LPF HPF
        private void button1_Click(object sender, EventArgs e)
        {
            LowPassFilter LPF = new LowPassFilter();
            float freq = 8000;
            float.TryParse(textBox1.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);
            int index = 0;
            for (index = 0; index < workingSample.GranuleBuffer.Count; index++)
            {
                float[] newArray = (float[])workingSample.GranuleBuffer[index].Clone();
                LPF.Clear();
                LPF.ProccessM(ref newArray, freq);
                workingSample.GranuleBuffer[index] = newArray;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            HighPassFilter HPF = new HighPassFilter();
            float freq = 200;
            float.TryParse(textBox2.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out freq);
            int index = 0;
            for (index = 0; index < workingSample.GranuleBuffer.Count; index++)
            {
                float[] newArray = (float[])workingSample.GranuleBuffer[index].Clone();
                HPF.Clear();
                HPF.ProccessM(ref newArray, freq);
                workingSample.GranuleBuffer[index] = newArray;
            }
        }

        //SHIFT
        private void button5_Click(object sender, EventArgs e)
        {
            float count = 2;
            float.TryParse(textBox5.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out count);

            int index = 0;
            for (index = 0; index < workingSample.GranuleBuffer.Count; index++)
            {
                float[] newArray = new float[(int)(workingSample.GranuleBuffer[index].Length / count)];

                FloatArraySource fas = new FloatArraySource(workingSample.GranuleBuffer[index], 10000);
                NAudio.Wave.SampleProviders.WdlResamplingSampleProvider ws = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(fas, (int)(10000 / count));
                ws.Read(newArray, 0, newArray.Length);

                workingSample.GranuleBuffer[index] = newArray;
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            float from = 1;
            float to = 1;
            float over = 1;
            float.TryParse(textBox6.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out from);
            float.TryParse(textBox7.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out to);
            float.TryParse(textBox8.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out over);

            int index = 0;
            for (index = 0; index < workingSample.GranuleBuffer.Count; index++)
            {
                float c = to;
                if (workingSample.Timing[index] / 48000f < over) c = from * (float)Math.Pow(to / from, (workingSample.Timing[index] / 48000f) / over);

                float[] newArray = new float[(int)(workingSample.GranuleBuffer[index].Length / c)];

                FloatArraySource fas = new FloatArraySource(workingSample.GranuleBuffer[index], 10000);
                NAudio.Wave.SampleProviders.WdlResamplingSampleProvider ws = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(fas, (int)(10000 / c));
                ws.Read(newArray, 0, newArray.Length);

                workingSample.GranuleBuffer[index] = newArray;
            }
        }

        //DEBUG
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

        private void button7_Click(object sender, EventArgs e)
        {
            DebugPlay();
        }
        
        private void button8_Click(object sender, EventArgs e)
        {
            workingSample = new GranuleSample(originalSample);
        }

        //replace - new
        private void button9_Click(object sender, EventArgs e)
        {
            Form1.Project.ProjectSamples[smpind] = new SampleAV() { ips = new GranuleSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name };
        }
        private void button10_Click(object sender, EventArgs e)
        {
            Form1.Project.AddSample(new SampleAV() { ips = new GranuleSample(workingSample), ivs = Form1.Project.ProjectSamples[smpind].ivs, name = Form1.Project.ProjectSamples[smpind].name + "(E)" });
        }
    }
}
