using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpartaRemixStudio2019
{
    public partial class Text_Generator : Form
    {
        public Text_Generator()
        {
            InitializeComponent();
        }
        public static Bitmap GenerateText(int width, int height, string text, string font, float emSize, bool useGlow, bool useFirstOutline, bool useSecondOutline, bool useGradient, int GlowWidth,
            Color GlowColorPerStage, Color OutlineColor1, float OutlineWidth1, Color OutlineColor2, float OutlineWidth2, Color FillColorA, Color FillColorB)
        {
            Bitmap b = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            FontFamily fontFamily = new FontFamily(font);
            StringFormat strformat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            string szbuf = text;

            GraphicsPath path = new GraphicsPath();
            path.AddString(szbuf, fontFamily,
            (int)FontStyle.Regular, emSize, new Rectangle(0, 0, width, height), strformat);

            if (useGlow)
            {
                for (int i = 1; i < GlowWidth; ++i)
                {
                    Pen glowPen = new Pen(GlowColorPerStage, i);
                    glowPen.LineJoin = LineJoin.Round;
                    g.DrawPath(glowPen, path);
                }
            }
            if (useSecondOutline)
            {
                Pen penout = new Pen(OutlineColor2, OutlineWidth2);
                penout.LineJoin = LineJoin.Round;
                g.DrawPath(penout, path);
            }
            if (useFirstOutline)
            {
                Pen pen = new Pen(OutlineColor1, OutlineWidth1);
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, path);
            }
            Brush brush;
            if (!useGradient) brush = new SolidBrush(FillColorA);
            else brush = new LinearGradientBrush(path.GetBounds(), FillColorA, FillColorB, LinearGradientMode.Vertical);
            g.FillPath(brush, path);

            g.Dispose();

            return b;
        }

        Bitmap b;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                float s = 150; float.TryParse(textBox3.Text, out s);

                float f4 = 6; float.TryParse(textBox4.Text, out f4);
                byte b5 = 0; byte.TryParse(textBox5.Text, out b5);
                byte b6 = 0; byte.TryParse(textBox6.Text, out b6);
                byte b7 = 0; byte.TryParse(textBox7.Text, out b7);

                float f11 = 6; float.TryParse(textBox11.Text, out f11);
                byte b10 = 0; byte.TryParse(textBox10.Text, out b10);
                byte b9 = 0; byte.TryParse(textBox9.Text, out b9);
                byte b8 = 0; byte.TryParse(textBox8.Text, out b8);

                byte b14 = 0; byte.TryParse(textBox14.Text, out b14);
                byte b13 = 0; byte.TryParse(textBox13.Text, out b13);
                byte b12 = 0; byte.TryParse(textBox12.Text, out b12);

                byte b17 = 0; byte.TryParse(textBox17.Text, out b17);
                byte b16 = 0; byte.TryParse(textBox16.Text, out b16);
                byte b15 = 0; byte.TryParse(textBox15.Text, out b15);

                b = GenerateText(Form1.Project.WIDTH, Form1.Project.HEIGHT, textBox1.Text, textBox2.Text, s, false, checkBox2.Checked, checkBox3.Checked, checkBox4.Checked,
                  0, new Color(), Color.FromArgb(b5,b6,b7),f4, Color.FromArgb(b10, b9, b8), f11, Color.FromArgb(b14, b13, b12), Color.FromArgb(b17, b16, b15));

                pictureBox1.Image = b;
            }
            catch
            {
                b = null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (b != null)
            {
                DateTime dt = DateTime.Now;
                string s = dt.ToString("yyMMddHHmmss");
                b.Save(s + ".png");
                Form1.Project.AddSample(new SampleAV() { name = textBox1.Text, ivs = new BitmapSource(s + ".png"), ips = null });
            }
        }
    }
}
