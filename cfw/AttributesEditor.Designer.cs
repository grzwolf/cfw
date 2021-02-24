namespace ChangeModifiedTime
{
    partial class AttributesEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttributesEditor));
            this.dtpCreated = new System.Windows.Forms.DateTimePicker();
            this.dtpModified = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.labelModified = new System.Windows.Forms.Label();
            this.labelAccessed = new System.Windows.Forms.Label();
            this.dtpAccessed = new System.Windows.Forms.DateTimePicker();
            this.bSave = new System.Windows.Forms.Button();
            this.tbStatus = new System.Windows.Forms.TextBox();
            this.cbCreated = new System.Windows.Forms.CheckBox();
            this.cbAccessed = new System.Windows.Forms.CheckBox();
            this.cbModified = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkBoxAllTheSame = new System.Windows.Forms.CheckBox();
            this.checkBoxApplyToSelection = new System.Windows.Forms.CheckBox();
            this.checkBoxRecursion = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // dtpCreated
            // 
            this.dtpCreated.CustomFormat = "HH:mm:ss     dddd MMMM dd, yyyy";
            this.dtpCreated.Enabled = false;
            this.dtpCreated.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpCreated.Location = new System.Drawing.Point(70, 12);
            this.dtpCreated.Name = "dtpCreated";
            this.dtpCreated.Size = new System.Drawing.Size(326, 20);
            this.dtpCreated.TabIndex = 1;
            // 
            // dtpModified
            // 
            this.dtpModified.CustomFormat = "HH:mm:ss     dddd MMMM dd, yyyy";
            this.dtpModified.Enabled = false;
            this.dtpModified.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpModified.Location = new System.Drawing.Point(70, 38);
            this.dtpModified.Name = "dtpModified";
            this.dtpModified.Size = new System.Drawing.Size(326, 20);
            this.dtpModified.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Created:";
            // 
            // labelModified
            // 
            this.labelModified.AutoSize = true;
            this.labelModified.Location = new System.Drawing.Point(7, 42);
            this.labelModified.Name = "labelModified";
            this.labelModified.Size = new System.Drawing.Size(50, 13);
            this.labelModified.TabIndex = 8;
            this.labelModified.Text = "Modified:";
            // 
            // labelAccessed
            // 
            this.labelAccessed.AutoSize = true;
            this.labelAccessed.Location = new System.Drawing.Point(7, 68);
            this.labelAccessed.Name = "labelAccessed";
            this.labelAccessed.Size = new System.Drawing.Size(57, 13);
            this.labelAccessed.TabIndex = 9;
            this.labelAccessed.Text = "Accessed:";
            // 
            // dtpAccessed
            // 
            this.dtpAccessed.CustomFormat = "HH:mm:ss     dddd MMMM dd, yyyy";
            this.dtpAccessed.Enabled = false;
            this.dtpAccessed.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpAccessed.Location = new System.Drawing.Point(70, 64);
            this.dtpAccessed.Name = "dtpAccessed";
            this.dtpAccessed.Size = new System.Drawing.Size(326, 20);
            this.dtpAccessed.TabIndex = 3;
            // 
            // bSave
            // 
            this.bSave.Location = new System.Drawing.Point(105, 199);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(75, 23);
            this.bSave.TabIndex = 5;
            this.bSave.Text = "Ok";
            this.bSave.UseVisualStyleBackColor = true;
            this.bSave.Click += new System.EventHandler(this.bSave_Click);
            // 
            // tbStatus
            // 
            this.tbStatus.Location = new System.Drawing.Point(1, 228);
            this.tbStatus.Name = "tbStatus";
            this.tbStatus.ReadOnly = true;
            this.tbStatus.Size = new System.Drawing.Size(438, 20);
            this.tbStatus.TabIndex = 6;
            // 
            // cbCreated
            // 
            this.cbCreated.AutoSize = true;
            this.cbCreated.Enabled = false;
            this.cbCreated.Location = new System.Drawing.Point(411, 15);
            this.cbCreated.Name = "cbCreated";
            this.cbCreated.Size = new System.Drawing.Size(15, 14);
            this.cbCreated.TabIndex = 10;
            this.cbCreated.UseVisualStyleBackColor = true;
            this.cbCreated.CheckedChanged += new System.EventHandler(this.cbCreated_CheckedChanged);
            // 
            // cbAccessed
            // 
            this.cbAccessed.AutoSize = true;
            this.cbAccessed.Enabled = false;
            this.cbAccessed.Location = new System.Drawing.Point(411, 67);
            this.cbAccessed.Name = "cbAccessed";
            this.cbAccessed.Size = new System.Drawing.Size(15, 14);
            this.cbAccessed.TabIndex = 11;
            this.cbAccessed.UseVisualStyleBackColor = true;
            this.cbAccessed.CheckedChanged += new System.EventHandler(this.cbAccessed_CheckedChanged);
            // 
            // cbModified
            // 
            this.cbModified.AutoSize = true;
            this.cbModified.Enabled = false;
            this.cbModified.Location = new System.Drawing.Point(411, 41);
            this.cbModified.Name = "cbModified";
            this.cbModified.Size = new System.Drawing.Size(15, 14);
            this.cbModified.TabIndex = 12;
            this.cbModified.UseVisualStyleBackColor = true;
            this.cbModified.CheckedChanged += new System.EventHandler(this.cbModified_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(258, 199);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 13;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // checkBoxAllTheSame
            // 
            this.checkBoxAllTheSame.AutoSize = true;
            this.checkBoxAllTheSame.Location = new System.Drawing.Point(12, 116);
            this.checkBoxAllTheSame.Name = "checkBoxAllTheSame";
            this.checkBoxAllTheSame.Size = new System.Drawing.Size(186, 17);
            this.checkBoxAllTheSame.TabIndex = 14;
            this.checkBoxAllTheSame.Text = "\'Modified\' && \'Accessed\' = \'Created\'";
            this.checkBoxAllTheSame.UseVisualStyleBackColor = true;
            this.checkBoxAllTheSame.CheckedChanged += new System.EventHandler(this.checkBoxAllTheSame_CheckedChanged);
            // 
            // checkBoxApplyToSelection
            // 
            this.checkBoxApplyToSelection.AutoSize = true;
            this.checkBoxApplyToSelection.Location = new System.Drawing.Point(12, 139);
            this.checkBoxApplyToSelection.Name = "checkBoxApplyToSelection";
            this.checkBoxApplyToSelection.Size = new System.Drawing.Size(205, 17);
            this.checkBoxApplyToSelection.TabIndex = 15;
            this.checkBoxApplyToSelection.Text = "Apply all 3 file times to whole selection";
            this.checkBoxApplyToSelection.UseVisualStyleBackColor = true;
            // 
            // checkBoxRecursion
            // 
            this.checkBoxRecursion.AutoSize = true;
            this.checkBoxRecursion.Location = new System.Drawing.Point(12, 162);
            this.checkBoxRecursion.Name = "checkBoxRecursion";
            this.checkBoxRecursion.Size = new System.Drawing.Size(223, 17);
            this.checkBoxRecursion.TabIndex = 16;
            this.checkBoxRecursion.Text = "Apply all 3 file times to selected subfolders";
            this.checkBoxRecursion.UseVisualStyleBackColor = true;
            this.checkBoxRecursion.CheckedChanged += new System.EventHandler(this.checkBoxRecursion_CheckedChanged);
            // 
            // AttributesEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 247);
            this.Controls.Add(this.checkBoxRecursion);
            this.Controls.Add(this.checkBoxApplyToSelection);
            this.Controls.Add(this.checkBoxAllTheSame);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.cbModified);
            this.Controls.Add(this.cbAccessed);
            this.Controls.Add(this.cbCreated);
            this.Controls.Add(this.tbStatus);
            this.Controls.Add(this.bSave);
            this.Controls.Add(this.labelAccessed);
            this.Controls.Add(this.dtpAccessed);
            this.Controls.Add(this.labelModified);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dtpModified);
            this.Controls.Add(this.dtpCreated);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttributesEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FileTime Attributes Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker dtpCreated;
        private System.Windows.Forms.DateTimePicker dtpModified;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelModified;
        private System.Windows.Forms.Label labelAccessed;
        private System.Windows.Forms.DateTimePicker dtpAccessed;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.TextBox tbStatus;
        private System.Windows.Forms.CheckBox cbCreated;
        private System.Windows.Forms.CheckBox cbAccessed;
        private System.Windows.Forms.CheckBox cbModified;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkBoxAllTheSame;
        private System.Windows.Forms.CheckBox checkBoxApplyToSelection;
        private System.Windows.Forms.CheckBox checkBoxRecursion;


    }
}

