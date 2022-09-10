using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public partial class MediaLibraryPattern : UserControl
    {
        public MediaLibraryPattern()
        {
            InitializeComponent();

            patternPicker1.ItemPicked += (s, e) => PatternLoaded();

            scrollableValue1.SetUnit(0.01f, "0.00");
            scrollableValue2.SetUnit(0.01f, "0.00");
            scrollableValue3.SetUnit(0.01f, "0.00");
            scrollableValue4.SetUnit(0.01f, "0.00");

            scrollableValue5.SetUnit(0.1f, "+0.0 db;-0.0 db;-0.0 db");
            scrollableValue6.SetUnit(0.1f, "+0.0 db;-0.0 db;-0.0 db");
            scrollableValue7.SetUnit(0.1f, "+0.0 db;-0.0 db;-0.0 db");
            scrollableValue8.SetUnit(0.1f, "+0.0 db;-0.0 db;-0.0 db");


        }

        int enteringIndex = -1;
        int autoEnteringIndex = 0;
        int sampleCount = 0;
        int sampleScroll = 0;

        uint pattern = 0;
        float[] positions = new float[0];
        float[] volumes = new float[0];
        uint[] samples = new uint[0];

        private void MediaLibraryPattern_Load(object sender, EventArgs e)
        {
        }

        public void PatternLoaded()
        {
            if (Form1.Project.ProjectPatterns.ContainsKey(pattern)) sampleCount = Form1.Project.ProjectPatterns[pattern].SampleCount;
            else sampleCount = 0;
            positions = new float[sampleCount];
            volumes = new float[sampleCount];
            samples = new uint[sampleCount];
            for (int i = 0; i < sampleCount; i++) samples[i] = uint.MaxValue;
            enteringIndex = -1;
            autoEnteringIndex = 0;
            sampleScroll = 0;

            SamplesScrolled();
        }
        private void SamplesScrolled()
        {
            if (sampleScroll == 0) button1.Enabled = false;
            if (sampleScroll == (sampleCount - 1) / 4) button2.Enabled = false;
            label1.Text = "Pattern " + pattern + ", uses " + sampleCount + " samples";

            if (sampleScroll * 4 >= 0 && sampleScroll * 4 < sampleCount)
            {
                scrollableValue1.Show();
                scrollableValue5.Show();
                pictureBox1.Show();
                scrollableValue1.SetValue(positions[sampleScroll * 4]);
                scrollableValue5.SetValue(volumes[sampleScroll * 4]);

                if (samples[sampleScroll * 4] == uint.MaxValue) pictureBox1.Image = null;
                else if (Form1.Project.ProjectSamples[samples[sampleScroll * 4]].ivs == null) pictureBox1.Image = null;
                else pictureBox1.Image = Form1.Project.ProjectSamples[samples[sampleScroll * 4]].ivs.PreviewBitmap;
            }
            else
            {
                scrollableValue1.Hide();
                scrollableValue5.Hide();
                pictureBox1.Hide();
            }
            if (sampleScroll * 4 + 1 >= 0 && sampleScroll * 4 + 1 < sampleCount)
            {
                scrollableValue2.Show();
                scrollableValue6.Show();
                pictureBox2.Show();
                scrollableValue2.SetValue(positions[sampleScroll * 4 + 1]);
                scrollableValue6.SetValue(volumes[sampleScroll * 4 + 1]);

                if (samples[sampleScroll * 4 + 1] == uint.MaxValue) pictureBox2.Image = null;
                else if (Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 1]].ivs == null) pictureBox2.Image = null;
                else pictureBox2.Image = Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 1]].ivs.PreviewBitmap;
            }
            else
            {
                scrollableValue2.Hide();
                scrollableValue6.Hide();
                pictureBox2.Hide();
            }
            if (sampleScroll * 4 + 2 >= 0 && sampleScroll * 4 + 2 < sampleCount)
            {
                scrollableValue3.Show();
                scrollableValue7.Show();
                pictureBox3.Show();
                scrollableValue3.SetValue(positions[sampleScroll * 4 + 2]);
                scrollableValue7.SetValue(volumes[sampleScroll * 4 + 2]);

                if (samples[sampleScroll * 4 + 2] == uint.MaxValue) pictureBox3.Image = null;
                else if (Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 2]].ivs == null) pictureBox3.Image = null;
                else pictureBox3.Image = Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 2]].ivs.PreviewBitmap;
            }
            else
            {
                scrollableValue3.Hide();
                scrollableValue7.Hide();
                pictureBox3.Hide();
            }
            if (sampleScroll * 4 + 3 >= 0 && sampleScroll * 4 + 3 < sampleCount)
            {
                scrollableValue4.Show();
                scrollableValue8.Show();
                pictureBox4.Show();
                scrollableValue4.SetValue(positions[sampleScroll * 4 + 3]);
                scrollableValue8.SetValue(volumes[sampleScroll * 4 + 3]);

                if (samples[sampleScroll * 4 + 3] == uint.MaxValue) pictureBox4.Image = null;
                else if (Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 3]].ivs == null) pictureBox4.Image = null;
                else pictureBox4.Image = Form1.Project.ProjectSamples[samples[sampleScroll * 4 + 3]].ivs.PreviewBitmap;
            }
            else
            {
                scrollableValue4.Hide();
                scrollableValue8.Hide();
                pictureBox4.Hide();
            }
        }

        private void mediaLibrarySample1_ItemPicked(object sender, EventArgs e)
        {
            int sampleAffected = 0;
            if (enteringIndex == -1 && sampleCount != 0)
            {
                sampleAffected = autoEnteringIndex;
                autoEnteringIndex++;
                autoEnteringIndex %= sampleCount;
            }
            else if (sampleCount == 0)
            {
                sampleAffected = 0;
            }
            else
            {
                sampleAffected = enteringIndex;
            }

            if (sampleAffected < sampleCount)
            {
                samples[sampleAffected] = mediaLibrarySample1.GetSample();
            }

            SamplesScrolled();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            sampleScroll--;
            SamplesScrolled();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            sampleScroll++;
            SamplesScrolled();
        }

        public TLPattern getTLM()
        {
            return new TLPattern(){ Pattern = pattern, Samples = samples.ToList(), Positions = positions.ToList(), Volumes = volumes.ToList(), KeepVideo = checkBox1.Checked};
        }
        public void loadTLM(TLPattern tlm)
        {
            pattern = tlm.Pattern;
            PatternLoaded();
            for (int i = 0; i < tlm.Samples.Count && i < samples.Length; i++) samples[i] = tlm.Samples[i];
            for (int i = 0; i < tlm.Volumes.Count && i < volumes.Length; i++) volumes[i] = tlm.Volumes[i];
            for (int i = 0; i < tlm.Positions.Count && i < positions.Length; i++) positions[i] = tlm.Positions[i];
            SamplesScrolled();
        }

        private void MediaLibraryPattern_Resize(object sender, EventArgs e)
        {
            //patternPicker1.Size = new Size(100, Height - 211);
            //mediaLibrarySample1.Size = new Size(Width - 357,100);
        }

        private void patternPicker1_ItemPicked(object sender, EventArgs e)
        {
            pattern = patternPicker1.GetPattern();
            PatternLoaded();
        }

        private void mediaLibrarySample1_Resize(object sender, EventArgs e)
        {
            //mediaLibrarySample1.Location = new Point(265, 0);
            //patternPicker1.Location = new Point(0, 193);
        }
    }
}
