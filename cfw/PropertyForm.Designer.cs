namespace cfw
{
    partial class PropertyForm
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
            this.buttonOkBreak = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelSelection = new System.Windows.Forms.Label();
            this.labelFolderCount = new System.Windows.Forms.Label();
            this.labelFileCount = new System.Windows.Forms.Label();
            this.labelTotalSize = new System.Windows.Forms.Label();
            this.labelExactSize = new System.Windows.Forms.Label();
            this.timerDelayedStart = new System.Windows.Forms.Timer(this.components);
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.labelSearchTime = new System.Windows.Forms.Label();
            this.labelCurrent = new System.Windows.Forms.TextBox();
            this.labelSpaceTotal = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.labelSpaceLeft = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOkBreak
            // 
            this.buttonOkBreak.Location = new System.Drawing.Point(112, 403);
            this.buttonOkBreak.Name = "buttonOkBreak";
            this.buttonOkBreak.Size = new System.Drawing.Size(75, 23);
            this.buttonOkBreak.TabIndex = 0;
            this.buttonOkBreak.Text = "Ok";
            this.buttonOkBreak.UseVisualStyleBackColor = true;
            this.buttonOkBreak.Click += new System.EventHandler(this.buttonOkBreak_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Folder Count:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 113);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "File Count:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Size:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Exact Size:";
            // 
            // labelSelection
            // 
            this.labelSelection.AutoSize = true;
            this.labelSelection.Location = new System.Drawing.Point(12, 24);
            this.labelSelection.Name = "labelSelection";
            this.labelSelection.Size = new System.Drawing.Size(49, 13);
            this.labelSelection.TabIndex = 5;
            this.labelSelection.Text = "selection";
            // 
            // labelFolderCount
            // 
            this.labelFolderCount.AutoSize = true;
            this.labelFolderCount.Location = new System.Drawing.Point(136, 76);
            this.labelFolderCount.Name = "labelFolderCount";
            this.labelFolderCount.Size = new System.Drawing.Size(35, 13);
            this.labelFolderCount.TabIndex = 6;
            this.labelFolderCount.Text = "label5";
            // 
            // labelFileCount
            // 
            this.labelFileCount.AutoSize = true;
            this.labelFileCount.Location = new System.Drawing.Point(136, 113);
            this.labelFileCount.Name = "labelFileCount";
            this.labelFileCount.Size = new System.Drawing.Size(35, 13);
            this.labelFileCount.TabIndex = 7;
            this.labelFileCount.Text = "label5";
            // 
            // labelTotalSize
            // 
            this.labelTotalSize.AutoSize = true;
            this.labelTotalSize.Location = new System.Drawing.Point(136, 151);
            this.labelTotalSize.Name = "labelTotalSize";
            this.labelTotalSize.Size = new System.Drawing.Size(35, 13);
            this.labelTotalSize.TabIndex = 8;
            this.labelTotalSize.Text = "label5";
            // 
            // labelExactSize
            // 
            this.labelExactSize.AutoSize = true;
            this.labelExactSize.Location = new System.Drawing.Point(136, 191);
            this.labelExactSize.Name = "labelExactSize";
            this.labelExactSize.Size = new System.Drawing.Size(35, 13);
            this.labelExactSize.TabIndex = 9;
            this.labelExactSize.Text = "label5";
            // 
            // timerDelayedStart
            // 
            this.timerDelayedStart.Interval = 10;
            this.timerDelayedStart.Tick += new System.EventHandler(this.timerDelayedStart_Tick);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Interval = 250;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // labelSearchTime
            // 
            this.labelSearchTime.AutoSize = true;
            this.labelSearchTime.Location = new System.Drawing.Point(9, 444);
            this.labelSearchTime.Name = "labelSearchTime";
            this.labelSearchTime.Size = new System.Drawing.Size(61, 13);
            this.labelSearchTime.TabIndex = 10;
            this.labelSearchTime.Text = "search time";
            // 
            // labelCurrent
            // 
            this.labelCurrent.Location = new System.Drawing.Point(12, 325);
            this.labelCurrent.Multiline = true;
            this.labelCurrent.Name = "labelCurrent";
            this.labelCurrent.ReadOnly = true;
            this.labelCurrent.Size = new System.Drawing.Size(280, 55);
            this.labelCurrent.TabIndex = 11;
            // 
            // labelSpaceTotal
            // 
            this.labelSpaceTotal.AutoSize = true;
            this.labelSpaceTotal.Location = new System.Drawing.Point(136, 247);
            this.labelSpaceTotal.Name = "labelSpaceTotal";
            this.labelSpaceTotal.Size = new System.Drawing.Size(10, 13);
            this.labelSpaceTotal.TabIndex = 13;
            this.labelSpaceTotal.Text = "-";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 247);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "HD Space:";
            // 
            // labelSpaceLeft
            // 
            this.labelSpaceLeft.AutoSize = true;
            this.labelSpaceLeft.Location = new System.Drawing.Point(136, 287);
            this.labelSpaceLeft.Name = "labelSpaceLeft";
            this.labelSpaceLeft.Size = new System.Drawing.Size(10, 13);
            this.labelSpaceLeft.TabIndex = 15;
            this.labelSpaceLeft.Text = "-";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 287);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(106, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "HD Space Available:";
            // 
            // PropertyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 473);
            this.Controls.Add(this.labelSpaceLeft);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.labelSpaceTotal);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.labelCurrent);
            this.Controls.Add(this.labelSearchTime);
            this.Controls.Add(this.labelExactSize);
            this.Controls.Add(this.labelTotalSize);
            this.Controls.Add(this.labelFileCount);
            this.Controls.Add(this.labelFolderCount);
            this.Controls.Add(this.labelSelection);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonOkBreak);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PropertyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Properties";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PropertyForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOkBreak;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label labelSelection;
        private System.Windows.Forms.Label labelFolderCount;
        private System.Windows.Forms.Label labelFileCount;
        private System.Windows.Forms.Label labelTotalSize;
        private System.Windows.Forms.Label labelExactSize;
        private System.Windows.Forms.Timer timerDelayedStart;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Label labelSearchTime;
        private System.Windows.Forms.TextBox labelCurrent;
        private System.Windows.Forms.Label labelSpaceTotal;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelSpaceLeft;
        private System.Windows.Forms.Label label7;
    }
}