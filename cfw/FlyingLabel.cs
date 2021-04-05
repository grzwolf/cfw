using System;
using System.Windows.Forms;

namespace cfw {
    public partial class FlyingLabel : Form {
        string _fullPath = "n/a";
        string _tailPath = "";

        public FlyingLabel() {
            this.InitializeComponent();
            // we are abusing the IsDisposed property in MainForm to signal the status of the FlyingLabel
            this.Close();
        }

        public FlyingLabel(string text) {
            this.InitializeComponent();
            this.LabelText = text;
        }

        public string LabelText {
            get {
                return this.label.Text;
            }
            set {
                this.label.Text = value;
            }
        }

        public string TailPath {
            get {
                return this._tailPath;
            }
            set {
                this._tailPath = value;
            }
        }

        public string FullPath {
            get {
                return this._fullPath;
            }
            set {
                this._fullPath = value;
            }
        }

        public IntPtr LabelHandle {
            get {
                return this.label.Handle;
            }
        }

        // activate form w/o giving focus to it
        protected override bool ShowWithoutActivation {
            get { return true; }
        }

        // click on the label shall cause closing this form + notifying parent & transferring FullPath to parent
        private void label_MouseDown(object sender, MouseEventArgs e) {
            EventHandler<ChangeEventArgs> ev = ChangeEvent;
            if ( e.Button == MouseButtons.Left ) {
                // make parent change the current path (aka LoadListView) according to selection
                if ( ev != null ) {
                    ev(this, new ChangeEventArgs(this._fullPath));
                }
                this.Close();
            }
            if ( e.Button == MouseButtons.Right ) {
                // just close FlyingLabel, just so happens that the button right click context menu will open
                if ( ev != null ) {
                    ev(this, new ChangeEventArgs(""));
                }
                this.Close();
            }
        }
        // PUBLIC: inform parent about this form was somehow changed 
        public class ChangeEventArgs : EventArgs {
            public ChangeEventArgs(string fullpath) {
                this.fullpath = fullpath;
            }
            public string fullpath { get; set; }
        }
        public event EventHandler<ChangeEventArgs> ChangeEvent;

    }
}
