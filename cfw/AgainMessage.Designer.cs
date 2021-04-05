namespace cfw
{
    partial class AgainMessage
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
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            this.checkBoxAgain = new System.Windows.Forms.CheckBox();
            this.buttonKeepFile = new System.Windows.Forms.Button();
            this.buttonBreak = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.AcceptsReturn = true;
            this.textBoxMessage.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxMessage.Location = new System.Drawing.Point(12, 12);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.ReadOnly = true;
            this.textBoxMessage.Size = new System.Drawing.Size(517, 111);
            this.textBoxMessage.TabIndex = 0;
            // 
            // buttonYes
            // 
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.Location = new System.Drawing.Point(12, 146);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(75, 23);
            this.buttonYes.TabIndex = 1;
            this.buttonYes.Text = "Overwrite";
            this.buttonYes.UseVisualStyleBackColor = true;
            this.buttonYes.Click += new System.EventHandler(this.OnButtonYes);
            // 
            // buttonNo
            // 
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.Location = new System.Drawing.Point(308, 146);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 2;
            this.buttonNo.Text = "Skip File";
            this.buttonNo.UseVisualStyleBackColor = true;
            this.buttonNo.Click += new System.EventHandler(this.OnButtonNo);
            // 
            // checkBoxAgain
            // 
            this.checkBoxAgain.AutoSize = true;
            this.checkBoxAgain.Location = new System.Drawing.Point(12, 187);
            this.checkBoxAgain.Name = "checkBoxAgain";
            this.checkBoxAgain.Size = new System.Drawing.Size(233, 17);
            this.checkBoxAgain.TabIndex = 3;
            this.checkBoxAgain.Text = "Apply the same answer to the next conflicts ";
            this.checkBoxAgain.UseVisualStyleBackColor = true;
            // 
            // buttonKeepFile
            // 
            this.buttonKeepFile.Location = new System.Drawing.Point(158, 146);
            this.buttonKeepFile.Name = "buttonKeepFile";
            this.buttonKeepFile.Size = new System.Drawing.Size(75, 23);
            this.buttonKeepFile.TabIndex = 4;
            this.buttonKeepFile.Text = "Keep File";
            this.buttonKeepFile.UseVisualStyleBackColor = true;
            this.buttonKeepFile.Click += new System.EventHandler(this.ButtonKeepFile_Click);
            // 
            // buttonBreak
            // 
            this.buttonBreak.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonBreak.Location = new System.Drawing.Point(454, 146);
            this.buttonBreak.Name = "buttonBreak";
            this.buttonBreak.Size = new System.Drawing.Size(75, 23);
            this.buttonBreak.TabIndex = 5;
            this.buttonBreak.Text = "Cancel All";
            this.buttonBreak.UseVisualStyleBackColor = true;
            this.buttonBreak.Click += new System.EventHandler(this.buttonBreak_Click);
            // 
            // AgainMessage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(541, 230);
            this.ControlBox = false;
            this.Controls.Add(this.buttonBreak);
            this.Controls.Add(this.buttonKeepFile);
            this.Controls.Add(this.checkBoxAgain);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.textBoxMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "AgainMessage";
            this.Text = "AgainMessage";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.Button buttonYes;
        private System.Windows.Forms.Button buttonNo;
        public System.Windows.Forms.CheckBox checkBoxAgain;
        private System.Windows.Forms.Button buttonKeepFile;
        private System.Windows.Forms.Button buttonBreak;
    }
}