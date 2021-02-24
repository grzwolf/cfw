using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;                 // CultureInfo.InvariantCulture
using System.Threading;                     // Sleep   
using System.Windows.Forms;

namespace cfw {
    public partial class PortIO : Form {
        readonly List<CheckBox> m_cbx = new List<CheckBox>();

        public PortIO() {
            this.InitializeComponent();

            // get the list of 16x CheckBox, representing the 16 Bits of the IO port content
            this.m_cbx.Add(this.checkBox0);
            this.m_cbx.Add(this.checkBox1);
            this.m_cbx.Add(this.checkBox2);
            this.m_cbx.Add(this.checkBox3);
            this.m_cbx.Add(this.checkBox4);
            this.m_cbx.Add(this.checkBox5);
            this.m_cbx.Add(this.checkBox6);
            this.m_cbx.Add(this.checkBox7);
            this.m_cbx.Add(this.checkBox8);
            this.m_cbx.Add(this.checkBox9);
            this.m_cbx.Add(this.checkBox10);
            this.m_cbx.Add(this.checkBox11);
            this.m_cbx.Add(this.checkBox12);
            this.m_cbx.Add(this.checkBox13);
            this.m_cbx.Add(this.checkBox14);
            this.m_cbx.Add(this.checkBox15);
        }

        // make sure IO port address fits to format 0x1f 
        private void textBoxPortAddress_TextChanged(object sender, EventArgs e) {
            uint value;
            string hex = this.textBoxPortAddress.Text;
            if ( !hex.StartsWith("0x", StringComparison.CurrentCultureIgnoreCase) ) {
                this.textBoxPortAddress.BackColor = Color.Red;
                Application.DoEvents();
                Thread.Sleep(500);
                this.textBoxPortAddress.BackColor = SystemColors.Window;
                Application.DoEvents();
                this.textBoxPortAddress.Text = "0x40";
                return;
            }
            hex = hex.Substring(2);
            if ( !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value) ) {
                this.textBoxPortAddress.BackColor = Color.Red;
                Application.DoEvents();
                Thread.Sleep(500);
                this.textBoxPortAddress.BackColor = SystemColors.Window;
                Application.DoEvents();
                this.textBoxPortAddress.Text = "0x40";
            }
        }

        // read IO port via button
        private void buttonReadPort_Click(object sender, EventArgs e) {
            // get port address and convert it to number 
            string hex = this.textBoxPortAddress.Text;
            hex = hex.Substring(2);
            uint value;
            uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out value);

            // open IO port
            PortAccess.PortAccess pa = null;
            pa = new PortAccess.PortAccess((short)value);
            if ( pa == null ) {
                this.checkBoxAutoRead.Checked = false;
                this.timerAutoRead.Stop();
                MessageBox.Show("Port could not be opened, perhaps you are missing 'inpoutXXX.dll'", "Error");
                return;
            }

            // read from IO port
            int outp = pa.Read();

            // show port content as hex
            this.textBoxPortContent.Text = "0x" + Convert.ToString(outp, 16);

            // show port content as bits 
            this.HexToCheckBoxes((short)outp);
        }
        void HexToCheckBoxes(short setBits) {
            short currentBit = 0x0001;
            for ( int index = 0; index < this.m_cbx.Count; index++ ) {
                this.m_cbx[index].Checked = (setBits & currentBit) == currentBit;
                currentBit <<= 1;
            }
        }

        // read IO port via timer
        private void checkBoxAutoRead_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBoxAutoRead.Checked ) {
                this.timerAutoRead.Start();
            } else {
                this.timerAutoRead.Stop();
            }
        }
        private void timerAutoRead_Tick(object sender, EventArgs e) {
            this.timerAutoRead.Stop();
            this.buttonReadPort_Click(null, null);
            this.timerAutoRead.Start();
        }

        // shut down port reading before closing form
        private void PortIO_FormClosing(object sender, FormClosingEventArgs e) {
            this.timerAutoRead.Stop();
            Thread.Sleep(500);
        }


    }
}
