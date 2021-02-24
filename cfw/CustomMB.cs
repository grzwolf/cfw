using System;
using System.Windows.Forms;

namespace cfw {
    public partial class CustomMB : Form {
        public enum ReturnCustomMB { option1, option2, queryremove };
        public ReturnCustomMB ReturnValue;

        public CustomMB(string caption, string text, string button1text, string button2text) {
            this.InitializeComponent();

            this.Text = caption;
            this.labelText.Text = text;
            this.buttonCustom1.Text = button1text;
            this.buttonCustom2.Text = button2text;
        }

        private void buttonCustom1_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.ReturnValue = ReturnCustomMB.option1;
            this.Close();
        }

        private void buttonCustom2_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.ReturnValue = ReturnCustomMB.option2;
            this.Close();
        }
    }
}
