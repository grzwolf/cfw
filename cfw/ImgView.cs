using System;
using System.Drawing;
using System.Drawing.Imaging;     // bmp handling
using System.IO;
using System.Windows.Forms;

namespace cfw {
    public partial class ImgView : UserControl {
        string _filename = "";
        Bitmap _bmp = null;
        ListView _parent = null;

        public ImgView() {
            this.InitializeComponent();
        }

        public void LoadDocument(string filename, ListView parent) {
            this._filename = filename;
            this._parent = parent;
            // 20161016: the only way not to keeping the file locked: 5th answer from http://stackoverflow.com/questions/4803935/free-file-locked-by-new-bitmapfilepath 
            ImageConverter ic = new ImageConverter();
            this._bmp = (Bitmap)ic.ConvertFrom(File.ReadAllBytes(filename));

            this.imageBox.Image = this._bmp;
            this.imageBox.ZoomToFit();

            this.buttonLeftTurn.Text = "\u21B6";
            this.buttonRightTurn.Text = "\u21B7";
            this.buttonRotate.Text = "\u21BA";
        }

        public void RePaint() {
            this.imageBox.ZoomToFit();
        }

        public void Clear() {
            if ( this.imageBox.Image != null ) {
                this.imageBox.Image.Dispose();
                Bitmap bmp = new Bitmap(10, 10);
                this.imageBox.Image = bmp;
            }
        }

        private void buttonPlus_Click(object sender, EventArgs e) {
            this.imageBox.ZoomChange(1);
        }

        private void button1by1_Click(object sender, EventArgs e) {
            this.imageBox.ZoomToFit();
        }

        private void buttonMinus_Click(object sender, EventArgs e) {
            this.imageBox.ZoomChange(-1);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format) {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach ( ImageCodecInfo codec in codecs ) {
                if ( codec.FormatID == format.Guid ) {
                    return codec;
                }
            }
            return null;
        }
        private void modImage(RotateFlipType rft) {
            if ( this._filename.Length == 0 ) {
                return;
            }
            this._bmp.RotateFlip(rft);
            this.imageBox.Image = this._bmp;
            this.imageBox.ZoomToFit();
            if ( this.checkBoxSave.Checked ) {
                if ( ".jpg.JPG.jpeg.JPEG".Contains(Path.GetExtension(this._filename)) ) {
                    // 20161016: prevent jpg quality loss requires to set the quality level explicitly 
                    ImageCodecInfo jgpEncoder = this.GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    this._bmp.Save(this._filename, jgpEncoder, myEncoderParameters);
                    myEncoderParameter.Dispose();
                } else {
                    // tread other file type as they are
                    this._bmp.Save(this._filename);
                }
            }
            try {
                this._parent.Focus();
            } catch {; }
        }
        private void buttonLeftTurn_Click(object sender, EventArgs e) {
            this.modImage(RotateFlipType.Rotate270FlipNone);
        }
        private void buttonRightTurn_Click(object sender, EventArgs e) {
            this.modImage(RotateFlipType.Rotate90FlipNone);
        }

        private void buttonRotate_Click(object sender, EventArgs e) {
            this.modImage(RotateFlipType.Rotate180FlipNone);
        }
    }
}
