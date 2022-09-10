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
    public partial class Form2 : Form
    {
        private void updateEditedPattern()
        {
            if (EditedPattern == 4294967295) EditedPattern = 0;
            label1.Text = "Pattern " + EditedPattern + ": " + (Form1.Project.ProjectPatterns[EditedPattern].Name == null ? "(NO NAME)" : Form1.Project.ProjectPatterns[EditedPattern].Name);
            textBox2.Text = Form1.Project.ProjectPatterns[EditedPattern].Name == null ? "" : Form1.Project.ProjectPatterns[EditedPattern].Name;
            pictureBox1.Invalidate();
        }
        private void updatePropertyTrack()
        {
            label4.Text = "Property " + EditedProperty + (Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(EditedProperty) ? ": Defined" : ": Not defined");
        }
        private void autoGenProperty(byte b)
        {
            if (b == 48)
            {
                if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(0))
                {
                    if (Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(48))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Remove(48);
                    }
                }
                else
                {
                    if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(48))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Add(48, new Automation(0));
                    }
                    Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[48].points.Clear();

                    foreach (AutomationPoint ap in Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[0].points)
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[48].points.Add(new AutomationPoint()
                        {
                            interpolationMethod = ap.interpolationMethod,
                            time = ap.time,
                            valueA = (float)Math.Pow(10, ap.valueA / 10),
                            valueB = (float)Math.Pow(10, ap.valueB / 10)
                        });
                    }
                }
            }
            else if (b == 49)
            {
                if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(4))
                {
                    if (Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(49))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Remove(49);
                    }
                }
                else
                {
                    if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(49))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Add(49, new Automation(4));
                    }
                    Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[49].points.Clear();

                    foreach (AutomationPoint ap in Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[4].points)
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[49].points.Add(new AutomationPoint()
                        {
                            interpolationMethod = ap.interpolationMethod,
                            time = ap.time,
                            valueA = ap.valueA,
                            valueB = ap.valueB
                        });
                    }
                }
            }
            else if (b == 50)
            {
                if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(2))
                {
                    if (Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(50))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Remove(50);
                    }
                }
                else
                {
                    if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(50))
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Add(50, new Automation(2));
                    }
                    Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[50].points.Clear();

                    foreach (AutomationPoint ap in Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[2].points)
                    {
                        Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks[50].points.Add(new AutomationPoint()
                        {
                            interpolationMethod = ap.interpolationMethod,
                            time = ap.time,
                            valueA = ap.valueA,
                            valueB = ap.valueB
                        });
                    }
                }
            }
        }

        public Form2()
        {
            InitializeComponent();
            patternPicker1.ItemPicked += (s, e) => { EditedPattern = patternPicker1.GetPattern(); updateEditedPattern(); };
            pictureBox1.MouseWheel += (s, e) => mouseWheel(s,e);
        }      

        uint EditedPattern = 0;
        byte EditedProperty = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            Pattern p = new Pattern();
            uint u = Form1.Project.AddPattern(p);
            EditedPattern = u;

            updateEditedPattern();
            Invalidate();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Form1.Project.ProjectPatterns[EditedPattern].Name = textBox2.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EditedProperty = 0;
            updatePropertyTrack();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            EditedProperty = 1;
            updatePropertyTrack();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            EditedProperty = 2;
            updatePropertyTrack();
        }
        private void button5_Click(object sender, EventArgs e)
        {
            EditedProperty = 3;
            updatePropertyTrack();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            byte.TryParse(textBox1.Text ,out EditedProperty);
            updatePropertyTrack();
        }

        private void button9_Click(object sender, EventArgs e) => autoGenProperty(48);
        private void button10_Click(object sender, EventArgs e) => autoGenProperty(49);
        private void button11_Click(object sender, EventArgs e) => autoGenProperty(50);

        //define / undefine
        private void button12_Click(object sender, EventArgs e)
        {
            if (!Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(EditedProperty))
            {
                if (EditedProperty == 2 || EditedProperty == 48 || EditedProperty == 50) Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Add(EditedProperty, new Automation(1));
                else Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Add(EditedProperty, new Automation(0));
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.ContainsKey(EditedProperty))
            {
                Form1.Project.ProjectPatterns[EditedPattern].PropertyTracks.Remove(EditedProperty);
            }
        }

        // PATTERN WINDOW
        public float from = 0;
        public float to = 384;
        public int subdivision = 192/16;
        public float noteFrom = 9.00f;
        public int noteHeight = 19;
        public int sideBarWidth = 50;
        public int topBarWidth = 10;
        public float beatMarginTostretch = 4f;

        int beatDelta = 96;

        float posXtoBeat(float pos)
        {
            pos -= sideBarWidth;
            float p = pos / (pictureBox1.Width - sideBarWidth);
            return (from * (1 - p)) + (to * p);
        }
        float beatToPosX(float beat)
        {
            float p = (beat - from) / (to - from);
            return sideBarWidth + p * (pictureBox1.Width - sideBarWidth);
        }
        float noteToPosY(long note)
        {
            float noteTo = -(pictureBox1.Height - topBarWidth) / noteHeight + noteFrom;
            float p = (note - noteFrom) / (noteTo - noteFrom);
            return topBarWidth + p * (pictureBox1.Height - topBarWidth);
        }
        float PosYToNote(float pos)
        {
            float noteTo = -(pictureBox1.Height - topBarWidth) / noteHeight + noteFrom;
            pos -= topBarWidth;
            float p = pos / (pictureBox1.Height - topBarWidth);
            return (noteFrom
                * (1 - p)) + (noteTo * p);
        }
        int RoundPos(float pos) => (int)Math.Round(pos / subdivision) * subdivision;

        private Pattern CurrentPattern => Form1.Project.ProjectPatterns[EditedPattern];

        private Note SelectedNoteSingle = null;
        private bool MouseDown = false;
        private bool StretchLeft = false;
        private bool StretchRight = false;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            int firstDivisiveLine = (int)(from / subdivision) * subdivision;
            for (int i = firstDivisiveLine; i < to; i+= subdivision)
            {
                int x = (int)beatToPosX(i);
                if (x >= sideBarWidth) e.Graphics.DrawLine(Pens.White, x, 0, x, Height);
                if (i % 96 == 0) e.Graphics.DrawLine(new Pen(Color.LightBlue, 3), x, 0, x, Height);
            }

            int firstNoteLine = (int)noteFrom;
            float noteTo = -(pictureBox1.Height - topBarWidth) / noteHeight + noteFrom;
            for (int i = firstNoteLine; i >= noteTo; i--)
            {
                e.Graphics.DrawLine(Pens.White, 0, noteToPosY(i), pictureBox1.Width, noteToPosY(i));
                e.Graphics.DrawString(i.ToString(), new Font("Arial", 8), Brushes.White, 0, noteToPosY(i));
            }
            //pouzit note from

            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                int x0 = (int)beatToPosX(n.BeatNoteOn);
                int x1 = (int)beatToPosX(n.BeatNoteOff);
                int y0 = (int)noteToPosY(n.Pitch);

                if (x0 < sideBarWidth) x0 = sideBarWidth;

                Brush b = Brushes.Green;
                if (n.VisualStyle % 4 == 1) b = Brushes.Yellow;
                if (n.VisualStyle % 4 == 2) b = Brushes.Blue;
                if (n.VisualStyle % 4 == 3) b = Brushes.Purple;

                e.Graphics.FillRectangle(b, x0 + 1, y0 + 1, x1 - x0 - 2, noteHeight - 2);
                e.Graphics.DrawString(n.Sample.ToString(), new Font("Arial", 8), (n.VisualStyle % 4 == 1) ? Brushes.Black : Brushes.White, x0+1, y0+1);

                string adittional = "";
                int modify = 0;
                if (ModifierKeys.HasFlag(Keys.Control)) modify += 1;
                if (ModifierKeys.HasFlag(Keys.Shift)) modify += 2;
                if (ModifierKeys.HasFlag(Keys.Alt)) modify += 4;

                if (modify == 0 + 0 + 0) adittional = n.volume.ToString("+0.0'dB';-0.0'dB';''");
                if (modify == 1 + 0 + 0) adittional = n.pan.ToString("'L-'0.00;'R-'0.00;'C'");
                if (modify == 0 + 2 + 0) adittional = n.opacity.ToString("0.00");
                if (modify == 0 + 0 + 4) adittional = n.VisualStyle.ToString("'VS-'0");
                e.Graphics.DrawString(adittional, new Font("Arial", 8), Brushes.White, x0 + 1, y0 + 1 + noteHeight);
            }

            e.Graphics.FillRectangle(Brushes.White, 0, 0, pictureBox1.Width, topBarWidth);
            for (int i = firstDivisiveLine; i < to; i+=beatDelta)
            {
                e.Graphics.DrawString((i / 192) + " " + ((i % 192) / beatDelta) + "/" + (192 / beatDelta), new Font("Arial", 8), Brushes.Black, beatToPosX(i), 0);
            }

        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int time = (int)posXtoBeat(e.X);
            int note = (int)Math.Floor(PosYToNote(e.Y))+1;

            Note noteContained = null;
            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                if (n.Pitch == note && n.BeatNoteOn <= time && n.BeatNoteOff > time)
                {
                    noteContained = n;
                    MouseDown = true;
                    break;
                }
            }
            if (noteContained == null && e.Button == MouseButtons.Left)
            {
                Note n = new Note(note, smp, RoundPos(time), RoundPos(time) + subdivision, 0, 0);
                CurrentPattern.ActivationTrack.Notes.Add(n);
                CurrentPattern.ActivationTrack.SortNotes();
                MouseDown = true;
                SelectedNoteSingle = n;
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (CurrentPattern.ActivationTrack.Notes.Contains(noteContained)) CurrentPattern.ActivationTrack.Notes.Remove(noteContained);
                    noteContained = null;
                    CurrentPattern.ActivationTrack.SortNotes();
                }
                else if (e.Button == MouseButtons.Left)
                {
                    MouseDown = true;
                    SelectedNoteSingle = noteContained;

                    if (posXtoBeat(e.X) - SelectedNoteSingle.BeatNoteOn < beatMarginTostretch)
                    {
                        StretchLeft = true;
                    }
                    else if (SelectedNoteSingle.BeatNoteOff - posXtoBeat(e.X) < beatMarginTostretch)
                    {
                        StretchRight = true;
                    }
                }
            }

            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseDown && e.Button == MouseButtons.Left)
            {
                int time = (int)posXtoBeat(e.X);
                int note = (int)Math.Floor(PosYToNote(e.Y)) + 1;
                if (SelectedNoteSingle != null && !StretchLeft && !StretchRight)
                {
                    long lenght = SelectedNoteSingle.BeatNoteOff - SelectedNoteSingle.BeatNoteOn;
                    SelectedNoteSingle.BeatNoteOn = RoundPos(time);
                    SelectedNoteSingle.BeatNoteOff = RoundPos(time) + lenght;
                    SelectedNoteSingle.Pitch = note;
                }
                else if (SelectedNoteSingle != null && StretchLeft)
                {
                    if (time <= SelectedNoteSingle.BeatNoteOff) SelectedNoteSingle.BeatNoteOn = RoundPos(time);
                }
                else if (SelectedNoteSingle != null && StretchRight)
                {
                    if (time >= SelectedNoteSingle.BeatNoteOn) SelectedNoteSingle.BeatNoteOff = RoundPos(time);
                }
            }

            pictureBox1.Invalidate();
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseDown = false;
            StretchLeft = false;
            StretchRight = false;
            if (SelectedNoteSingle != null)
            {
                if (SelectedNoteSingle.BeatNoteOn >= SelectedNoteSingle.BeatNoteOff)
                {
                    if (CurrentPattern.ActivationTrack.Notes.Contains(SelectedNoteSingle))
                    {
                        CurrentPattern.ActivationTrack.Notes.Remove(SelectedNoteSingle);
                        CurrentPattern.ActivationTrack.SortNotes();
                    }
                    SelectedNoteSingle = null;
                }
            }
            pictureBox1.Invalidate();

            if (!Form1.patternsEdited.Contains(EditedPattern)) Form1.patternsEdited.Add(EditedPattern);
        }
        private void mouseWheel(object sender, MouseEventArgs e)
        {
            Note noteContained = null;
            int time = (int)posXtoBeat(e.X);
            int note = (int)Math.Floor(PosYToNote(e.Y)) + 1;
            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                if (n.Pitch == note && n.BeatNoteOn <= time && n.BeatNoteOff > time)
                {
                    noteContained = n;
                    MouseDown = true;
                    break;
                }
            }

            if (noteContained != null)
            {
                int modify = 0;
                if (ModifierKeys.HasFlag(Keys.Control)) modify += 1;
                if (ModifierKeys.HasFlag(Keys.Shift)) modify += 2;
                if (ModifierKeys.HasFlag(Keys.Alt)) modify += 4;

                if (modify == 0 + 0 + 0) noteContained.volume += 0.1f * e.Delta / 120f;
                if (modify == 1 + 0 + 0) noteContained.pan += 0.05f * e.Delta / 120f;
                if (modify == 0 + 2 + 0) noteContained.opacity += 0.05f * e.Delta / 120f;
                if (modify == 1 + 0 + 4) noteContained.Sample += e.Delta / 120;
                if (modify == 0 + 0 + 4) noteContained.VisualStyle += e.Delta / 120;
            }

            pictureBox1.Invalidate();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            noteFrom = - vScrollBar1.Value / 10f + 66;
            pictureBox1.Invalidate();
        }

        //property editor
        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {

        }

        private bool duplet = true;
        private int fract = 8;
        private void button14_Click(object sender, EventArgs e)
        {
            duplet = true;
            UpdateSubdivision();
        }
        private void button15_Click(object sender, EventArgs e)
        {
            duplet = false;
            UpdateSubdivision();
        }
        private void button16_Click(object sender, EventArgs e)
        {
            fract = 4;
            UpdateSubdivision();
        }
        private void button17_Click(object sender, EventArgs e)
        {
            fract = 8;
            UpdateSubdivision();
        }
        private void button18_Click(object sender, EventArgs e)
        {
            fract = 16;
            UpdateSubdivision();
        }
        private void button19_Click(object sender, EventArgs e)
        {
            fract = 32;
            UpdateSubdivision();
        }
        private void button20_Click(object sender, EventArgs e)
        {
            fract = 64;
            UpdateSubdivision();
        }
        private void button21_Click(object sender, EventArgs e)
        {
            fract = 128;
            UpdateSubdivision();
        }
        private void UpdateSubdivision()
        {
            if (duplet) subdivision = 192 / fract;
            else subdivision = 128 / fract;
            pictureBox1.Invalidate();
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            Pattern p = (Pattern)CurrentPattern.Clone();
            uint u = Form1.Project.AddPattern(p);
            EditedPattern = u;

            updateEditedPattern();
            Invalidate();
        }
        //copy 1
        private void button22_Click(object sender, EventArgs e)
        {
            int pitch0 = 0;
            int pitch1 = 0;
            int pitch2 = 0;
            int pitch3 = 0;

            int.TryParse(textBox3.Text, out pitch0);
            int.TryParse(textBox4.Text, out pitch1);
            int.TryParse(textBox5.Text, out pitch2);
            int.TryParse(textBox6.Text, out pitch3);


            List<int> ToRemove = new List<int>();
            int index = 0;
            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                if (n.BeatNoteOn >= 96) ToRemove.Add(index);
                index++;
            }
            ToRemove.Reverse();
            foreach (int i in ToRemove)
            {
                CurrentPattern.ActivationTrack.Notes.RemoveAt(i);
            }

            int nc = CurrentPattern.ActivationTrack.Notes.Count;

            for (int i = 0; i < nc; i++)
            {
                Note n1 = (Note)CurrentPattern.ActivationTrack.Notes[i].Clone();
                Note n2 = (Note)CurrentPattern.ActivationTrack.Notes[i].Clone();
                Note n3 = (Note)CurrentPattern.ActivationTrack.Notes[i].Clone();

                n1.BeatNoteOn += 96;
                n2.BeatNoteOn += 96*2;
                n3.BeatNoteOn += 96*3;
                n1.BeatNoteOff += 96;
                n2.BeatNoteOff += 96*2;
                n3.BeatNoteOff += 96*3;
                n1.Pitch += (pitch1 - pitch0);
                n2.Pitch += (pitch2 - pitch0);
                n3.Pitch += (pitch3 - pitch0);

                CurrentPattern.ActivationTrack.Notes.Add(n1);
                CurrentPattern.ActivationTrack.Notes.Add(n2);
                CurrentPattern.ActivationTrack.Notes.Add(n3);
            }
            CurrentPattern.ActivationTrack.SortNotes();
            pictureBox1.Invalidate();
        }
        //copy 2
        private void button23_Click(object sender, EventArgs e)
        {
            int pitch0 = 0;
            int pitch1 = 0;
            int pitch2 = 0;
            int pitch3 = 0;

            int.TryParse(textBox3.Text, out pitch0);
            int.TryParse(textBox4.Text, out pitch1);
            int.TryParse(textBox5.Text, out pitch2);
            int.TryParse(textBox6.Text, out pitch3);


            List<int> ToRemove = new List<int>();
            int index = 0;
            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                if (n.BeatNoteOn >= 192) ToRemove.Add(index);
                index++;
            }
            ToRemove.Reverse();
            foreach (int i in ToRemove)
            {
                CurrentPattern.ActivationTrack.Notes.RemoveAt(i);
            }

            int nc = CurrentPattern.ActivationTrack.Notes.Count;

            for (int i = 0; i < nc; i++)
            {
                Note n1 = (Note)CurrentPattern.ActivationTrack.Notes[i].Clone();

                n1.BeatNoteOn += 192;
                n1.BeatNoteOff += 192;
                if (CurrentPattern.ActivationTrack.Notes[i].BeatNoteOn < 96) n1.Pitch += (pitch2 - pitch0);
                else n1.Pitch += (pitch3 - pitch1);

                CurrentPattern.ActivationTrack.Notes.Add(n1);
            }
            CurrentPattern.ActivationTrack.SortNotes();
            pictureBox1.Invalidate();
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            from = hScrollBar1.Value;
            to = from + 384;
            pictureBox1.Invalidate();
        }
        //AF2
        private void button7_Click(object sender, EventArgs e)
        {
            int f = 0;
            foreach(Note n in CurrentPattern.ActivationTrack.Notes)
            {
                n.VisualStyle = f;
                f++;
                f %= 2;
            }
        }
        //AF4
        private void button24_Click(object sender, EventArgs e)
        {
            int f = 0;
            foreach (Note n in CurrentPattern.ActivationTrack.Notes)
            {
                n.VisualStyle = f;
                f++;
                f %= 4;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        int smp = 0;
        private void button25_Click(object sender, EventArgs e) => smp = 0;
        private void button26_Click(object sender, EventArgs e) => smp = 1;
        private void button27_Click(object sender, EventArgs e) => smp = 2;
        private void button28_Click(object sender, EventArgs e) => smp = 3;
    }
}
