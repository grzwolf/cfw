using System;
using System.IO;
using System.Windows.Forms;

namespace cfw {
    public partial class AssignFKeys : Form {
        public string f1text = "";
        public string f2text = "";
        public string f11text = "";
        public string f12text = "";
        public string f1prog = "";
        public string f2prog = "";
        public string f11prog = "";
        public string f12prog = "";
        public bool f1admin = false;
        public bool f2admin = false;
        public bool f11admin = false;
        public bool f12admin = false;


        public AssignFKeys(string f1text, string f2text, string f11text, string f12text, string f1prog, string f2prog, string f11prog, string f12prog, bool f1admin, bool f2admin, bool f11admin, bool f12admin) {
            this.InitializeComponent();

            this.textBoxF1.Text = f1text;
            this.textBoxF2.Text = f2text;
            this.textBoxF11.Text = f11text;
            this.textBoxF12.Text = f12text;
            this.textBoxF1Prog.Text = f1prog;
            this.textBoxF2Prog.Text = f2prog;
            this.textBoxF11Prog.Text = f11prog;
            this.textBoxF12Prog.Text = f12prog;
            this.checkBoxAdminF1.Checked = f1admin;
            this.checkBoxAdminF2.Checked = f2admin;
            this.checkBoxAdminF11.Checked = f11admin;
            this.checkBoxAdminF12.Checked = f12admin;
        }

        private void buttonF1Prog_Click(object sender, EventArgs e) {
            string ret = this.OpenFile(this.textBoxF1Prog.Text);
            if ( ret.Length > 0 ) {
                this.textBoxF1Prog.Text = ret;
            }
        }
        private void buttonF2Prog_Click(object sender, EventArgs e) {
            string ret = this.OpenFile(this.textBoxF2Prog.Text);
            if ( ret.Length > 0 ) {
                this.textBoxF2Prog.Text = ret;
            }
        }
        private void buttonF11Prog_Click(object sender, EventArgs e) {
            string ret = this.OpenFile(this.textBoxF11Prog.Text);
            if ( ret.Length > 0 ) {
                this.textBoxF11Prog.Text = ret;
            }
        }
        private void buttonF12Prog_Click(object sender, EventArgs e) {
            string ret = this.OpenFile(this.textBoxF12Prog.Text);
            if ( ret.Length > 0 ) {
                this.textBoxF12Prog.Text = ret;
            }
        }
        string OpenFile(string defaultfile) {
            string ret = "";

            // self made dialog
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select File";
            if ( !File.Exists(defaultfile) ) {
                defaultfile = Application.StartupPath;
            }
            sff.DefaultPath = defaultfile;
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                ret = sff.ReturnFile;
            }
            sff.Dispose();

            return ret;
        }

        private void buttonOk_Click(object sender, EventArgs e) {
            this.f1text = this.textBoxF1.Text;
            this.f2text = this.textBoxF2.Text;
            this.f11text = this.textBoxF11.Text;
            this.f12text = this.textBoxF12.Text;
            this.f1prog = this.textBoxF1Prog.Text;
            this.f2prog = this.textBoxF2Prog.Text;
            this.f11prog = this.textBoxF11Prog.Text;
            this.f12prog = this.textBoxF12Prog.Text;
            this.f1admin = this.checkBoxAdminF1.Checked;
            this.f2admin = this.checkBoxAdminF2.Checked;
            this.f11admin = this.checkBoxAdminF11.Checked;
            this.f12admin = this.checkBoxAdminF12.Checked;
        }
    }
}
