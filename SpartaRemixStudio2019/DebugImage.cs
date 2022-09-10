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
    public partial class DebugImage : Form
    {
        public DebugImage(Bitmap b, string s)
        {
            InitializeComponent();
            label2.Text = s;
            pictureBox1.Image = b;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
            Form1.Debug2 = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
