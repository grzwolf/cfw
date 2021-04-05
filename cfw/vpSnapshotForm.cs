using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;              // culture info
using System.IO;
using System.Windows.Forms;

namespace VideoPlayer {
    public partial class SnapshotForm : Form {
        private string _path = "";

        public SnapshotForm() {
            this.InitializeComponent();
        }

        public void SetImage(Bitmap bitmap, string path = "") {
            if ( bitmap == null ) {
                return;
            }
            this._path = path;

            this.timeBox.Text = DateTime.Now.ToLongTimeString() + "  --  " + bitmap.Width.ToString() + " x " + bitmap.Height.ToString();

            lock ( this ) {
                Bitmap old = (Bitmap)this.pictureBox.Image;
                if ( old != null ) {
                    old.Dispose();
                }
                this.pictureBox.Image = (Image)bitmap.Clone();
            }
        }

        private void saveButton_Click(object sender, EventArgs e) {
            if ( this.pictureBox.Image == null ) {
                return;
            }

            // get filename
            string file = "";
            if ( this.checkBoxAutoFileName.Checked ) {
                file = DateTime.Now.ToString("yyyyMMdd_HHmmss.", CultureInfo.InvariantCulture) + ".jpg";
            } else {
                this.saveFileDialog.InitialDirectory = this._path;
                if ( this.saveFileDialog.ShowDialog() != DialogResult.OK ) {
                    return;
                }
                file = this.saveFileDialog.FileName;
            }

            // determin image file format from extension
            string ext = Path.GetExtension(file).ToLower();
            ImageFormat format = ImageFormat.Jpeg;
            if ( ext == ".bmp" ) {
                format = ImageFormat.Bmp;
            } else if ( ext == ".png" ) {
                format = ImageFormat.Png;
            }

            // save image
            try {
                lock ( this ) {
                    Bitmap image = (Bitmap)this.pictureBox.Image;
                    image.Save(file, format);
                }
            } catch ( Exception ex ) {
                MessageBox.Show("Failed saving the snapshot.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
    }
}

