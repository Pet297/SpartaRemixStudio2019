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
    public partial class AudioFXEdit : Form
    {
        List<IAudioFX> reflist;

        string[] buttonText = new string[64] {"LP","HP","LPF","HPF","FF-COMB","FB-COMB","","",
        "Delay","Reverb","","Reverb2","","","","",
        "","","WahWah","","Flange","","","",
        "","","","","","","","",
        "","","","","","","","",
        "","","","","","","","",
        "","","","","","","","",
        "","","","","","","",""};

        public AudioFXEdit(List<IAudioFX> list)
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

            reflist = list;
            collectionEditor1.afx = reflist;
        }

        void PressButton(int number)
        {
            IAudioFX fx = EffectHelper.AFXFromNumber((ushort)number);
            if (fx != null) collectionEditor1.afx.Add(fx);
            collectionEditor1.Invalidate();

            Form1.UpdateTimeLine = true;
        }

        private void collectionEditor1_MouseUp(object sender, MouseEventArgs e)
        {
            if (collectionEditor1.SelectedLoc >= 0 && collectionEditor1.SelectedLoc < collectionEditor1.afx.Count)
                knobEditor1.Load(collectionEditor1.afx[collectionEditor1.SelectedLoc]);
            else knobEditor1.ia = null;
            knobEditor1.Invalidate();

            Form1.UpdateTimeLine = true;
        }
    }
}
