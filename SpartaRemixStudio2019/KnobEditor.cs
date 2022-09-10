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
    public partial class KnobEditor : UserControl
    {
        ScrollableValue[] svs;
        Label[] l1s;
        Label[] l2s;
        Button[] bts;

        public KnobEditor()
        {
            InitializeComponent();

            svs = new ScrollableValue[12];
            l1s = new Label[12];
            l2s = new Label[12];
            bts = new Button[12];

            for (int i = 0; i < 12; i++)
            {
                svs[i] = new ScrollableValue();
                l1s[i] = new Label();
                l2s[i] = new Label();
                bts[i] = new Button();

                svs[i].BackColor = System.Drawing.Color.Black;
                svs[i].Location = new System.Drawing.Point(4, 4 + 27 * i);
                svs[i].Size = new System.Drawing.Size(71, 21);

                l1s[i].Location = new System.Drawing.Point(81, 8 + 27 * i);
                l1s[i].Size = new System.Drawing.Size(66, 21);
                l1s[i].Text = "PVal------";

                l2s[i].Location = new System.Drawing.Point(150, 8 + 27 * i);
                l2s[i].Size = new System.Drawing.Size(76, 21);
                l2s[i].Text = "PText------";

                bts[i].Location = new System.Drawing.Point(233, 4 + 27 * i);
                bts[i].Size = new System.Drawing.Size(36, 21);
                bts[i].Text = "NFX";
                bts[i].UseVisualStyleBackColor = true;

                this.Controls.Add(svs[i]);
                this.Controls.Add(l1s[i]);
                this.Controls.Add(l2s[i]);
                this.Controls.Add(bts[i]);

                svs[i].Hide();
                l1s[i].Hide();
                l2s[i].Hide();
                bts[i].Hide();

                int j = i;
                bts[i].Click += (s, e) => ClickOn(j);
                svs[i].ValueChanged += (s, e) => Changed(j);
            }

            ScrollUpdate();
        }

        public IAutomatable ia;
        int Scroll = 0;
        int NumFx = 0;

        public void Load(IAutomatable ia)
        {
            NumFx = ia.FXFloatValueCount;
            this.ia = ia;
            ScrollUpdate();
        }
        private void ScrollUpdate()
        {
            if (NumFx < 12)
            {
                button13.Hide();
                button14.Hide();
            }
            else
            {
                button13.Show();
                button14.Show();
                button13.Enabled = true;
                button14.Enabled = true;
                if (Scroll == 0) button14.Enabled = false;
                if (Scroll == (NumFx - 1) / 12) button13.Enabled = false;
            }
            
            for (int i = 0; i < 12; i++)
            {
                if (Scroll * 12 + i < NumFx)
                {
                    svs[i].Show();
                    l1s[i].Show();
                    l2s[i].Show();
                    bts[i].Show();

                    Tuple<float, string, bool, float, float> t = ia.GetValues(i + Scroll * 12);
                    if (t.Item3) svs[i].SetUnit(t.Item1, t.Item2, t.Item4, t.Item5);
                    else svs[i].SetUnit(t.Item1, t.Item2);
                    svs[i].SetValue(ia.GetFloatValueReference(i + Scroll * 12).BaseValue);
                    l1s[i].Text = ia.GetAutomatedValueName(Scroll * 12 + i);
                    l2s[i].Text = ia.ConvertFXFToNumber(i + Scroll * 12, ia.GetFloatValueReference(i + Scroll * 12).BaseValue);
                }
                else
                {
                    svs[i].Hide();
                    l1s[i].Hide();
                    l2s[i].Hide();
                    bts[i].Hide();
                }
            }
        }

        private void ClickOn(int i)
        {
            NumberFXEdit nfxe = new NumberFXEdit(ia.GetFloatValueReference(Scroll * 12 + i).Nfx);
            nfxe.Show();
        }
        private void Changed(int i)
        {
            if (ia != null)
            {
                ia.GetFloatValueReference(Scroll * 12 + i).BaseValue = svs[i].GetValue();
                l2s[i].Text = ia.ConvertFXFToNumber(i + Scroll * 12, ia.GetFloatValueReference(i + Scroll * 12).BaseValue);
            }
        }
    }
}
