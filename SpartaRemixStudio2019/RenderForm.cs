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
    public partial class RenderForm : Form
    {
        public RenderForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int beats = 0;
            int FPS = 0;
            int.TryParse(textBox1.Text, out beats);
            int.TryParse(textBox2.Text, out FPS);
            if (beats > 0 && FPS > 0)
            {
                Form1.RenderBeats = beats;
                Form1.RenderFPS = FPS;
                Form1.StartRendering = true;
            }
        }
    }
}
