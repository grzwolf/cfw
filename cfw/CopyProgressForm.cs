using System;
using System.Windows.Forms;

namespace cfw {
    public partial class CopyProgressForm : Form {
        public CopyProgressForm() {
            this.InitializeComponent();
        }

        private void OnCancel(object sender, EventArgs e) {
            this.Close();
        }
    }
}
