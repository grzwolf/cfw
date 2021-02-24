using System;
using System.Windows.Forms;

namespace cfw {
    public partial class SimpleInput : Form {
        public SimpleInput() {
            this.InitializeComponent();

            this.checkBoxOption.Visible = false;
            this.textBoxInput.SelectionStart = 0;
            this.textBoxInput.SelectionLength = this.textBoxInput.Text.Length;
            this.textBoxInput.Focus();
            this.textBoxInput.Select(0, this.textBoxInput.Text.Length);
        }

        public void SetOption(string text, bool state) {
            this.checkBoxOption.Visible = true;
            this.checkBoxOption.Checked = state;
            this.checkBoxOption.Text = text;
        }
        public bool GetOption() {
            return this.checkBoxOption.Checked;
        }

        public string Input {
            get {
                return this.textBoxInput.Text;
            }
            set {
                this.textBoxInput.Text = value;
                this.textBoxInput.SelectionStart = 0;
                this.textBoxInput.SelectionLength = this.textBoxInput.Text.Length;
            }
        }
        public string Hint {
            set {
                this.labelHint.Text = value;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e) {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e) {
            System.Windows.Forms.Clipboard.SetText(this.labelHint.SelectedText);
        }

    }
}
