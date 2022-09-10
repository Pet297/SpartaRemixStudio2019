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
    public partial class MediaLibrary : UserControl
    {
        public MediaLibrary()
        {
            InitializeComponent();

            ShowNone();
            mediaLibrarySample1.Dock = DockStyle.Fill;
            mediaLibrarySample1.Show();
        }

        private void ShowNone()
        {
            mediaLibrarySample1.Dock = DockStyle.None;
            mediaLibrarySample1.Hide();          

            mediaLibraryPattern1.Dock = DockStyle.None;
            mediaLibraryPattern1.Hide();

            mediaLibraryNumber1.Dock = DockStyle.None;
            mediaLibraryNumber1.Hide();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
            mediaLibrarySample1.Dock = DockStyle.Fill;
            mediaLibrarySample1.Show();
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
            mediaLibraryPattern1.Dock = DockStyle.Fill;
            mediaLibraryPattern1.Show();
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
            mediaLibraryNumber1.Dock = DockStyle.Fill;
            mediaLibraryNumber1.Show();
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
        }
        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            ShowNone();
        }

        public TimelineMediaType getTLM()
        {
            if (radioButton1.Checked)
            {
                if (mediaLibrarySample1.GetSample() != uint.MaxValue) return new TLSample() { Sample = mediaLibrarySample1.GetSample() };


                return null;
            }
            if (radioButton2.Checked) return mediaLibraryPattern1.getTLM();
            if (radioButton3.Checked) return mediaLibraryNumber1.GetTLT();
            return null;
        }

        private void MediaLibrary_Resize(object sender, EventArgs e)
        {

        }
    }
}
