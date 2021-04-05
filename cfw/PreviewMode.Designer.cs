namespace cfw
{
    partial class PreviewMode
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxImg = new System.Windows.Forms.CheckBox();
            this.checkBoxDoc = new System.Windows.Forms.CheckBox();
            this.checkBoxPdf = new System.Windows.Forms.CheckBox();
            this.checkBoxZip = new System.Windows.Forms.CheckBox();
            this.checkBoxHtm = new System.Windows.Forms.CheckBox();
            this.checkBoxAsIs = new System.Windows.Forms.CheckBox();
            this.checkBoxWmpAudio = new System.Windows.Forms.CheckBox();
            this.checkBoxWmpVideo = new System.Windows.Forms.CheckBox();
            this.checkBoxCfwVideo = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(34, 268);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(160, 268);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxImg
            // 
            this.checkBoxImg.AutoSize = true;
            this.checkBoxImg.Location = new System.Drawing.Point(22, 21);
            this.checkBoxImg.Name = "checkBoxImg";
            this.checkBoxImg.Size = new System.Drawing.Size(162, 17);
            this.checkBoxImg.TabIndex = 2;
            this.checkBoxImg.Text = "Images (jpg, bmp, png, tif, ...)";
            this.checkBoxImg.UseVisualStyleBackColor = true;
            // 
            // checkBoxDoc
            // 
            this.checkBoxDoc.AutoSize = true;
            this.checkBoxDoc.Location = new System.Drawing.Point(22, 114);
            this.checkBoxDoc.Name = "checkBoxDoc";
            this.checkBoxDoc.Size = new System.Drawing.Size(233, 17);
            this.checkBoxDoc.TabIndex = 3;
            this.checkBoxDoc.Text = "Word, Excel, PowerPoint, Visio (all versions)";
            this.checkBoxDoc.UseVisualStyleBackColor = true;
            // 
            // checkBoxPdf
            // 
            this.checkBoxPdf.AutoSize = true;
            this.checkBoxPdf.Location = new System.Drawing.Point(22, 137);
            this.checkBoxPdf.Name = "checkBoxPdf";
            this.checkBoxPdf.Size = new System.Drawing.Size(47, 17);
            this.checkBoxPdf.TabIndex = 4;
            this.checkBoxPdf.Text = "PDF";
            this.checkBoxPdf.UseVisualStyleBackColor = true;
            // 
            // checkBoxZip
            // 
            this.checkBoxZip.AutoSize = true;
            this.checkBoxZip.Location = new System.Drawing.Point(22, 160);
            this.checkBoxZip.Name = "checkBoxZip";
            this.checkBoxZip.Size = new System.Drawing.Size(43, 17);
            this.checkBoxZip.TabIndex = 5;
            this.checkBoxZip.Text = "ZIP";
            this.checkBoxZip.UseVisualStyleBackColor = true;
            // 
            // checkBoxHtm
            // 
            this.checkBoxHtm.AutoSize = true;
            this.checkBoxHtm.Location = new System.Drawing.Point(22, 183);
            this.checkBoxHtm.Name = "checkBoxHtm";
            this.checkBoxHtm.Size = new System.Drawing.Size(197, 17);
            this.checkBoxHtm.TabIndex = 6;
            this.checkBoxHtm.Text = "all HTML, Outlook MSG, MHT, EML";
            this.checkBoxHtm.UseVisualStyleBackColor = true;
            // 
            // checkBoxAsIs
            // 
            this.checkBoxAsIs.AutoSize = true;
            this.checkBoxAsIs.Location = new System.Drawing.Point(22, 206);
            this.checkBoxAsIs.Name = "checkBoxAsIs";
            this.checkBoxAsIs.Size = new System.Drawing.Size(71, 17);
            this.checkBoxAsIs.TabIndex = 7;
            this.checkBoxAsIs.Text = "file as it is";
            this.checkBoxAsIs.UseVisualStyleBackColor = true;
            // 
            // checkBoxWmpAudio
            // 
            this.checkBoxWmpAudio.AutoSize = true;
            this.checkBoxWmpAudio.Location = new System.Drawing.Point(22, 44);
            this.checkBoxWmpAudio.Name = "checkBoxWmpAudio";
            this.checkBoxWmpAudio.Size = new System.Drawing.Size(200, 17);
            this.checkBoxWmpAudio.TabIndex = 8;
            this.checkBoxWmpAudio.Text = "Media Player Sound Files (mp3, wav)";
            this.checkBoxWmpAudio.UseVisualStyleBackColor = true;
            // 
            // checkBoxWmpVideo
            // 
            this.checkBoxWmpVideo.AutoSize = true;
            this.checkBoxWmpVideo.Location = new System.Drawing.Point(22, 67);
            this.checkBoxWmpVideo.Name = "checkBoxWmpVideo";
            this.checkBoxWmpVideo.Size = new System.Drawing.Size(194, 17);
            this.checkBoxWmpVideo.TabIndex = 9;
            this.checkBoxWmpVideo.Text = "Media Player Video (wmv, avi, mov)";
            this.checkBoxWmpVideo.UseVisualStyleBackColor = true;
            this.checkBoxWmpVideo.Click += new System.EventHandler(this.checkBoxWmpVideo_Click);
            // 
            // checkBoxCfwVideo
            // 
            this.checkBoxCfwVideo.AutoSize = true;
            this.checkBoxCfwVideo.Location = new System.Drawing.Point(22, 90);
            this.checkBoxCfwVideo.Name = "checkBoxCfwVideo";
            this.checkBoxCfwVideo.Size = new System.Drawing.Size(160, 17);
            this.checkBoxCfwVideo.TabIndex = 10;
            this.checkBoxCfwVideo.Text = "CfW Video Player (wmv, avi)";
            this.checkBoxCfwVideo.UseVisualStyleBackColor = true;
            this.checkBoxCfwVideo.Click += new System.EventHandler(this.checkBoxCfwVideo_Click);
            // 
            // PreviewMode
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 313);
            this.Controls.Add(this.checkBoxCfwVideo);
            this.Controls.Add(this.checkBoxWmpVideo);
            this.Controls.Add(this.checkBoxWmpAudio);
            this.Controls.Add(this.checkBoxAsIs);
            this.Controls.Add(this.checkBoxHtm);
            this.Controls.Add(this.checkBoxZip);
            this.Controls.Add(this.checkBoxPdf);
            this.Controls.Add(this.checkBoxDoc);
            this.Controls.Add(this.checkBoxImg);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreviewMode";
            this.Text = "PreviewMode";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxImg;
        private System.Windows.Forms.CheckBox checkBoxDoc;
        private System.Windows.Forms.CheckBox checkBoxPdf;
        private System.Windows.Forms.CheckBox checkBoxZip;
        private System.Windows.Forms.CheckBox checkBoxHtm;
        private System.Windows.Forms.CheckBox checkBoxAsIs;
        private System.Windows.Forms.CheckBox checkBoxWmpAudio;
        private System.Windows.Forms.CheckBox checkBoxWmpVideo;
        private System.Windows.Forms.CheckBox checkBoxCfwVideo;
    }
}