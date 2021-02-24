namespace cfw
{
    partial class NetworkMapping
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
            this.comboBoxDrive = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonSelectNetworkFolder = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.comboBoxNetworkFolder = new System.Windows.Forms.ComboBox();
            this.textBoxUser = new System.Windows.Forms.TextBox();
            this.textBoxPwd = new System.Windows.Forms.TextBox();
            this.checkBoxOption = new System.Windows.Forms.CheckBox();
            this.labelUser = new System.Windows.Forms.Label();
            this.labelPwd = new System.Windows.Forms.Label();
            this.checkBoxRemapAtLogon = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // comboBoxDrive
            // 
            this.comboBoxDrive.DropDownHeight = 500;
            this.comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDrive.FormattingEnabled = true;
            this.comboBoxDrive.IntegralHeight = false;
            this.comboBoxDrive.Location = new System.Drawing.Point(108, 42);
            this.comboBoxDrive.Name = "comboBoxDrive";
            this.comboBoxDrive.Size = new System.Drawing.Size(621, 21);
            this.comboBoxDrive.TabIndex = 0;
            this.comboBoxDrive.SelectedIndexChanged += new System.EventHandler(this.comboBoxDrive_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Drive Letter";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Network Folder";
            // 
            // buttonSelectNetworkFolder
            // 
            this.buttonSelectNetworkFolder.Location = new System.Drawing.Point(698, 95);
            this.buttonSelectNetworkFolder.Name = "buttonSelectNetworkFolder";
            this.buttonSelectNetworkFolder.Size = new System.Drawing.Size(31, 23);
            this.buttonSelectNetworkFolder.TabIndex = 4;
            this.buttonSelectNetworkFolder.Text = "...";
            this.buttonSelectNetworkFolder.UseVisualStyleBackColor = true;
            this.buttonSelectNetworkFolder.Click += new System.EventHandler(this.buttonSelectNetworkFolder_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(751, 42);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 76);
            this.buttonConnect.TabIndex = 5;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(751, 172);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 37);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Exit";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // comboBoxNetworkFolder
            // 
            this.comboBoxNetworkFolder.DropDownHeight = 300;
            this.comboBoxNetworkFolder.FormattingEnabled = true;
            this.comboBoxNetworkFolder.IntegralHeight = false;
            this.comboBoxNetworkFolder.Location = new System.Drawing.Point(108, 97);
            this.comboBoxNetworkFolder.Name = "comboBoxNetworkFolder";
            this.comboBoxNetworkFolder.Size = new System.Drawing.Size(584, 21);
            this.comboBoxNetworkFolder.TabIndex = 8;
            this.comboBoxNetworkFolder.Validated += new System.EventHandler(this.comboBoxNetworkFolder_Validated);
            // 
            // textBoxUser
            // 
            this.textBoxUser.Enabled = false;
            this.textBoxUser.Location = new System.Drawing.Point(108, 153);
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.Size = new System.Drawing.Size(137, 20);
            this.textBoxUser.TabIndex = 9;
            // 
            // textBoxPwd
            // 
            this.textBoxPwd.Enabled = false;
            this.textBoxPwd.Location = new System.Drawing.Point(108, 189);
            this.textBoxPwd.Name = "textBoxPwd";
            this.textBoxPwd.Size = new System.Drawing.Size(137, 20);
            this.textBoxPwd.TabIndex = 10;
            this.textBoxPwd.UseSystemPasswordChar = true;
            // 
            // checkBoxOption
            // 
            this.checkBoxOption.AutoSize = true;
            this.checkBoxOption.Location = new System.Drawing.Point(15, 153);
            this.checkBoxOption.Name = "checkBoxOption";
            this.checkBoxOption.Size = new System.Drawing.Size(62, 17);
            this.checkBoxOption.TabIndex = 11;
            this.checkBoxOption.Text = "Options";
            this.checkBoxOption.UseVisualStyleBackColor = true;
            this.checkBoxOption.CheckedChanged += new System.EventHandler(this.checkBoxOption_CheckedChanged);
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Enabled = false;
            this.labelUser.Location = new System.Drawing.Point(251, 157);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(66, 13);
            this.labelUser.TabIndex = 12;
            this.labelUser.Text = "domain\\user";
            // 
            // labelPwd
            // 
            this.labelPwd.AutoSize = true;
            this.labelPwd.Enabled = false;
            this.labelPwd.Location = new System.Drawing.Point(251, 192);
            this.labelPwd.Name = "labelPwd";
            this.labelPwd.Size = new System.Drawing.Size(52, 13);
            this.labelPwd.TabIndex = 13;
            this.labelPwd.Text = "password";
            // 
            // checkBoxRemapAtLogon
            // 
            this.checkBoxRemapAtLogon.AutoSize = true;
            this.checkBoxRemapAtLogon.Location = new System.Drawing.Point(386, 153);
            this.checkBoxRemapAtLogon.Name = "checkBoxRemapAtLogon";
            this.checkBoxRemapAtLogon.Size = new System.Drawing.Size(176, 17);
            this.checkBoxRemapAtLogon.TabIndex = 14;
            this.checkBoxRemapAtLogon.Text = "Restore Drive Mapping at logon";
            this.checkBoxRemapAtLogon.UseVisualStyleBackColor = true;
            // 
            // NetworkMapping
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 231);
            this.Controls.Add(this.checkBoxRemapAtLogon);
            this.Controls.Add(this.labelPwd);
            this.Controls.Add(this.labelUser);
            this.Controls.Add(this.checkBoxOption);
            this.Controls.Add(this.textBoxPwd);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.comboBoxNetworkFolder);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.buttonSelectNetworkFolder);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxDrive);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NetworkMapping";
            this.Text = "Network Drive Mapping";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NetworkMapping_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxDrive;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSelectNetworkFolder;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ComboBox comboBoxNetworkFolder;
        private System.Windows.Forms.TextBox textBoxUser;
        private System.Windows.Forms.TextBox textBoxPwd;
        private System.Windows.Forms.CheckBox checkBoxOption;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.Label labelPwd;
        private System.Windows.Forms.CheckBox checkBoxRemapAtLogon;
    }
}