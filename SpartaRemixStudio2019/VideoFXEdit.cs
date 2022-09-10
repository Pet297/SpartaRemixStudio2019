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
    public partial class VideoFXEdit : Form
    {
        string[] buttonText = new string[64]
        {"P-FlipX","P-FlipY","P-Opac","P-Zoom","P-Mult","P-Cont","P-Bright","P-BW",
        "P-Inv","P-grid-vis","P-AR-Zoom","P-AR-Stretch","","","","",
        "Border-Solid","Blur-Dir","Blur-DirMP","Blur-Gaus","","","","",
        "Flatten","Flatten-InfX","Flatten-InfY","Flatten-InfXY","","","","",
        "TR-Trans","TR-Scale","TR-Rot-Z","TR-Rot-Y","TR-Rot-X","","","",
        "","","","","","","","",
        "","","","","","","","",
        "","","","","","","",""};

        public VideoFXEdit(List<IVideoFX> list)
        {
            InitializeComponent();

            for (int i = 0; i < 64; i++)
            {
                Button b = new Button();
                b.Location = new Point(12 + (i % 8) * 54, 12 + (i / 8) * 54);
                b.Size = new Size(48, 48);
                b.UseVisualStyleBackColor = true;
                b.Text = buttonText[i];
                int j = i;
                b.Click += (s, e) => PressButton(j);
                this.Controls.Add(b);
            }

            collectionEditor1.vfx = list;
        }

        void PressButton(int number)
        {
            IVideoFX fx = EffectHelper.VFXFromNumber((ushort)number);
            if (fx != null) collectionEditor1.vfx.Add(fx);
            fx?.Init();
            collectionEditor1.Invalidate();

            Form1.UpdateTimeLine = true;
        }

        private void collectionEditor1_MouseUp(object sender, MouseEventArgs e)
        {
            if (collectionEditor1.SelectedLoc >= 0 && collectionEditor1.SelectedLoc < collectionEditor1.vfx.Count)
                knobEditor1.Load(collectionEditor1.vfx[collectionEditor1.SelectedLoc]);
            else knobEditor1.ia = null;
            knobEditor1.Invalidate();

            Form1.UpdateTimeLine = true;
        }
    }
}
