using System.Windows.Forms;

namespace cfw {
    public partial class FileViewer : Form {
        public FileViewer(string filename) {
            this.InitializeComponent();

            // Set KeyPreview object to true to allow the form to process the key before the control with focus processes it
            this.KeyPreview = true;

            // load and show
            bool ret = this.fileView1.LoadDocument(this, filename, null);
            if ( !ret ) {
                MessageBox.Show("File access to '" + filename + "' is denied.", "Error");
                this.Close();
            }
        }

        // forward keydown event to control
        private void FileViewer_KeyDown(object sender, KeyEventArgs e) {
            // 20161016: close view
            if ( e.KeyCode == Keys.Escape ) {
                this.fileView1.CloseView();
                this.Close();
            }
        }
    }
}