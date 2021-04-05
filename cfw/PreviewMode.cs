using System;
using System.Windows.Forms;

namespace cfw {
    public partial class PreviewMode : Form {
        public PreviewMode() {
            this.InitializeComponent();
        }

        public bool Img {
            get {
                return (this.checkBoxImg.Checked);
            }
            set {
                this.checkBoxImg.Checked = value;
            }
        }
        public bool Doc {
            get {
                return (this.checkBoxDoc.Checked);
            }
            set {
                this.checkBoxDoc.Checked = value;
            }
        }
        public bool Pdf {
            get {
                return (this.checkBoxPdf.Checked);
            }
            set {
                this.checkBoxPdf.Checked = value;
            }
        }
        public bool Zip {
            get {
                return (this.checkBoxZip.Checked);
            }
            set {
                this.checkBoxZip.Checked = value;
            }
        }
        public bool Htm {
            get {
                return (this.checkBoxHtm.Checked);
            }
            set {
                this.checkBoxHtm.Checked = value;
            }
        }
        public bool AsIs {
            get {
                return (this.checkBoxAsIs.Checked);
            }
            set {
                this.checkBoxAsIs.Checked = value;
            }
        }
        public bool WmpAudio {
            get {
                return (this.checkBoxWmpAudio.Checked);
            }
            set {
                this.checkBoxWmpAudio.Checked = value;
            }
        }
        public bool WmpVideo {
            get {
                return (this.checkBoxWmpVideo.Checked);
            }
            set {
                this.checkBoxWmpVideo.Checked = value;
            }
        }
        public bool CfwVideo {
            get {
                return (this.checkBoxCfwVideo.Checked);
            }
            set {
                this.checkBoxCfwVideo.Checked = value;
            }
        }

        private void checkBoxWmpVideo_Click(object sender, EventArgs e) {
            if ( this.checkBoxWmpVideo.Checked ) {
                this.checkBoxCfwVideo.Checked = false;
            }
        }

        private void checkBoxCfwVideo_Click(object sender, EventArgs e) {
            if ( this.checkBoxCfwVideo.Checked ) {
                this.checkBoxWmpVideo.Checked = false;
            }
        }
    }
}
