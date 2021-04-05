using System.Windows.Forms;

namespace cfw {
    public partial class HexViewerForm : Form {
        public HexViewerForm(string FileName) {
            this.InitializeComponent();

            this.hexView1.SetFile = FileName;

            this.Text = "Hex Viewer - " + FileName;
        }
    }
}
