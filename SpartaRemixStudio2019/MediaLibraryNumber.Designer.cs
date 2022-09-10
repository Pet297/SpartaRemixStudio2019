namespace SpartaRemixStudio2019
{
    partial class MediaLibraryNumber
    {
        /// <summary> 
        /// Vyžaduje se proměnná návrháře.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Uvolněte všechny používané prostředky.
        /// </summary>
        /// <param name="disposing">hodnota true, když by se měl spravovaný prostředek odstranit; jinak false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kód vygenerovaný pomocí Návrháře komponent

        /// <summary> 
        /// Metoda vyžadovaná pro podporu Návrháře - neupravovat
        /// obsah této metody v editoru kódu.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.scrollableValue1 = new SpartaRemixStudio2019.ScrollableValue();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton13 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.scrollableValue2 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue3 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue4 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue5 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue6 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue7 = new SpartaRemixStudio2019.ScrollableValue();
            this.scrollableValue8 = new SpartaRemixStudio2019.ScrollableValue();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(36, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "X value";
            // 
            // scrollableValue1
            // 
            this.scrollableValue1.BackColor = System.Drawing.Color.Black;
            this.scrollableValue1.Location = new System.Drawing.Point(39, 26);
            this.scrollableValue1.Name = "scrollableValue1";
            this.scrollableValue1.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue1.TabIndex = 2;
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.ForeColor = System.Drawing.Color.White;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(78, 17);
            this.radioButton1.TabIndex = 5;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Float Value";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.ForeColor = System.Drawing.Color.White;
            this.radioButton2.Location = new System.Drawing.Point(6, 39);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(97, 17);
            this.radioButton2.TabIndex = 6;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Float Transition";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton7);
            this.groupBox1.Controls.Add(this.radioButton6);
            this.groupBox1.Controls.Add(this.radioButton5);
            this.groupBox1.Controls.Add(this.radioButton4);
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(6, 111);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(98, 133);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Step";
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Location = new System.Drawing.Point(6, 110);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(37, 17);
            this.radioButton7.TabIndex = 12;
            this.radioButton7.TabStop = true;
            this.radioButton7.Text = "10";
            this.radioButton7.UseVisualStyleBackColor = true;
            this.radioButton7.CheckedChanged += new System.EventHandler(this.ChangeStepValue);
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Location = new System.Drawing.Point(6, 88);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(31, 17);
            this.radioButton6.TabIndex = 11;
            this.radioButton6.TabStop = true;
            this.radioButton6.Text = "1";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.CheckedChanged += new System.EventHandler(this.ChangeStepValue);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(6, 65);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(40, 17);
            this.radioButton5.TabIndex = 10;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "0.1";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.ChangeStepValue);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(6, 42);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(46, 17);
            this.radioButton4.TabIndex = 9;
            this.radioButton4.Text = "0.01";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.ChangeStepValue);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = true;
            this.radioButton3.ForeColor = System.Drawing.Color.White;
            this.radioButton3.Location = new System.Drawing.Point(6, 19);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(52, 17);
            this.radioButton3.TabIndex = 8;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "0.001";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.ChangeStepValue);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton12);
            this.groupBox2.Controls.Add(this.radioButton13);
            this.groupBox2.Controls.Add(this.radioButton10);
            this.groupBox2.Controls.Add(this.radioButton11);
            this.groupBox2.Controls.Add(this.radioButton8);
            this.groupBox2.Controls.Add(this.radioButton9);
            this.groupBox2.Controls.Add(this.radioButton1);
            this.groupBox2.Controls.Add(this.radioButton2);
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(110, 111);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(216, 133);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Media Type";
            // 
            // radioButton12
            // 
            this.radioButton12.AutoSize = true;
            this.radioButton12.ForeColor = System.Drawing.Color.White;
            this.radioButton12.Location = new System.Drawing.Point(110, 74);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(80, 17);
            this.radioButton12.TabIndex = 11;
            this.radioButton12.TabStop = true;
            this.radioButton12.Text = "Vec4 Value";
            this.radioButton12.UseVisualStyleBackColor = true;
            // 
            // radioButton13
            // 
            this.radioButton13.AutoSize = true;
            this.radioButton13.ForeColor = System.Drawing.Color.White;
            this.radioButton13.Location = new System.Drawing.Point(110, 94);
            this.radioButton13.Name = "radioButton13";
            this.radioButton13.Size = new System.Drawing.Size(99, 17);
            this.radioButton13.TabIndex = 12;
            this.radioButton13.TabStop = true;
            this.radioButton13.Text = "Vec4 Transition";
            this.radioButton13.UseVisualStyleBackColor = true;
            // 
            // radioButton10
            // 
            this.radioButton10.AutoSize = true;
            this.radioButton10.ForeColor = System.Drawing.Color.White;
            this.radioButton10.Location = new System.Drawing.Point(110, 19);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(80, 17);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.TabStop = true;
            this.radioButton10.Text = "Vec3 Value";
            this.radioButton10.UseVisualStyleBackColor = true;
            // 
            // radioButton11
            // 
            this.radioButton11.AutoSize = true;
            this.radioButton11.ForeColor = System.Drawing.Color.White;
            this.radioButton11.Location = new System.Drawing.Point(110, 39);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(99, 17);
            this.radioButton11.TabIndex = 10;
            this.radioButton11.TabStop = true;
            this.radioButton11.Text = "Vec3 Transition";
            this.radioButton11.UseVisualStyleBackColor = true;
            // 
            // radioButton8
            // 
            this.radioButton8.AutoSize = true;
            this.radioButton8.ForeColor = System.Drawing.Color.White;
            this.radioButton8.Location = new System.Drawing.Point(6, 74);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(80, 17);
            this.radioButton8.TabIndex = 7;
            this.radioButton8.TabStop = true;
            this.radioButton8.Text = "Vec2 Value";
            this.radioButton8.UseVisualStyleBackColor = true;
            // 
            // radioButton9
            // 
            this.radioButton9.AutoSize = true;
            this.radioButton9.ForeColor = System.Drawing.Color.White;
            this.radioButton9.Location = new System.Drawing.Point(6, 94);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(99, 17);
            this.radioButton9.TabIndex = 8;
            this.radioButton9.TabStop = true;
            this.radioButton9.Text = "Vec2 Transition";
            this.radioButton9.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "From";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(3, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "To";
            // 
            // scrollableValue2
            // 
            this.scrollableValue2.BackColor = System.Drawing.Color.Black;
            this.scrollableValue2.Location = new System.Drawing.Point(39, 50);
            this.scrollableValue2.Name = "scrollableValue2";
            this.scrollableValue2.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue2.TabIndex = 11;
            // 
            // scrollableValue3
            // 
            this.scrollableValue3.BackColor = System.Drawing.Color.Black;
            this.scrollableValue3.Location = new System.Drawing.Point(110, 26);
            this.scrollableValue3.Name = "scrollableValue3";
            this.scrollableValue3.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue3.TabIndex = 12;
            // 
            // scrollableValue4
            // 
            this.scrollableValue4.BackColor = System.Drawing.Color.Black;
            this.scrollableValue4.Location = new System.Drawing.Point(110, 50);
            this.scrollableValue4.Name = "scrollableValue4";
            this.scrollableValue4.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue4.TabIndex = 13;
            // 
            // scrollableValue5
            // 
            this.scrollableValue5.BackColor = System.Drawing.Color.Black;
            this.scrollableValue5.Location = new System.Drawing.Point(181, 26);
            this.scrollableValue5.Name = "scrollableValue5";
            this.scrollableValue5.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue5.TabIndex = 14;
            // 
            // scrollableValue6
            // 
            this.scrollableValue6.BackColor = System.Drawing.Color.Black;
            this.scrollableValue6.Location = new System.Drawing.Point(181, 50);
            this.scrollableValue6.Name = "scrollableValue6";
            this.scrollableValue6.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue6.TabIndex = 15;
            // 
            // scrollableValue7
            // 
            this.scrollableValue7.BackColor = System.Drawing.Color.Black;
            this.scrollableValue7.Location = new System.Drawing.Point(252, 26);
            this.scrollableValue7.Name = "scrollableValue7";
            this.scrollableValue7.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue7.TabIndex = 16;
            // 
            // scrollableValue8
            // 
            this.scrollableValue8.BackColor = System.Drawing.Color.Black;
            this.scrollableValue8.Location = new System.Drawing.Point(252, 50);
            this.scrollableValue8.Name = "scrollableValue8";
            this.scrollableValue8.Size = new System.Drawing.Size(65, 21);
            this.scrollableValue8.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(107, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Y value";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(178, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Z value";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(249, 10);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "W value";
            // 
            // MediaLibraryNumber
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.scrollableValue8);
            this.Controls.Add(this.scrollableValue7);
            this.Controls.Add(this.scrollableValue6);
            this.Controls.Add(this.scrollableValue5);
            this.Controls.Add(this.scrollableValue4);
            this.Controls.Add(this.scrollableValue3);
            this.Controls.Add(this.scrollableValue2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.scrollableValue1);
            this.Controls.Add(this.label1);
            this.Name = "MediaLibraryNumber";
            this.Size = new System.Drawing.Size(340, 253);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private ScrollableValue scrollableValue1;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton12;
        private System.Windows.Forms.RadioButton radioButton13;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.RadioButton radioButton11;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private ScrollableValue scrollableValue2;
        private ScrollableValue scrollableValue3;
        private ScrollableValue scrollableValue4;
        private ScrollableValue scrollableValue5;
        private ScrollableValue scrollableValue6;
        private ScrollableValue scrollableValue7;
        private ScrollableValue scrollableValue8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}
