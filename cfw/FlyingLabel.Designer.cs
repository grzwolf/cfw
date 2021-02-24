namespace cfw
{
    partial class FlyingLabel
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
            if (disposing && (components != null)) {
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
            this.label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.BackColor = System.Drawing.SystemColors.Highlight;
            this.label.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label.CausesValidation = false;
            this.label.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label.ForeColor = System.Drawing.SystemColors.Window;
            this.label.Location = new System.Drawing.Point(0, 0);
            this.label.Margin = new System.Windows.Forms.Padding(0);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(52, 15);
            this.label.TabIndex = 0;
            this.label.Text = "labelText";
            this.label.MouseDown += new System.Windows.Forms.MouseEventHandler(this.label_MouseDown);
            // 
            // FlyingLabel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(57, 19);
            this.ControlBox = false;
            this.Controls.Add(this.label);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FlyingLabel";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FlyingLabel";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label;
    }
}