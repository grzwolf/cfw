namespace cfw
{
    partial class HexViewerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && (components != null) ) {
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
            this.hexView1 = new cfw.HexView();
            this.SuspendLayout();
            // 
            // hexView1
            // 
            this.hexView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hexView1.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexView1.Location = new System.Drawing.Point(0, 0);
            this.hexView1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.hexView1.Name = "hexView1";
            this.hexView1.Size = new System.Drawing.Size(640, 736);
            this.hexView1.TabIndex = 0;
            // 
            // HexViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 736);
            this.Controls.Add(this.hexView1);
            this.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "HexViewerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hex Viewer";
            this.ResumeLayout(false);

        }

        #endregion

        private HexView hexView1;

    }
}