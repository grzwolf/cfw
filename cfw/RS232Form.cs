using System;
using System.Drawing;
using System.Globalization;                         // CultureInfo.InvariantCulture
using System.IO;                                    // Path
using System.IO.Ports;                              // RS232  
using System.Linq;
using System.Threading;                             // Sleep   
using System.Windows.Forms;

namespace cfw {
    public partial class RS232Form : Form {
        StreamWriter m_swLog = null;
        string[] m_TxFileData = null;
        int m_TxFileIndex = 0;
        string m_sGlobalRX = "";

        public RS232Form() {
            this.InitializeComponent();

            this.richTextBoxRX.Text = "";

            this.GetSerialPortsToComboBox(this.comboBoxSerialPorts);
            if ( this.comboBoxSerialPorts.Items.Count == 0 ) {
                return;
            }

            // setup defaults: TBD get last setting from INI
            this.comboBoxSerialPorts.SelectedIndex = 0;
            this.comboBoxBaud.SelectedItem = "9600";
            this.comboBoxDataBits.SelectedItem = "8";
            this.comboBoxStop.SelectedItem = "One";
            this.comboBoxParity.SelectedItem = "None";
            this.comboBoxHandShake.SelectedItem = "None";
        }

        // get serial port names and fill them into a combo box
        void GetSerialPortsToComboBox(ComboBox cbx) {
            string[] ArrayComPortsNames = null;
            ArrayComPortsNames = SerialPort.GetPortNames();
            if ( ArrayComPortsNames != null ) {
                Array.Sort(ArrayComPortsNames);
                foreach ( string sCom in ArrayComPortsNames ) {
                    cbx.Items.Add(sCom);
                }
            }
        }

        void CloseComPort() {
            if ( this.serialPort1.IsOpen ) {
                this.serialPort1.Close();
                this.buttonOpenPort.BackColor = SystemColors.Control;
                this.buttonOpenPort.Text = "Open COM";
                if ( this.checkBoxRxToFile.Checked ) {
                    this.m_swLog.WriteLine("--------------------------------------------------------------------------");
                    this.m_swLog.WriteLine("COM closed " + DateTime.Now.ToString());
                }
                Thread.Sleep(100);
            }
        }
        private void buttonOpenPort_Click(object sender, EventArgs e) {
            // global memory for RX
            this.m_sGlobalRX = "";

            // toggle back to 'Open COM' in case COM is already open
            if ( this.serialPort1.IsOpen ) {
                this.CloseComPort();
                return;
            }

            // apply COM settings
            string errormessage = "";
            try {
                this.serialPort1.PortName = this.comboBoxSerialPorts.SelectedItem.ToString();
            } catch ( Exception ) {
                this.comboBoxSerialPorts.BackColor = Color.Red;
                errormessage += "Port name is not acceptabel.\r\n";
            }
            try {
                this.serialPort1.BaudRate = Int32.Parse(this.comboBoxBaud.SelectedItem.ToString());
            } catch ( Exception ) {
                this.comboBoxBaud.BackColor = Color.Red;
                errormessage += "Baud Rate is not acceptabel.\r\n";
            }
            try {
                this.serialPort1.DataBits = Int32.Parse(this.comboBoxDataBits.SelectedItem.ToString());
            } catch ( Exception ) {
                this.comboBoxDataBits.BackColor = Color.Red;
                errormessage += "Data Bits setting is not acceptabel.\r\n";
            }
            try {
                this.serialPort1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), this.comboBoxStop.SelectedItem.ToString());
            } catch ( Exception ) {
                this.comboBoxStop.BackColor = Color.Red;
                errormessage += "Stop Bits setting is not acceptabel.\r\n";
            }
            try {
                this.serialPort1.Parity = (Parity)Enum.Parse(typeof(Parity), this.comboBoxParity.SelectedItem.ToString());
            } catch ( Exception ) {
                this.comboBoxParity.BackColor = Color.Red;
                errormessage += "Parity setting is not acceptabel.\r\n";
            }
            try {
                this.serialPort1.Handshake = (Handshake)Enum.Parse(typeof(Handshake), this.comboBoxHandShake.SelectedItem.ToString());
            } catch ( Exception ) {
                this.comboBoxHandShake.BackColor = Color.Red;
                errormessage += "Handshake setting is not acceptabel.\r\n";
            }
            if ( errormessage.Length > 0 ) {
                this.buttonOpenPort.BackColor = Color.Red;
                MessageBox.Show(errormessage, "Error");
                this.timerCleanUp.Start();
                return;
            }

