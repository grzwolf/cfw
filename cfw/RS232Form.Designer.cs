namespace cfw
{
    partial class RS232Form
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
            this.components = new System.ComponentModel.Container();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.comboBoxSerialPorts = new System.Windows.Forms.ComboBox();
            this.comboBoxBaud = new System.Windows.Forms.ComboBox();
            this.comboBoxDataBits = new System.Windows.Forms.ComboBox();
            this.comboBoxParity = new System.Windows.Forms.ComboBox();
            this.comboBoxStop = new System.Windows.Forms.ComboBox();
            this.comboBoxHandShake = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.buttonOpenPort = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxTX = new System.Windows.Forms.TextBox();
            this.buttonTX = new System.Windows.Forms.Button();
            this.richTextBoxRX = new System.Windows.Forms.RichTextBox();
            this.checkBoxTxLock = new System.Windows.Forms.CheckBox();
            this.checkBoxTxFromFile = new System.Windows.Forms.CheckBox();
            this.checkBoxTxRepeat = new System.Windows.Forms.CheckBox();
            this.checkBoxRxCrLf = new System.Windows.Forms.CheckBox();
            this.checkBoxRxToFile = new System.Windows.Forms.CheckBox();
            this.checkBoxRxLogTx = new System.Windows.Forms.CheckBox();
            this.buttonRxClear = new System.Windows.Forms.Button();
            this.checkBoxTimeStamps = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxTxRepeat = new System.Windows.Forms.TextBox();
            this.checkBoxDelay = new System.Windows.Forms.CheckBox();
            this.checkBoxExpectRxCrLf = new System.Windows.Forms.CheckBox();
            this.checkBoxTxTimeStamp = new System.Windows.Forms.CheckBox();
            this.timerCleanUp = new System.Windows.Forms.Timer(this.components);
            this.timerTxRepeat = new System.Windows.Forms.Timer(this.components);
            this.timerTxFile = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // serialPort1
            // 
            this.serialPort1.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler(this.serialPort1_ErrorReceived);
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // comboBoxSerialPorts
            // 
            this.comboBoxSerialPorts.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxSerialPorts.FormattingEnabled = true;
            this.comboBoxSerialPorts.Location = new System.Drawing.Point(114, 36);
            this.comboBoxSerialPorts.Name = "comboBoxSerialPorts";
            this.comboBoxSerialPorts.Size = new System.Drawing.Size(106, 21);
            this.comboBoxSerialPorts.TabIndex = 0;
            this.comboBoxSerialPorts.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxSerialPorts.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // comboBoxBaud
            // 
            this.comboBoxBaud.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxBaud.FormattingEnabled = true;
            this.comboBoxBaud.Items.AddRange(new object[] {
            "300",
            "600",
            "1200",
            "2400",
            "9600",
            "14400",
            "19200",
            "38400",
            "57600",
            "115200"});
            this.comboBoxBaud.Location = new System.Drawing.Point(226, 36);
            this.comboBoxBaud.Name = "comboBoxBaud";
            this.comboBoxBaud.Size = new System.Drawing.Size(106, 21);
            this.comboBoxBaud.TabIndex = 1;
            this.comboBoxBaud.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxBaud.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // comboBoxDataBits
            // 
            this.comboBoxDataBits.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxDataBits.FormattingEnabled = true;
            this.comboBoxDataBits.Items.AddRange(new object[] {
            "7",
            "8"});
            this.comboBoxDataBits.Location = new System.Drawing.Point(338, 36);
            this.comboBoxDataBits.Name = "comboBoxDataBits";
            this.comboBoxDataBits.Size = new System.Drawing.Size(106, 21);
            this.comboBoxDataBits.TabIndex = 2;
            this.comboBoxDataBits.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxDataBits.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // comboBoxParity
            // 
            this.comboBoxParity.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxParity.FormattingEnabled = true;
            this.comboBoxParity.Items.AddRange(new object[] {
            "None",
            "Even",
            "Mark",
            "Odd",
            "Space"});
            this.comboBoxParity.Location = new System.Drawing.Point(562, 36);
            this.comboBoxParity.Name = "comboBoxParity";
            this.comboBoxParity.Size = new System.Drawing.Size(106, 21);
            this.comboBoxParity.TabIndex = 3;
            this.comboBoxParity.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxParity.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // comboBoxStop
            // 
            this.comboBoxStop.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxStop.FormattingEnabled = true;
            this.comboBoxStop.Items.AddRange(new object[] {
            "One",
            "OnePointFive",
            "Two"});
            this.comboBoxStop.Location = new System.Drawing.Point(450, 36);
            this.comboBoxStop.Name = "comboBoxStop";
            this.comboBoxStop.Size = new System.Drawing.Size(106, 21);
            this.comboBoxStop.TabIndex = 4;
            this.comboBoxStop.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxStop.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // comboBoxHandShake
            // 
            this.comboBoxHandShake.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.comboBoxHandShake.FormattingEnabled = true;
            this.comboBoxHandShake.Items.AddRange(new object[] {
            "None",
            "RequestToSend",
            "RequestToSendXOnXOff",
            "XOnXOff"});
            this.comboBoxHandShake.Location = new System.Drawing.Point(674, 36);
            this.comboBoxHandShake.Name = "comboBoxHandShake";
            this.comboBoxHandShake.Size = new System.Drawing.Size(107, 21);
            this.comboBoxHandShake.TabIndex = 5;
            this.comboBoxHandShake.SelectedIndexChanged += new System.EventHandler(this.comboBox_SettingChanged);
            this.comboBoxHandShake.TextChanged += new System.EventHandler(this.comboBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label1.Location = new System.Drawing.Point(114, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label2.Location = new System.Drawing.Point(450, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Stop Bits";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label3.Location = new System.Drawing.Point(562, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Parity";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label4.Location = new System.Drawing.Point(226, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Baud";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label5.Location = new System.Drawing.Point(338, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Data Bits";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label6.Location = new System.Drawing.Point(674, 17);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Hand Shake";
            // 
            // buttonOpenPort
            // 
            this.buttonOpenPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonOpenPort.Location = new System.Drawing.Point(3, 3);
            this.buttonOpenPort.Name = "buttonOpenPort";
            this.tableLayoutPanel1.SetRowSpan(this.buttonOpenPort, 2);
            this.buttonOpenPort.Size = new System.Drawing.Size(105, 54);
            this.buttonOpenPort.TabIndex = 12;
            this.buttonOpenPort.Text = "Open COM";
            this.buttonOpenPort.UseVisualStyleBackColor = true;
            this.buttonOpenPort.Click += new System.EventHandler(this.buttonOpenPort_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 7;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28572F));
            this.tableLayoutPanel1.Controls.Add(this.comboBoxHandShake, 6, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxParity, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.label6, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxStop, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.label5, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxDataBits, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxBaud, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.comboBoxSerialPorts, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.buttonOpenPort, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.textBoxTX, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.buttonTX, 6, 3);
            this.tableLayoutPanel1.Controls.Add(this.richTextBoxRX, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTxLock, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTxFromFile, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTxRepeat, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxRxCrLf, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxRxToFile, 1, 9);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxRxLogTx, 2, 9);
            this.tableLayoutPanel1.Controls.Add(this.buttonRxClear, 6, 9);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTimeStamps, 3, 9);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxExpectRxCrLf, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxTxTimeStamp, 4, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 562);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // textBoxTX
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.textBoxTX, 6);
            this.textBoxTX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxTX.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxTX.Location = new System.Drawing.Point(3, 113);
            this.textBoxTX.Name = "textBoxTX";
            this.textBoxTX.Size = new System.Drawing.Size(665, 21);
            this.textBoxTX.TabIndex = 13;
            this.textBoxTX.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxTX_KeyDown);
            // 
            // buttonTX
            // 
            this.buttonTX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonTX.Location = new System.Drawing.Point(674, 113);
            this.buttonTX.Name = "buttonTX";
            this.buttonTX.Size = new System.Drawing.Size(107, 24);
            this.buttonTX.TabIndex = 14;
            this.buttonTX.Text = "Transmit";
            this.buttonTX.UseVisualStyleBackColor = true;
            this.buttonTX.Click += new System.EventHandler(this.buttonTX_Click);
            // 
            // richTextBoxRX
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.richTextBoxRX, 6);
            this.richTextBoxRX.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxRX.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxRX.Location = new System.Drawing.Point(3, 173);
            this.richTextBoxRX.Name = "richTextBoxRX";
            this.tableLayoutPanel1.SetRowSpan(this.richTextBoxRX, 4);
            this.richTextBoxRX.Size = new System.Drawing.Size(665, 354);
            this.richTextBoxRX.TabIndex = 15;
            this.richTextBoxRX.Text = "";
            // 
            // checkBoxTxLock
            // 
            this.checkBoxTxLock.AutoSize = true;
            this.checkBoxTxLock.Checked = true;
            this.checkBoxTxLock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxTxLock.Location = new System.Drawing.Point(3, 143);
            this.checkBoxTxLock.Name = "checkBoxTxLock";
            this.checkBoxTxLock.Size = new System.Drawing.Size(63, 17);
            this.checkBoxTxLock.TabIndex = 16;
            this.checkBoxTxLock.Text = "TX lock";
            this.checkBoxTxLock.UseVisualStyleBackColor = true;
            // 
            // checkBoxTxFromFile
            // 
            this.checkBoxTxFromFile.AutoSize = true;
            this.checkBoxTxFromFile.Location = new System.Drawing.Point(114, 143);
            this.checkBoxTxFromFile.Name = "checkBoxTxFromFile";
            this.checkBoxTxFromFile.Size = new System.Drawing.Size(56, 17);
            this.checkBoxTxFromFile.TabIndex = 17;
            this.checkBoxTxFromFile.Text = "TX file";
            this.checkBoxTxFromFile.UseVisualStyleBackColor = true;
            this.checkBoxTxFromFile.CheckedChanged += new System.EventHandler(this.checkBoxTxFromFile_CheckedChanged);
            // 
            // checkBoxTxRepeat
            // 
            this.checkBoxTxRepeat.AutoSize = true;
            this.checkBoxTxRepeat.Location = new System.Drawing.Point(226, 143);
            this.checkBoxTxRepeat.Name = "checkBoxTxRepeat";
            this.checkBoxTxRepeat.Size = new System.Drawing.Size(73, 17);
            this.checkBoxTxRepeat.TabIndex = 18;
            this.checkBoxTxRepeat.Text = "TX repeat";
            this.checkBoxTxRepeat.UseVisualStyleBackColor = true;
            this.checkBoxTxRepeat.CheckedChanged += new System.EventHandler(this.checkBoxTxRepeat_CheckedChanged);
            // 
            // checkBoxRxCrLf
            // 
            this.checkBoxRxCrLf.AutoSize = true;
            this.checkBoxRxCrLf.Location = new System.Drawing.Point(3, 533);
            this.checkBoxRxCrLf.Name = "checkBoxRxCrLf";
            this.checkBoxRxCrLf.Size = new System.Drawing.Size(79, 17);
            this.checkBoxRxCrLf.TabIndex = 20;
            this.checkBoxRxCrLf.Text = "RX add crlf";
            this.checkBoxRxCrLf.UseVisualStyleBackColor = true;
            // 
            // checkBoxRxToFile
            // 
            this.checkBoxRxToFile.AutoSize = true;
            this.checkBoxRxToFile.Location = new System.Drawing.Point(226, 533);
            this.checkBoxRxToFile.Name = "checkBoxRxToFile";
            this.checkBoxRxToFile.Size = new System.Drawing.Size(69, 17);
            this.checkBoxRxToFile.TabIndex = 21;
            this.checkBoxRxToFile.Text = "RX to file";
            this.checkBoxRxToFile.UseVisualStyleBackColor = true;
            this.checkBoxRxToFile.CheckedChanged += new System.EventHandler(this.checkBoxRxToFile_CheckedChanged);
            // 
            // checkBoxRxLogTx
            // 
            this.checkBoxRxLogTx.AutoSize = true;
            this.checkBoxRxLogTx.Location = new System.Drawing.Point(338, 533);
            this.checkBoxRxLogTx.Name = "checkBoxRxLogTx";
            this.checkBoxRxLogTx.Size = new System.Drawing.Size(80, 17);
            this.checkBoxRxLogTx.TabIndex = 22;
            this.checkBoxRxLogTx.Text = "RX logs TX";
            this.checkBoxRxLogTx.UseVisualStyleBackColor = true;
            // 
            // buttonRxClear
            // 
            this.buttonRxClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonRxClear.Location = new System.Drawing.Point(674, 533);
            this.buttonRxClear.Name = "buttonRxClear";
            this.buttonRxClear.Size = new System.Drawing.Size(107, 26);
            this.buttonRxClear.TabIndex = 23;
            this.buttonRxClear.Text = "clear";
            this.buttonRxClear.UseVisualStyleBackColor = true;
            this.buttonRxClear.Click += new System.EventHandler(this.buttonRxClear_Click);
            // 
            // checkBoxTimeStamps
            // 
            this.checkBoxTimeStamps.AutoSize = true;
            this.checkBoxTimeStamps.Location = new System.Drawing.Point(450, 533);
            this.checkBoxTimeStamps.Name = "checkBoxTimeStamps";
            this.checkBoxTimeStamps.Size = new System.Drawing.Size(94, 17);
            this.checkBoxTimeStamps.TabIndex = 24;
            this.checkBoxTimeStamps.Text = "RX time stamp";
            this.checkBoxTimeStamps.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.Controls.Add(this.textBoxTxRepeat, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.checkBoxDelay, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(335, 140);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(112, 30);
            this.tableLayoutPanel2.TabIndex = 25;
            // 
            // textBoxTxRepeat
            // 
            this.textBoxTxRepeat.Location = new System.Drawing.Point(70, 2);
            this.textBoxTxRepeat.Margin = new System.Windows.Forms.Padding(3, 2, 3, 3);
            this.textBoxTxRepeat.Name = "textBoxTxRepeat";
            this.textBoxTxRepeat.Size = new System.Drawing.Size(39, 20);
            this.textBoxTxRepeat.TabIndex = 19;
            this.textBoxTxRepeat.Text = "100";
            this.textBoxTxRepeat.TextChanged += new System.EventHandler(this.textBoxTxRepeat_TextChanged);
            // 
            // checkBoxDelay
            // 
            this.checkBoxDelay.AutoSize = true;
            this.checkBoxDelay.Location = new System.Drawing.Point(0, 3);
            this.checkBoxDelay.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.checkBoxDelay.Name = "checkBoxDelay";
            this.checkBoxDelay.Size = new System.Drawing.Size(67, 17);
            this.checkBoxDelay.TabIndex = 20;
            this.checkBoxDelay.Text = "ms delay";
            this.checkBoxDelay.UseVisualStyleBackColor = true;
            // 
            // checkBoxExpectRxCrLf
            // 
            this.checkBoxExpectRxCrLf.AutoSize = true;
            this.checkBoxExpectRxCrLf.Checked = true;
            this.checkBoxExpectRxCrLf.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxExpectRxCrLf.Location = new System.Drawing.Point(114, 533);
            this.checkBoxExpectRxCrLf.Name = "checkBoxExpectRxCrLf";
            this.checkBoxExpectRxCrLf.Size = new System.Drawing.Size(85, 17);
            this.checkBoxExpectRxCrLf.TabIndex = 26;
            this.checkBoxExpectRxCrLf.Text = "RX with \\r\\n";
            this.checkBoxExpectRxCrLf.UseVisualStyleBackColor = true;
            // 
            // checkBoxTxTimeStamp
            // 
            this.checkBoxTxTimeStamp.AutoSize = true;
            this.checkBoxTxTimeStamp.Location = new System.Drawing.Point(450, 143);
            this.checkBoxTxTimeStamp.Name = "checkBoxTxTimeStamp";
            this.checkBoxTxTimeStamp.Size = new System.Drawing.Size(93, 17);
            this.checkBoxTxTimeStamp.TabIndex = 27;
            this.checkBoxTxTimeStamp.Text = "TX time stamp";
            this.checkBoxTxTimeStamp.UseVisualStyleBackColor = true;
            // 
            // timerCleanUp
            // 
            this.timerCleanUp.Interval = 500;
            this.timerCleanUp.Tick += new System.EventHandler(this.timerCleanUp_Tick);
            // 
            // timerTxRepeat
            // 
            this.timerTxRepeat.Tick += new System.EventHandler(this.timerTxRepeat_Tick);
            // 
            // timerTxFile
            // 
            this.timerTxFile.Tick += new System.EventHandler(this.timerTxFile_Tick);
            // 
            // RS232Form
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "RS232Form";
            this.Text = "RS232 Transmitter / Receiver";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RS232Form_FormClosing);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.ComboBox comboBoxSerialPorts;
        private System.Windows.Forms.ComboBox comboBoxBaud;
        private System.Windows.Forms.ComboBox comboBoxDataBits;
        private System.Windows.Forms.ComboBox comboBoxParity;
        private System.Windows.Forms.ComboBox comboBoxStop;
        private System.Windows.Forms.ComboBox comboBoxHandShake;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button buttonOpenPort;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxTX;
        private System.Windows.Forms.Button buttonTX;
        private System.Windows.Forms.RichTextBox richTextBoxRX;
        private System.Windows.Forms.CheckBox checkBoxTxLock;
        private System.Windows.Forms.CheckBox checkBoxTxFromFile;
        private System.Windows.Forms.CheckBox checkBoxTxRepeat;
        private System.Windows.Forms.TextBox textBoxTxRepeat;
        private System.Windows.Forms.CheckBox checkBoxRxCrLf;
        private System.Windows.Forms.CheckBox checkBoxRxToFile;
        private System.Windows.Forms.CheckBox checkBoxRxLogTx;
        private System.Windows.Forms.Button buttonRxClear;
        private System.Windows.Forms.Timer timerCleanUp;
        private System.Windows.Forms.Timer timerTxRepeat;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox checkBoxDelay;
        private System.Windows.Forms.Timer timerTxFile;
        private System.Windows.Forms.CheckBox checkBoxExpectRxCrLf;
        private System.Windows.Forms.CheckBox checkBoxTimeStamps;
        private System.Windows.Forms.CheckBox checkBoxTxTimeStamp;
    }
}