using System;
using System.Windows.Forms;

namespace cfw {
    public partial class SimpleYes : Form {
        public SimpleYes() {
            this.InitializeComponent();
        }

        private void SimpleYes_MouseLeave(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button1_MouseLeave(object sender, EventArgs e) {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.button1.MouseLeave -= new System.EventHandler(this.button1_MouseLeave);
            MouseLeave -= new System.EventHandler(this.SimpleYes_MouseLeave);
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.Close();
        }
    }
}
