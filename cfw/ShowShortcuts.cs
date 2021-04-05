using System;
using System.Windows.Forms;

namespace cfw {
    public partial class ShowShortcuts : Form {
        readonly Form m_parent = null;

        public ShowShortcuts(Form parent) {
            this.InitializeComponent();
            this.m_parent = parent;
        }

        private void close_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void ShowShortcuts_KeyDown(object sender, KeyEventArgs e) {
            if ( (e.Alt) & (e.KeyCode == Keys.S) ) {
                this.Hide();
                this.m_parent.BringToFront();
                this.m_parent.Focus();
                e.Handled = true;
            }
        }

        private void ShowShortcuts_KeyUp(object sender, KeyEventArgs e) {
            if ( (e.Alt) & (e.KeyCode == Keys.S) ) {
                e.Handled = true;
            }
        }
    }
}
