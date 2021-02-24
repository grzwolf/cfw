using System;
using System.Windows.Forms;

namespace cfw {
    public partial class InputBox : Form {
        string m_Category = "*.*";

        public InputBox() {
            this.InitializeComponent();

            this.ReadINI();
            this.comboBoxFilter.Focus();
        }

        private void OnOk(object sender, EventArgs e) {
            this.m_Category = this.comboBoxFilter.Text;
            if ( this.m_Category == "" ) {
                MessageBox.Show("An empty filter selection was detected, it is reset to *.*", "Note");
                this.m_Category = "*.*";
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void OnCancel(object sender, EventArgs e) {
            this.m_Category = "";

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string Category {
            get {
                return (this.m_Category);
            }
            set {
                this.m_Category = value;
                this.comboBoxSetFilter(this.m_Category);
            }
        }

        private void textBoxNewCategory_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Enter ) {
                string str = this.comboBoxFilter.Text;
                this.comboBoxSetFilter(str);
                this.OnOk(null, null);
            }
            if ( e.KeyCode == Keys.Escape ) {
                this.m_Category = "";
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        // reset filter & return
        private void buttonReset_Click(object sender, EventArgs e) {
            string str = "*.*";
            this.comboBoxSetFilter(str);
            this.OnOk(null, null);
        }

        // indicator that combobox had lost focus, ie. editing is over either thru ET or combo lost focus 
        private void comboBoxFilter_Validated(object sender, EventArgs e) {
            // insert the just edited buzzword to the combobox list
            string str = this.comboBoxFilter.Text;
            this.comboBoxSetFilter(str);
        }

        // set filter
        private void comboBoxSetFilter(string str) {
            int index = this.comboBoxFilter.FindStringExact(str);
            // if modified filter is already member of the list, we delete it first, then insert it at the top
            if ( (index < this.comboBoxFilter.Items.Count) && (index >= 0) ) {
                this.comboBoxFilter.Items.RemoveAt(index);
            }
            // insert filter at the top of the list
            this.comboBoxFilter.Items.Insert(0, str);
            this.comboBoxFilter.SelectedIndex = 0;
            //
            this.SaveINI();
        }

        // delete filter
        private void comboBoxDeleteFilter(string str) {
            int index = this.comboBoxFilter.FindStringExact(str);
            // if modified filter is already member of the list, we delete it first, then insert it at the top
            if ( (index < this.comboBoxFilter.Items.Count) && (index >= 0) ) {
                this.comboBoxFilter.Items.RemoveAt(index);
            }
            this.comboBoxFilter.SelectedIndex = 0;
            //
            this.SaveINI();
        }

        // INI: write 
        void SaveINI() {
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 20; i++ ) {
                if ( i < this.comboBoxFilter.Items.Count ) {
                    ini.IniWriteValue("cfw", "filter" + i.ToString(), this.comboBoxFilter.GetItemText(this.comboBoxFilter.Items[i]));
                } else {
                    ini.IniWriteValue("cfw", "filter" + i.ToString(), null);
                }
            }
        }
        // read from INI on load form
        private void ReadINI() {
            // INI: read
            GrzTools.IniFile ini = new GrzTools.IniFile(System.Windows.Forms.Application.ExecutablePath + ".ini");
            for ( int i = 0; i < 20; i++ ) {
                string tmp = ini.IniReadValue("cfw", "filter" + i.ToString(), "");
                if ( tmp.Length > 0 ) {
                    this.comboBoxFilter.Items.Add(tmp);
                }
            }
        }

        private void deleteFilterToolStripMenuItem_Click(object sender, EventArgs e) {
            string str = this.comboBoxFilter.Text;
            this.comboBoxDeleteFilter(str);
        }

        private void comboBoxFilter_SelectionChangeCommitted(object sender, EventArgs e) {
            string str = this.comboBoxFilter.SelectedItem.ToString();
            this.comboBoxSetFilter(str);
            //OnOk(null, null);
        }

    }
}