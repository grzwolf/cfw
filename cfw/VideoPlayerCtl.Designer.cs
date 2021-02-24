namespace VideoPlayer
{
    partial class VideoPlayerCtl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonFastFwd = new System.Windows.Forms.Button();
            this.buttonFastBack = new System.Windows.Forms.Button();
            this.checkBoxRepeat = new System.Windows.Forms.CheckBox();
            this.buttonReverse = new System.Windows.Forms.Button();
            this.buttonBeg = new System.Windows.Forms.Button();
            this.buttonEnd = new System.Windows.Forms.Button();
            this.buttonStepBack = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.buttonStepFwd = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonSnapshot = new System.Windows.Forms.Button();
            this.buttonFile = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.trackBar = new SliderBar.CustomTrackBar();
            this.contextMenuStripRecordVideo = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.injectImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timerRepeater = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 282F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.trackBar, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(437, 369);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonFastFwd);
            this.panel1.Controls.Add(this.buttonFastBack);
            this.panel1.Controls.Add(this.checkBoxRepeat);
            this.panel1.Controls.Add(this.buttonReverse);
            this.panel1.Controls.Add(this.buttonBeg);
            this.panel1.Controls.Add(this.buttonEnd);
            this.panel1.Controls.Add(this.buttonStepBack);
            this.panel1.Controls.Add(this.buttonPlay);
            this.panel1.Controls.Add(this.buttonStepFwd);
            this.panel1.Location = new System.Drawing.Point(0, 336);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(282, 30);
            this.panel1.TabIndex = 2;
            // 
            // buttonFastFwd
            // 
            this.buttonFastFwd.Location = new System.Drawing.Point(168, 4);
            this.buttonFastFwd.Margin = new System.Windows.Forms.Padding(0);
            this.buttonFastFwd.Name = "buttonFastFwd";
            this.buttonFastFwd.Size = new System.Drawing.Size(27, 23);
            this.buttonFastFwd.TabIndex = 8;
            this.buttonFastFwd.Text = ">>";
            this.buttonFastFwd.UseVisualStyleBackColor = true;
            this.buttonFastFwd.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonFastFwd_MouseDown);
            this.buttonFastFwd.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonFastFwd_MouseUp);
            // 
            // buttonFastBack
            // 
            this.buttonFastBack.Location = new System.Drawing.Point(28, 4);
            this.buttonFastBack.Margin = new System.Windows.Forms.Padding(0);
            this.buttonFastBack.Name = "buttonFastBack";
            this.buttonFastBack.Size = new System.Drawing.Size(27, 23);
            this.buttonFastBack.TabIndex = 7;
            this.buttonFastBack.Text = "<<";
            this.buttonFastBack.UseVisualStyleBackColor = true;
            this.buttonFastBack.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonFastBack_MouseDown);
            this.buttonFastBack.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonFastBack_MouseUp);
            // 
            // checkBoxRepeat
            // 
            this.checkBoxRepeat.AutoSize = true;
            this.checkBoxRepeat.Location = new System.Drawing.Point(242, 8);
            this.checkBoxRepeat.Name = "checkBoxRepeat";
            this.checkBoxRepeat.Size = new System.Drawing.Size(37, 17);
            this.checkBoxRepeat.TabIndex = 6;
            this.checkBoxRepeat.Text = "@";
            this.checkBoxRepeat.UseVisualStyleBackColor = true;
            // 
            // buttonReverse
            // 
            this.buttonReverse.Location = new System.Drawing.Point(82, 4);
            this.buttonReverse.Margin = new System.Windows.Forms.Padding(0);
            this.buttonReverse.Name = "buttonReverse";
            this.buttonReverse.Size = new System.Drawing.Size(27, 23);
            this.buttonReverse.TabIndex = 2;
            this.buttonReverse.Text = "<";
            this.buttonReverse.UseVisualStyleBackColor = true;
            this.buttonReverse.Click += new System.EventHandler(this.buttonReverse_Click);
            // 
            // buttonBeg
            // 
            this.buttonBeg.Location = new System.Drawing.Point(1, 4);
            this.buttonBeg.Margin = new System.Windows.Forms.Padding(0);
            this.buttonBeg.Name = "buttonBeg";
            this.buttonBeg.Size = new System.Drawing.Size(27, 23);
            this.buttonBeg.TabIndex = 0;
            this.buttonBeg.Text = "|<";
            this.buttonBeg.UseVisualStyleBackColor = true;
            this.buttonBeg.Click += new System.EventHandler(this.buttonBeg_Click);
            // 
            // buttonEnd
            // 
            this.buttonEnd.Location = new System.Drawing.Point(195, 4);
            this.buttonEnd.Margin = new System.Windows.Forms.Padding(0);
            this.buttonEnd.Name = "buttonEnd";
            this.buttonEnd.Size = new System.Drawing.Size(27, 23);
            this.buttonEnd.TabIndex = 5;
            this.buttonEnd.Text = ">|";
            this.buttonEnd.UseVisualStyleBackColor = true;
            this.buttonEnd.Click += new System.EventHandler(this.buttonEnd_Click);
            // 
            // buttonStepBack
            // 
            this.buttonStepBack.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonStepBack.Location = new System.Drawing.Point(55, 4);
            this.buttonStepBack.Margin = new System.Windows.Forms.Padding(0);
            this.buttonStepBack.Name = "buttonStepBack";
            this.buttonStepBack.Size = new System.Drawing.Size(27, 23);
            this.buttonStepBack.TabIndex = 1;
            this.buttonStepBack.Text = "<1";
            this.buttonStepBack.UseVisualStyleBackColor = true;
            this.buttonStepBack.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonBck_MouseDown);
            this.buttonStepBack.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonBck_MouseUp);
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(114, 4);
            this.buttonPlay.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(27, 23);
            this.buttonPlay.TabIndex = 3;
            this.buttonPlay.Text = ">";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // buttonStepFwd
            // 
            this.buttonStepFwd.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonStepFwd.Location = new System.Drawing.Point(141, 4);
            this.buttonStepFwd.Margin = new System.Windows.Forms.Padding(0);
            this.buttonStepFwd.Name = "buttonStepFwd";
            this.buttonStepFwd.Size = new System.Drawing.Size(27, 23);
            this.buttonStepFwd.TabIndex = 4;
            this.buttonStepFwd.Text = "1>";
            this.buttonStepFwd.UseVisualStyleBackColor = true;
            this.buttonStepFwd.MouseDown += new System.Windows.Forms.MouseEventHandler(this.buttonStepFwd_MouseDown);
            this.buttonStepFwd.MouseUp += new System.Windows.Forms.MouseEventHandler(this.buttonStepFwd_MouseUp);
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.buttonSnapshot);
            this.panel2.Controls.Add(this.buttonFile);
            this.panel2.Location = new System.Drawing.Point(284, 336);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(153, 33);
            this.panel2.TabIndex = 1;
            // 
            // buttonSnapshot
            // 
            this.buttonSnapshot.Location = new System.Drawing.Point(3, 4);
            this.buttonSnapshot.Name = "buttonSnapshot";
            this.buttonSnapshot.Size = new System.Drawing.Size(62, 23);
            this.buttonSnapshot.TabIndex = 1;
            this.buttonSnapshot.Text = "snapshot";
            this.buttonSnapshot.UseVisualStyleBackColor = true;
            this.buttonSnapshot.Click += new System.EventHandler(this.buttonSnapshot_Click);
            // 
            // buttonFile
            // 
            this.buttonFile.Location = new System.Drawing.Point(71, 1);
            this.buttonFile.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.buttonFile.Name = "buttonFile";
            this.buttonFile.Size = new System.Drawing.Size(82, 32);
            this.buttonFile.TabIndex = 0;
            this.buttonFile.Text = "Open Media";
            this.buttonFile.UseVisualStyleBackColor = true;
            this.buttonFile.Click += new System.EventHandler(this.buttonFile_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.pictureBox, 2);
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox.Location = new System.Drawing.Point(0, 20);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(437, 294);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox.TabIndex = 3;
            this.pictureBox.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tableLayoutPanel1.SetColumnSpan(this.panel3, 2);
            this.panel3.Controls.Add(this.labelStatus);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(437, 20);
            this.panel3.TabIndex = 5;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(3, 3);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(35, 13);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.Text = "label1";
            // 
            // trackBar
            // 
            this.trackBar.BackColor = System.Drawing.Color.Transparent;
            this.trackBar.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.tableLayoutPanel1.SetColumnSpan(this.trackBar, 2);
            this.trackBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBar.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.trackBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(125)))), ((int)(((byte)(123)))));
            this.trackBar.IndentHeight = 3;
            this.trackBar.IndentWidth = 3;
            this.trackBar.Location = new System.Drawing.Point(0, 314);
            this.trackBar.Margin = new System.Windows.Forms.Padding(0);
            this.trackBar.Maximum = 10;
            this.trackBar.Minimum = 0;
            this.trackBar.Name = "trackBar";
            this.trackBar.Size = new System.Drawing.Size(437, 19);
            this.trackBar.TabIndex = 6;
            this.trackBar.TextTickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(146)))), ((int)(((byte)(148)))));
            this.trackBar.TickHeight = 1;
            this.trackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBar.TrackerColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(130)))), ((int)(((byte)(198)))));
            this.trackBar.TrackerSize = new System.Drawing.Size(13, 13);
            this.trackBar.TrackLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(93)))), ((int)(((byte)(90)))));
            this.trackBar.TrackLineHeight = 1;
            this.trackBar.Value = 0;
            this.trackBar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBar_MouseDown);
            this.trackBar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.trackBar_MouseMove);
            // 
            // contextMenuStripRecordVideo
            // 
            this.contextMenuStripRecordVideo.Name = "contextMenuStripRecordVideo";
            this.contextMenuStripRecordVideo.Size = new System.Drawing.Size(61, 4);
            // 
            // injectImageToolStripMenuItem
            // 
            this.injectImageToolStripMenuItem.Name = "injectImageToolStripMenuItem";
            this.injectImageToolStripMenuItem.Size = new System.Drawing.Size(32, 19);
            // 
            // timerRepeater
            // 
            this.timerRepeater.Tick += new System.EventHandler(this.timerRepeater_Tick);
            // 
            // VideoPlayerCtl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(100, 20);
            this.Name = "VideoPlayerCtl";
            this.Size = new System.Drawing.Size(437, 369);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button buttonStepFwd;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonStepBack;
        private System.Windows.Forms.Button buttonBeg;
        private System.Windows.Forms.Button buttonEnd;
        private System.Windows.Forms.Button buttonReverse;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonFile;
        private System.Windows.Forms.Button buttonSnapshot;
        private System.Windows.Forms.CheckBox checkBoxRepeat;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRecordVideo;
        private System.Windows.Forms.ToolStripMenuItem injectImageToolStripMenuItem;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button buttonFastFwd;
        private System.Windows.Forms.Button buttonFastBack;
        private System.Windows.Forms.Timer timerRepeater;
        private SliderBar.CustomTrackBar trackBar;

    }
}
