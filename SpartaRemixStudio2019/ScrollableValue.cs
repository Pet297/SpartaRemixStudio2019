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
    [Browsable(false)]
    public partial class ScrollableValue : UserControl
    {
        public event EventHandler ValueChanged;

        public float unit = 0.001f;
        public float max = 1f;
        public float min = 0f;
        public bool hasMax = false;
        public bool hasMin = false;
        public string format = "0.000";

        private int value = 0;

        public ScrollableValue()
        {
            InitializeComponent();
            this.MouseWheel += (s, e) => MouseScroll(s, e);
        }

        private void MouseScroll(object sender, MouseEventArgs e)
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                value += 20 * (int)(e.Delta / 120f);
            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                value += 100 * (int)(e.Delta / 120f);
            }
            else
            {
                value += (int)(e.Delta / 120f);
            }
            if (GetValue() > max && hasMax) SetValue(max);
            if (GetValue() < min && hasMin) SetValue(min);
            label1.Text = GetValue().ToString(format);
            ValueChanged?.Invoke(this, new EventArgs());
        }
        public float GetValue()
        {
            return unit * value;
        }
        public void SetUnit(float unit, string format)
        {
            hasMax = false;
            hasMin = false;
            this.unit = unit;
            this.format = format;

            label1.Text = GetValue().ToString(format);
        }
        public void SetUnit(float unit, string format, float min, float max)
        {
            hasMax = true;
            hasMin = true;
            this.max = max;
            this.min = min;
            this.unit = unit;
            this.format = format;

            label1.Text = GetValue().ToString(format);
        }
        public void SetValue(float value)
        {
            float v2 = value / unit;
            this.value = (int)Math.Round(v2);

            label1.Text = GetValue().ToString(format);
        }
    }
}
