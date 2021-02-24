namespace cfw
{
    partial class SimpleProgress
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
            this.components = new System.ComponentModel.Container();
            this.labelPct = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.labelTimer = new System.Windows.Forms.Label();
            this.labelText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPct
            // 
            this.labelPct.AutoSize = true;
            this.labelPct.Location = new System.Drawing.Point(127, 17);
            this.labelPct.Name = "labelPct";
            this.labelPct.Size = new System.Drawing.Size(45, 13);
            this.labelPct.TabIndex = 0;
            this.labelPct.Text = "labelPct";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(13, 50);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(272, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 1;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // labelTimer
            // 
            this.labelTimer.AutoSize = true;
            this.labelTimer.Location = new System.Drawing.Point(18, 109);
            this.labelTimer.Name = "labelTimer";
            this.labelTimer.Size = new System.Drawing.Size(10, 13);
            this.labelTimer.TabIndex = 2;
            this.labelTimer.Text = ".";
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(18, 87);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(10, 13);
            this.labelText.TabIndex = 3;
            this.labelText.Text = ".";
            // 
            // SimpleProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(298, 163);
            this.ControlBox = false;
            this.Controls.Add(this.labelText);
            this.Controls.Add(this.labelTimer);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelPct);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimpleProgress";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Progress";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPct;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label labelTimer;
        private System.Windows.Forms.Label labelText;
    }
}