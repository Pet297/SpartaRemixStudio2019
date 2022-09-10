namespace SpartaRemixStudio2019
{
    partial class CollectionEditor
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
            this.SuspendLayout();
            // 
            // CollectionEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.DoubleBuffered = true;
            this.Name = "CollectionEditor";
            this.Size = new System.Drawing.Size(165, 409);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CollectionEditor_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.CollectionEditor_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CollectionEditor_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.CollectionEditor_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
