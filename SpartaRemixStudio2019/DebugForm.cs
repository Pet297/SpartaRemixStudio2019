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
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }


        private void DebugForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int buf = 2;
            int del = 10;
            int.TryParse(textBox1.Text, out del);
            int.TryParse(textBox2.Text, out buf);

            Form1.so.ReplaceAudioProvider("WaveOut",buf,del, checkBox1.Checked);
            if (checkBox2.Checked) Form1.so.SaveAudioProvider("WaveOut", buf, del, checkBox1.Checked);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            int buf = 2;
            int del = 10;
            int.TryParse(textBox1.Text, out del);
            int.TryParse(textBox2.Text, out buf);

            Form1.so.ReplaceAudioProvider("WaveOutEvent", buf, del, checkBox1.Checked);
            if (checkBox2.Checked) Form1.so.SaveAudioProvider("WaveOutEvent", buf, del, checkBox1.Checked);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            int buf = 2;
            int del = 10;
            int.TryParse(textBox1.Text, out del);
            int.TryParse(textBox2.Text, out buf);

            Form1.so.ReplaceAudioProvider("WasapiOut", buf, del, checkBox1.Checked);
            if (checkBox2.Checked) Form1.so.SaveAudioProvider("WasapiOut", buf, del, checkBox1.Checked);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            int buf = 2;
            int del = 10;
            int.TryParse(textBox1.Text, out del);
            int.TryParse(textBox2.Text, out buf);

            Form1.so.ReplaceAudioProvider("AsioOut", buf, del, checkBox1.Checked);
            if (checkBox2.Checked) Form1.so.SaveAudioProvider("AsioOut", buf, del, checkBox1.Checked);

        }
        private void button5_Click(object sender, EventArgs e)
        {
            int buf = 2;
            int del = 10;
            int.TryParse(textBox1.Text, out del);
            int.TryParse(textBox2.Text, out buf);

            Form1.so.ReplaceAudioProvider("DirectSoundOut", buf, del, checkBox1.Checked);
            if (checkBox2.Checked) Form1.so.SaveAudioProvider("DirectSoundOut", buf, del, checkBox1.Checked);
        }

        private void button6_Click(object sender, EventArgs e) => Form1.Debug1 = true;
        private void button7_Click(object sender, EventArgs e) => Form1.Debug2 = true;
        private void button8_Click(object sender, EventArgs e) => Form1.Debug3 = true;

        private void button8_Click_1(object sender, EventArgs e)
        {
            Form1.renderVideoDescriptor = false;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            Form1.renderVideoDescriptor = true;
        }
    }
}
