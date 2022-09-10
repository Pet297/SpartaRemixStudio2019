namespace SpartaRemixStudio2019
{
    partial class VideoFXEdit
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.collectionEditor1 = new SpartaRemixStudio2019.CollectionEditor();
            this.knobEditor1 = new SpartaRemixStudio2019.KnobEditor();
            this.SuspendLayout();
            // 
            // collectionEditor1
            // 
            this.collectionEditor1.Location = new System.Drawing.Point(439, 13);
            this.collectionEditor1.Name = "collectionEditor1";
            this.collectionEditor1.SelectedLoc = 0;
            this.collectionEditor1.Size = new System.Drawing.Size(177, 425);
            this.collectionEditor1.TabIndex = 0;
            this.collectionEditor1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.collectionEditor1_MouseUp);
            // 
            // knobEditor1
            // 
            this.knobEditor1.Location = new System.Drawing.Point(623, 13);
            this.knobEditor1.Name = "knobEditor1";
            this.knobEditor1.Size = new System.Drawing.Size(311, 425);
            this.knobEditor1.TabIndex = 1;
            // 
            // VideoFXEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 450);
            this.Controls.Add(this.knobEditor1);
            this.Controls.Add(this.collectionEditor1);
            this.Name = "VideoFXEdit";
            this.Text = "VideoFXEdit";
            this.ResumeLayout(false);

        }

        #endregion

        private CollectionEditor collectionEditor1;
        private KnobEditor knobEditor1;
    }
}