using System;
using System.Management;              // WMI stuff
using System.Windows.Forms;

namespace cfw {
    public partial class IpEtc : Form {
        public string ReturnValueIpString;

        public IpEtc() {
            this.InitializeComponent();

            this.ListIP();
        }

        public void ListIP() {
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
            foreach ( ManagementObject objMO in objMOC ) {

                if ( !(bool)objMO["ipEnabled"] ) {
                    continue;
                }

                this.Text = "My IPs - " + (string)objMO["DNSHostName"];
                string caption = (string)objMO["Caption"];
                string srvname = (string)objMO["ServiceName"];
                string mac = (string)objMO["MACAddress"];
                string[] ipaddresses = (string[])objMO["IPAddress"];
                string[] subnets = (string[])objMO["IPSubnet"];
                string[] gateways = (string[])objMO["DefaultIPGateway"];

                string[] strarr = new string[6] { "", "", "", "", "", "" };
                try {
                    strarr[0] = srvname;
                } catch ( Exception ) {; }
                try {
                    strarr[1] = ipaddresses[0];
                } catch ( Exception ) {; }
                try {
                    strarr[2] = subnets[0];
                } catch ( Exception ) {; }
                try {
                    strarr[3] = gateways[0];
                } catch ( Exception ) {; }
                try {
                    strarr[4] = mac;
                } catch ( Exception ) {; }
                try {
                    strarr[5] = caption;
                } catch ( Exception ) {; }

                this.listView1.Items.Add(new ListViewItem(strarr));
            }

            this.listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void copyIPAddressToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            string toCopy = this.listView1.SelectedItems[0].SubItems[1].Text;
            Clipboard.Clear();
            Clipboard.SetText(toCopy);
        }

        private void copySubnetToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            string toCopy = this.listView1.SelectedItems[0].SubItems[2].Text;
            Clipboard.Clear();
            Clipboard.SetText(toCopy);
        }

        private void copyGatewayToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            string toCopy = this.listView1.SelectedItems[0].SubItems[3].Text;
            Clipboard.Clear();
            Clipboard.SetText(toCopy);
        }

        private void copyMACAddressToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            string toCopy = this.listView1.SelectedItems[0].SubItems[4].Text;
            Clipboard.Clear();
            Clipboard.SetText(toCopy);
        }

        private void copyRowToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.listView1.SelectedItems.Count == 0 ) {
                return;
            }
            string toCopy = "";
            for ( int i = 0; i < 6; i++ ) {
                toCopy += this.listView1.SelectedItems[0].SubItems[i].Text + "  ";
            }
            Clipboard.Clear();
            Clipboard.SetText(toCopy);
        }

        private void button1_Click(object sender, EventArgs e) {
            this.ReturnValueIpString = "";
            if ( this.listView1.SelectedItems.Count > 0 ) {
                this.ReturnValueIpString = this.listView1.SelectedItems[0].SubItems[1].Text;
            }
            this.Close();
        }

    }
}
