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
    public partial class PickProjectForm : Form
    {
        public PickProjectForm()
        {
            InitializeComponent();
        }

        private void PickProjectForm_Load(object sender, EventArgs e)
        {
            IEnumerable<string> dir = Directory.EnumerateDirectories("Projects");
            foreach(string s in dir)
            {
                string s2 = s.Remove(0, 9);
                listBox1.Items.Add(s2);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string s = (string)listBox1.SelectedItem;
                Form1.projectName = s;
                int.TryParse(textBox6.Text, out Form1.changeWidth);
                int.TryParse(textBox5.Text, out Form1.changeHeight);

                if (checkBox1.Checked) Form1.changeAR = true;
                else Form1.changeAR = false;
                Form1.newProject = false;

                Close();
            }
        }

        int resw = 1280;
        int resh = 720;
        private void button1_Click(object sender, EventArgs e)
        {
            resw = 1920;
            resh = 1080;
            NewProject();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            resw = 1440;
            resh = 1080;
            NewProject();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            resw = 1280;
            resh = 720;
            NewProject();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            resw = 960;
            resh = 720;
            NewProject();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            int.TryParse(textBox1.Text, out resw);
            int.TryParse(textBox2.Text, out resh);
            NewProject();
        }
        private void NewProject()
        {
            Form1.projectName = textBox3.Text;

            if (listBox1.Items.Contains(Form1.projectName))
            {
                Form1.changeWidth = resw;
                Form1.changeHeight = resh;

                Form1.changeAR = true;
                Form1.newProject = false;
                Close();
            }
            else
            {
                Form1.changeAR = true;
                Form1.changeWidth = resw;
                Form1.changeHeight = resh;
                Form1.newProject = true;
                Close();
            }
        }
    }
}
