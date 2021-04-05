using System;
using System.Windows.Forms;

namespace cfw {
    public partial class CancelDialog : Form {
        public CancelDialog() {
            this.InitializeComponent();
        }
        public CancelDialog(string headLine) {
            this.InitializeComponent();
            this.Text = headLine;
        }

        // Control wants to inform the parent application, that the cancel button was pushed: public event handler and the private sources for this event
        public event EventHandler<EventArgs> WantClose;
        private void button1_Click(object sender, EventArgs e) {
            EventHandler<EventArgs> handler = WantClose;
            if ( handler != null ) {
                handler(sender, EventArgs.Empty);
            }
        }

    }
}
