using System.Windows.Forms;

namespace cfw {
    public partial class Loupe : Form {
        public Loupe() {
            this.InitializeComponent();

            this.magnifyingGlass1.UpdateTimer.Start();
        }
    }
}
