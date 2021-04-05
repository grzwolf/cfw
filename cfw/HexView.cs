using System.ComponentModel.Design;            // ByteViewer
using System.Drawing;
using System.Windows.Forms;

namespace cfw {
    public partial class HexView : UserControl {
        private readonly System.ComponentModel.Design.ByteViewer byteviewer;

        public HexView() {
            this.InitializeComponent();

            // Initialize the ByteViewer.
            this.byteviewer = new ByteViewer();
            this.byteviewer.Location = new Point(8, 46);
            this.byteviewer.Size = new Size(600, 338);
            //byteviewer.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this.byteviewer.Dock = DockStyle.Fill;
            this.byteviewer.SetBytes(new byte[] { });
            this.Controls.Add(this.byteviewer);
        }

        public string SetFile {
            set {
                this.byteviewer.SetFile(value);
            }
        }
    }
}
