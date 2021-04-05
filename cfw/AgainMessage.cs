using System;
using System.Windows.Forms;

namespace cfw {
    public partial class AgainMessage : Form {
        public AgainMessage() {
            this.InitializeComponent();
        }

        private void OnButtonYes(object sender, EventArgs e) {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
        }

        private void OnButtonNo(object sender, EventArgs e) {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.No;
        }

        private void ButtonKeepFile_Click(object sender, EventArgs e) {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Ignore;
        }

        private void buttonBreak_Click(object sender, EventArgs e) {
            this.Close();
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
        }
    }
}
