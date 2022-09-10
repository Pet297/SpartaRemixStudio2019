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
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox1.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out Form1.Project.BPM);
            if (Form1.Project.BPM < 1) Form1.Project.BPM = 20;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            float.TryParse(textBox2.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out Form1.Project.MasterPitch);
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = Form1.Project.BPM.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
            textBox2.Text = Form1.Project.MasterPitch.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
