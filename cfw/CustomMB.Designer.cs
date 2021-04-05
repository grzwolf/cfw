namespace cfw
{
    partial class CustomMB
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
            this.labelText = new System.Windows.Forms.Label();
            this.buttonCustom1 = new System.Windows.Forms.Button();
            this.buttonCustom2 = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelText
            // 
            this.labelText.AutoSize = true;
            this.labelText.Location = new System.Drawing.Point(22, 25);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(41, 13);
            this.labelText.TabIndex = 0;
            this.labelText.Text = "set text";
            // 
            // buttonCustom1
            // 
            this.buttonCustom1.Location = new System.Drawing.Point(207, 119);
            this.buttonCustom1.Name = "buttonCustom1";
            this.buttonCustom1.Size = new System.Drawing.Size(75, 49);
            this.buttonCustom1.TabIndex = 1;
            this.buttonCustom1.Text = "buttonText1";
            this.buttonCustom1.UseVisualStyleBackColor = true;
            this.buttonCustom1.Click += new System.EventHandler(this.buttonCustom1_Click);
            // 
            // buttonCustom2
            // 
            this.buttonCustom2.Location = new System.Drawing.Point(25, 132);
            this.buttonCustom2.Name = "buttonCustom2";
            this.buttonCustom2.Size = new System.Drawing.Size(75, 23);
            this.buttonCustom2.TabIndex = 2;
            this.buttonCustom2.Text = "buttonText2";
            this.buttonCustom2.UseVisualStyleBackColor = true;
            this.buttonCustom2.Click += new System.EventHandler(this.buttonCustom2_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(386, 132);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // CustomMB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 176);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonCustom2);
            this.Controls.Add(this.buttonCustom1);
            this.Controls.Add(this.labelText);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "set title";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Button buttonCustom1;
        private System.Windows.Forms.Button buttonCustom2;
        private System.Windows.Forms.Button buttonCancel;
    }
}