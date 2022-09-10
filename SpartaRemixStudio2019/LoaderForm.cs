using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public partial class LoaderForm : Form
    {
        public LoaderForm()
        {
            InitializeComponent();
        }

        int style = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            style = 0;
            openFileDialog1.ShowDialog();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            style = 1;
            openFileDialog1.ShowDialog();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            style = 2;
            openFileDialog1.ShowDialog();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (style == 0)
            {
                VideoSourceS vss = new VideoSourceS(openFileDialog1.FileName);

                MessageBox.Show("audio " + (vss.audio ? "" : "not ") + "found.\nvideo " + (vss.video ? "" : "not ") + "found.\n\n" + (vss.video || vss.audio ? "New Source Added." : "Wasn't added.") + (vss.video ? "" : "\nNote that FFMPEG can't open .wmv files."));
                if (vss.audio || vss.video)
                {
                    string name = "";
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    }
                    catch
                    {
                        name = "NamingFunctionBug";
                    }
                    SampleAV sav = new SampleAV() { ivs = (vss.video ? vss : null), ips = (vss.audio ? vss : null), name = name };
                    Form1.Project.AddSample(sav);
                }
            }
            if (style == 1)
            {
                try
                {
                    AudioFileReader afr = new AudioFileReader(openFileDialog1.FileName);
                    int lenght = (int)(afr.TotalTime.TotalSeconds*48000) * 2;
                    ISampleProvider isp = afr;
                    if (isp.WaveFormat.SampleRate != 48000) isp = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(isp, 48000);
                    if (isp.WaveFormat.Channels != 2) isp = new NAudio.Wave.SampleProviders.MonoToStereoSampleProvider(isp);
                    float[] audio = new float[lenght];
                    float[] dummy = new float[2];

                    while(dummy[0] == 0 && dummy[1] == 0)
                    {
                        isp.Read(dummy, 0, 2);
                    }

                    isp.Read(audio, 0, audio.Length);


                    MessageBox.Show("Audio loaded. Sample added.");

                    string name = "";
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    }
                    catch
                    {
                        name = "NamingFunctionBug";
                    }
                    SampleAV sav = new SampleAV() { ivs = null, ips = new AudioCutSample(audio, true, false, 1, 0.03f, 0, 1, 0.03f, false), name = name };
                    Form1.Project.AddSample(sav);
                }
                catch
                {
                    MessageBox.Show("Audio couldn't be found.");
                }
            }
            if (style == 2)
            {
                try
                {
                    Bitmap bmp = new Bitmap(openFileDialog1.FileName);

                    if (Form1.Debug1)
                    {
                        DebugImage di = new DebugImage(bmp, "This is what will be loaded in. Does it look right?");
                        di.ShowDialog();
                    }

                    MessageBox.Show("Image can be opened. Added.");

                    string name = "";
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    }
                    catch
                    {
                        name = "NamingFunctionBug";
                    }
                    SampleAV sav = new SampleAV() { ivs = new BitmapSource(openFileDialog1.FileName), ips = null, name = name };
                    Form1.Project.AddSample(sav);

                    if (Form1.Debug1)
                    {
                        MessageBox.Show("Now, check wether it got loaded to OpenGL.");
                    }
                }
                catch
                {
                    MessageBox.Show("Image couldn't be opened.");
                }
                Form1.Debug1 = false;
            }
        }
    }
}
