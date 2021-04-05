using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace cfw {
    public partial class HexEdit : Form {
        string m_fileName = "";

        public HexEdit(string filename) {
            this.InitializeComponent();

            this.hexBoxEdit.Font = new Font("Courier New", 11);

            this.OpenFile(filename);
        }

        public void OpenFile(string fileName) {
            if ( !File.Exists(fileName) ) {
                return;
            }

            if ( this.CloseFile() == DialogResult.Cancel ) {
                return;
            }

            try {
                Be.Windows.Forms.DynamicFileByteProvider dynamicFileByteProvider = null;
                try {
                    // try to open in write mode
                    dynamicFileByteProvider = new Be.Windows.Forms.DynamicFileByteProvider(fileName);
                    dynamicFileByteProvider.Changed += new EventHandler(this.byteProvider_Changed);
                } catch ( Exception ex ) // write mode failed
                  {
                    MessageBox.Show(ex.Message, "Error");
                    return;
                }

                this.hexBoxEdit.ByteProvider = dynamicFileByteProvider;

            } catch ( Exception ex1 ) {
                MessageBox.Show(ex1.Message, "Error");
                return;
            }

            this.m_fileName = fileName;
            this.Text = "Hex Editor - " + fileName;
        }

        void byteProvider_Changed(object sender, EventArgs e) {
            this.Text = "Hex Editor - " + this.m_fileName + " *)";
        }


        void SaveFile() {
            if ( this.hexBoxEdit.ByteProvider == null )
                return;

            try {
                Be.Windows.Forms.DynamicFileByteProvider dynamicFileByteProvider = this.hexBoxEdit.ByteProvider as Be.Windows.Forms.DynamicFileByteProvider;
                dynamicFileByteProvider.ApplyChanges();
            } catch ( Exception ex1 ) {
                MessageBox.Show(ex1.Message, "Error");
                return;
            }

            this.Text = "Hex Editor - " + this.m_fileName;
        }

        DialogResult CloseFile() {
            if ( this.hexBoxEdit.ByteProvider == null )
                return DialogResult.OK;

            try {
                if ( this.hexBoxEdit.ByteProvider != null && this.hexBoxEdit.ByteProvider.HasChanges() ) {
                    DialogResult res = MessageBox.Show("Save Changes?", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                    if ( res == DialogResult.Yes ) {
                        this.SaveFile();
                        this.CleanUp();
                    } else if ( res == DialogResult.No ) {
                        this.CleanUp();
                    } else if ( res == DialogResult.Cancel ) {
                        return res;
                    }
                    return res;
                } else {
                    this.CleanUp();
                    return DialogResult.OK;
                }
            } catch ( Exception ) {; }

            return DialogResult.OK;
        }

        void CleanUp() {
            if ( this.hexBoxEdit.ByteProvider != null ) {
                IDisposable byteProvider = this.hexBoxEdit.ByteProvider as IDisposable;
                if ( byteProvider != null )
                    byteProvider.Dispose();
                this.hexBoxEdit.ByteProvider = null;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            SelectFolderOrFile sff = new SelectFolderOrFile();
            sff.Text = "Select File";
            sff.DefaultPath = (this.m_fileName.Length > 0) ? Path.GetDirectoryName(this.m_fileName) : "";
            DialogResult dlr = sff.ShowDialog(this);
            if ( dlr == System.Windows.Forms.DialogResult.OK ) {
                this.OpenFile(sff.ReturnFile);
            }
            sff.Dispose();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            this.SaveFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            if ( this.CloseFile() == DialogResult.Cancel )
                return;

            this.Close();
        }

        private void HexEdit_FormClosing(object sender, FormClosingEventArgs e) {
            if ( this.CloseFile() == DialogResult.Cancel ) {
                e.Cancel = true;
            }
        }

        private void hexBoxEdit_Copied(object sender, EventArgs e) {

        }

        private void hexBoxEdit_CopiedHex(object sender, EventArgs e) {

        }

    }
}
