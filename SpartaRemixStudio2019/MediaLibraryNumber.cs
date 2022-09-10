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
    public partial class MediaLibraryNumber : UserControl
    {
        public MediaLibraryNumber()
        {
            InitializeComponent();
        }

        float step = 0.001f;
        string format = "0.001";
        private void ChangeStepValue(object sender, EventArgs e)
        {
            if (radioButton3.Checked) step = 0.001f;
            if (radioButton4.Checked) step = 0.01f;
            if (radioButton5.Checked) step = 0.1f;
            if (radioButton6.Checked) step = 1f;
            if (radioButton7.Checked) step = 10f;

            if (radioButton3.Checked) format = "0.000";
            if (radioButton4.Checked) format = "0.00";
            if (radioButton5.Checked) format = "0.0";
            if (radioButton6.Checked) format = "0.#";
            if (radioButton7.Checked) format = "0.#";

            scrollableValue1.SetUnit(step, format);
            scrollableValue2.SetUnit(step, format);
            scrollableValue3.SetUnit(step, format);
            scrollableValue4.SetUnit(step, format);
            scrollableValue5.SetUnit(step, format);
            scrollableValue6.SetUnit(step, format);
            scrollableValue7.SetUnit(step, format);
            scrollableValue8.SetUnit(step, format);
        }

        public TimelineMediaType GetTLT()
        {
            if (radioButton1.Checked) return new TLNumber() { Number = scrollableValue1.GetValue() };
            if (radioButton2.Checked) return new TLNumberTrans() { Number0 = scrollableValue1.GetValue(), Number1 = scrollableValue2.GetValue() };
            return null;
        }
    }
}
