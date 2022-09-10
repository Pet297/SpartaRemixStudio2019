namespace SpartaRemixStudio2019
{
    partial class MediaLibrary
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.mediaLibraryNumber1 = new SpartaRemixStudio2019.MediaLibraryNumber();
            this.mediaLibrarySample1 = new SpartaRemixStudio2019.MediaLibrarySample();
            this.mediaLibraryPattern1 = new SpartaRemixStudio2019.MediaLibraryPattern();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton6);
            this.groupBox1.Controls.Add(this.radioButton5);
            this.groupBox1.Controls.Add(this.radioButton4);
            this.groupBox1.Controls.Add(this.radioButton3);
            this.groupBox1.Controls.Add(this.radioButton2);
            this.groupBox1.Controls.Add(this.radioButton1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(435, 68);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Media Type";
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.ForeColor = System.Drawing.Color.White;
            this.radioButton6.Location = new System.Drawing.Point(309, 42);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(80, 17);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Text = "Truth Vaule";
            this.radioButton6.UseVisualStyleBackColor = true;
            this.radioButton6.CheckedChanged += new System.EventHandler(this.radioButton6_CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.ForeColor = System.Drawing.Color.White;
            this.radioButton5.Location = new System.Drawing.Point(309, 19);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(120, 17);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "Linear Comb. Coefs.";
            this.radioButton5.UseVisualStyleBackColor = true;
            this.radioButton5.CheckedChanged += new System.EventHandler(this.radioButton5_CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.ForeColor = System.Drawing.Color.White;
            this.radioButton4.Location = new System.Drawing.Point(160, 42);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(52, 17);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "String";
            this.radioButton4.UseVisualStyleBackColor = true;
            this.radioButton4.CheckedChanged += new System.EventHandler(this.radioButton4_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.ForeColor = System.Drawing.Color.White;
            this.radioButton3.Location = new System.Drawing.Point(160, 19);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(104, 17);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "Number / Vector";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.ForeColor = System.Drawing.Color.White;
            this.radioButton2.Location = new System.Drawing.Point(6, 42);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(59, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "Pattern";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.ForeColor = System.Drawing.Color.White;
            this.radioButton1.Location = new System.Drawing.Point(6, 19);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(92, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Sample/Video";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // mediaLibraryNumber1
            // 
            this.mediaLibraryNumber1.BackColor = System.Drawing.Color.Black;
            this.mediaLibraryNumber1.Location = new System.Drawing.Point(501, 118);
            this.mediaLibraryNumber1.Name = "mediaLibraryNumber1";
            this.mediaLibraryNumber1.Size = new System.Drawing.Size(340, 253);
            this.mediaLibraryNumber1.TabIndex = 3;
            // 
            // mediaLibrarySample1
            // 
            this.mediaLibrarySample1.BackColor = System.Drawing.Color.White;
            this.mediaLibrarySample1.Location = new System.Drawing.Point(6, 107);
            this.mediaLibrarySample1.Name = "mediaLibrarySample1";
            this.mediaLibrarySample1.Size = new System.Drawing.Size(324, 534);
            this.mediaLibrarySample1.TabIndex = 2;
            // 
            // mediaLibraryPattern1
            // 
            this.mediaLibraryPattern1.BackColor = System.Drawing.Color.Black;
            this.mediaLibraryPattern1.Location = new System.Drawing.Point(187, 74);
            this.mediaLibraryPattern1.Name = "mediaLibraryPattern1";
            this.mediaLibraryPattern1.Size = new System.Drawing.Size(459, 382);
            this.mediaLibraryPattern1.TabIndex = 1;
            // 
            // MediaLibrary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.mediaLibraryNumber1);
            this.Controls.Add(this.mediaLibrarySample1);
            this.Controls.Add(this.mediaLibraryPattern1);
            this.Controls.Add(this.groupBox1);
            this.Name = "MediaLibrary";
            this.Size = new System.Drawing.Size(435, 381);
            this.Resize += new System.EventHandler(this.MediaLibrary_Resize);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private MediaLibraryPattern mediaLibraryPattern1;
        private MediaLibrarySample mediaLibrarySample1;
        private MediaLibraryNumber mediaLibraryNumber1;
    }
}