            // open COM
            try {
                this.serialPort1.Open();
                Thread.Sleep(100);
                if ( this.serialPort1.IsOpen ) {
                    this.buttonOpenPort.BackColor = Color.Green;
                    this.buttonOpenPort.Text = "Close COM";
                }
            } catch ( Exception ex ) {
                this.buttonOpenPort.BackColor = Color.Red;
                MessageBox.Show("At least one of the setup parameters is misbehaving.\r\n" + ex.ToString(), "Error");
                this.timerCleanUp.Start();
            }

            // logfile
            if ( this.checkBoxRxToFile.Checked ) {
                if ( this.serialPort1.IsOpen ) {
                    string portsetting = "";
                    portsetting += this.serialPort1.PortName + "/";
                    portsetting += this.serialPort1.BaudRate.ToString() + "/";
                    portsetting += this.serialPort1.DataBits.ToString() + "/";
                    portsetting += this.serialPort1.StopBits.ToString() + "/";
                    portsetting += this.serialPort1.Parity.ToString() + "/";
                    portsetting += this.serialPort1.Handshake.ToString() + " ";
                    this.m_swLog.WriteLine("COM opened " + portsetting + DateTime.Now.ToString());
                    this.m_swLog.WriteLine("---------------------------------------------------------------------------");
                }
            }
        }
        private void comboBox_TextChanged(object sender, EventArgs e) {
            ComboBox cbx = (ComboBox)sender;
            cbx.BackColor = SystemColors.Window;
            string txt = cbx.Text;
            int index = cbx.FindString(txt);
            if ( (index < cbx.Items.Count) && (index >= 0) ) {
                cbx.SelectedIndex = index;
            } else {
                cbx.BackColor = Color.Red;
            }
        }
        private void timerCleanUp_Tick(object sender, EventArgs e) {
            this.timerCleanUp.Stop();
            this.buttonOpenPort.BackColor = SystemColors.Control;
        }
        private void RS232Form_FormClosing(object sender, FormClosingEventArgs e) {
            this.timerTxRepeat.Stop();
            this.CloseComPort();
            this.CloseLogfile();
        }
        private void comboBox_SettingChanged(object sender, EventArgs e) {
            this.CloseComPort();
        }

        // COM data receiver handler
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            if ( e.EventType == SerialData.Chars ) {
                if ( this.serialPort1.BytesToRead > 0 ) {
                    // read rx data
                    string rxData = this.serialPort1.ReadExisting();
                    // execute data handling in a separate thread
                    new System.Threading.Tasks.Task(() => this.handleRX(rxData)).Start();
                }
            }
        }
        void handleRX(string rxData) {
            // read rx data
            string rxString = rxData;

            // if m_sGlobalRX is supposed to contain "\r\n", but it doesn't (yet), we simply return doing nothing
            if ( this.checkBoxExpectRxCrLf.Checked ) {
                // in case we want to wait for "\r\n" in RX: initially memorize whatever was received so far
                this.m_sGlobalRX += rxString;
                if ( !this.m_sGlobalRX.Contains("\r\n") ) {
                    return;
                } else {
                    rxString = this.m_sGlobalRX;
                }
            }

            // if we reach this line, RX contains "\r\n" 
            if ( this.m_sGlobalRX.Contains("\r\n") ) {
                // we keep whatever came in after the "\r\n" in m_sGlobalRX, because this 'leftover' belongs to the next message
                this.m_sGlobalRX = this.m_sGlobalRX.Substring(this.m_sGlobalRX.IndexOf("\r\n") + 2);
            } else {
                this.m_sGlobalRX = "";
            }

            // add timestamp
            string timestamp = "";
            if ( this.checkBoxTimeStamps.Checked ) {
                timestamp = DateTime.Now.ToString("HH:mm:ss.fff ", CultureInfo.InvariantCulture);
            }

            // add auto cr/lf 
            if ( this.checkBoxRxCrLf.Checked ) {
                rxString += "\u2028";
            }

            // we need to invoke, because we are in a not UI thread
            this.Invoke(new Action(() => {
                this.richTextBoxRX.AppendText(timestamp + rxString);
                this.richTextBoxRX.Select(this.richTextBoxRX.Text.Length, 0);
                this.richTextBoxRX.ScrollToCaret();
            }));

            // logfile
            if ( this.m_swLog != null ) {
                this.m_swLog.Write(rxString);
            }

        }

        // COM error reception
        private void serialPort1_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
            // very elegant: w/o this Invoke(..) thing, events raised from a separate thread are forbidden to access UI-Thread elements and exceptions are thrown. This is an ALTERNATIVE: Invoke(new Action(() => { richTextBoxRX.ScrollToCaret(); }));
            if ( this.InvokeRequired ) {
                try {
                    this.Invoke(new System.IO.Ports.SerialErrorReceivedEventHandler(this.serialPort1_ErrorReceived), sender, e);
                } catch ( Exception ) {; }
                return;
            }

            this.richTextBoxRX.Text += "=============================ERROR=================================";
            this.richTextBoxRX.Text += e.ToString();
            this.richTextBoxRX.Text += "===================================================================";
            if ( this.m_swLog != null ) {
                this.m_swLog.WriteLine("=============================ERROR=================================");
                this.m_swLog.WriteLine(e.ToString());
                this.m_swLog.WriteLine("===================================================================");
            }
            this.richTextBoxRX.Select(this.richTextBoxRX.Text.Length, 0);
            this.richTextBoxRX.ScrollToCaret();
        }

        // COM transmit data
        private void buttonTX_Click(object sender, EventArgs e) {
            if ( this.serialPort1.IsOpen ) {
                // send with time stamp
                string timestamp = "";
                if ( this.checkBoxTxTimeStamp.Checked ) {
                    timestamp = DateTime.Now.ToString("HH:mm:ss.fff ", CultureInfo.InvariantCulture);
                }
                // send
                this.serialPort1.Write(timestamp + this.textBoxTX.Text + "\r\n");
                // log with time stamp
                if ( this.checkBoxRxLogTx.Checked ) {
                    this.richTextBoxRX.Text += "<TX>" + this.textBoxTX.Text + "<TX>\r\n";
                    if ( this.m_swLog != null ) {
                        this.m_swLog.WriteLine("<TX>" + this.textBoxTX.Text + "<TX>");
                    }
                    this.richTextBoxRX.Select(this.richTextBoxRX.Text.Length, 0);
                    this.richTextBoxRX.ScrollToCaret();
                }
                if ( !this.checkBoxTxLock.Checked ) {
                    this.textBoxTX.Text = "";
                }
            }
        }
        private void textBoxTX_KeyDown(object sender, KeyEventArgs e) {
            // enter key shall act like push 'transmit' button
            if ( e.KeyCode == Keys.Enter ) {
                this.buttonTX_Click(null, null);
            }
        }

        // empty RX window
        private void buttonRxClear_Click(object sender, EventArgs e) {
            this.richTextBoxRX.Text = "";
        }

        // Logfile open/close handling
        private void checkBoxRxToFile_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxRxToFile.Checked ) {
                this.OpenLogfile();
            } else {
                this.CloseLogfile();
            }
        }
        void OpenLogfile() {
            string logfile = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture) + ".txt";
            this.m_swLog = File.CreateText(logfile);
            this.m_swLog.WriteLine("COM logfile started " + DateTime.Now.ToString());
            this.m_swLog.WriteLine("===========================================================================");
            string portsetting = "";
            if ( this.serialPort1.IsOpen ) {
                portsetting += "COM is open ";
                portsetting += this.serialPort1.PortName + "/";
                portsetting += this.serialPort1.BaudRate.ToString() + "/";
                portsetting += this.serialPort1.DataBits.ToString() + "/";
                portsetting += this.serialPort1.StopBits.ToString() + "/";
                portsetting += this.serialPort1.Parity.ToString() + "/";
                portsetting += this.serialPort1.Handshake.ToString() + "\r\n";
                this.m_swLog.WriteLine(portsetting);
                this.m_swLog.WriteLine("---------------------------------------------------------------------------");
            }
        }
        void CloseLogfile() {
            if ( this.m_swLog != null ) {
                this.m_swLog.WriteLine("---------------------------------------------------------------------------");
                this.m_swLog.WriteLine("COM Logfile stopped  " + DateTime.Now.ToString());
                string filename = ((FileStream)(this.m_swLog.BaseStream)).Name;
                this.m_swLog.Close();
                this.m_swLog = null;
                GrzTools.AutoMessageBox.Show(filename, "COM Logfile closed", 3000);
            }
        }

        // automatically correct any wrong input into textbox
        private void textBoxTxRepeat_TextChanged(object sender, EventArgs e) {
            int value = 1;
            if ( !int.TryParse(this.textBoxTxRepeat.Text, out value) ) {
                this.textBoxTxRepeat.BackColor = Color.Red;
                Application.DoEvents();
                Thread.Sleep(500);
                Application.DoEvents();
                this.textBoxTxRepeat.BackColor = SystemColors.Window;
                this.textBoxTxRepeat.Text = "100";
            } else {
                // if timer is already running, then update its time interval
                if ( this.checkBoxTxRepeat.Checked ) {
                    this.timerTxRepeat.Interval = value;
                }
            }
        }

        // repeat single line TX via timer 
        private void checkBoxTxRepeat_CheckedChanged(object sender, EventArgs e) {
            CheckBox cb = (CheckBox)sender;
            if ( cb.Checked ) {
                int value = 1;
                if ( this.checkBoxDelay.Checked ) {
                    int.TryParse(this.textBoxTxRepeat.Text, out value);
                }
                this.timerTxRepeat.Interval = value;
                this.timerTxRepeat.Start();
            } else {
                this.timerTxRepeat.Stop();
            }
        }
        private void timerTxRepeat_Tick(object sender, EventArgs e) {
            this.buttonTX_Click(null, null);
            Application.DoEvents();
        }

        // transmit a whole file via timer
        private void checkBoxTxFromFile_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxTxFromFile.Checked ) {
                // self made dialog
                SelectFolderOrFile sff = new SelectFolderOrFile();
                sff.Text = "Select File";
                sff.DefaultPath = Path.GetDirectoryName(Application.ExecutablePath);
                DialogResult dlr = sff.ShowDialog(this);
                if ( dlr != System.Windows.Forms.DialogResult.OK ) {
                    return;
                }
                this.m_TxFileData = File.ReadLines(sff.ReturnFile).ToArray();
                sff.Dispose();

                int value = 1;
                if ( this.checkBoxDelay.Checked ) {
                    int.TryParse(this.textBoxTxRepeat.Text, out value);
                }
                this.m_TxFileIndex = 0;
                this.timerTxFile.Interval = value;
                this.timerTxFile.Start();
            } else {
                this.timerTxFile.Stop();
                this.m_TxFileData = null;
            }
        }
        private void timerTxFile_Tick(object sender, EventArgs e) {
            Application.DoEvents();
            if ( this.m_TxFileData == null ) {
                return;
            }
            if ( this.m_TxFileIndex < this.m_TxFileData.Length ) {
                this.textBoxTX.Text = this.m_TxFileData[this.m_TxFileIndex++];
                this.buttonTX_Click(null, null);
            } else {
                this.checkBoxTxFromFile.Checked = false;
                this.timerTxFile.Stop();
                this.m_TxFileIndex = 0;
                this.m_TxFileData = null;
            }
            Application.DoEvents();
        }
    }


}
