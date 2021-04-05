using System;
using System.Windows.Forms;

namespace cfw {
    public partial class crmdDialog : Form {
        public string ReturnDestination { get; set; }

        readonly string m_sOriginalDestination;

        public crmdDialog(string title, string description, string destination, bool clipboard) {
            this.InitializeComponent();
            this.Text = title;
            this.textBoxDescription.Text = description;
            this.textBoxDescription.Select(0, 0);
            this.textBoxDestination.Text = destination;
            this.textBoxDestination.Select(destination.Length, 0);
            this.m_sOriginalDestination = destination;
            if ( clipboard ) {
                this.checkBox1.Show();
            } else {
                this.checkBox1.Hide();
            }
            if ( this.Text == "Delete" ) {
                this.checkBox1.Text = "Delete To Trash";
                this.checkBox1.Checked = true;
                this.textBoxDestination.Hide();
                this.buttonBrowse.Hide();
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e) {
            // self made file folder select dialog
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select Folder";
            sff.DefaultPath = this.textBoxDestination.Text;
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                this.textBoxDestination.Text = sff.ReturnPath;
            }
            sff.Dispose();
        }

        private void buttonOk_Click(object sender, EventArgs e) {
            this.ReturnDestination = this.textBoxDestination.Text;
        }

        private void buttonCancel_Click(object sender, EventArgs e) {
            this.ReturnDestination = this.m_sOriginalDestination;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if ( this.checkBox1.Checked ) {
                if ( this.Text == "Delete" ) {
                    this.textBoxDestination.Text = "::trash::";
                } else {
                    this.textBoxDestination.Text = "::clipboard::";
                }
            } else {
                this.textBoxDestination.Text = this.m_sOriginalDestination;
            }
        }

        private void textBoxDestination_KeyPress(object sender, KeyPressEventArgs e) {
            if ( e.KeyChar == Convert.ToChar(Keys.Enter) ) {
                this.ReturnDestination = this.textBoxDestination.Text;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }

        // 20160717
        private void crmdDialog_KeyDown(object sender, KeyEventArgs e) {
            if ( e.KeyCode == Keys.Escape ) {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
            }
            if ( e.KeyCode == Keys.Enter ) {
                this.ReturnDestination = this.textBoxDestination.Text;
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                this.Close();
            }
        }
    }
}
