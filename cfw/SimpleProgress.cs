using System;
using System.Drawing;
using System.Windows.Forms;

namespace cfw {
    public partial class SimpleProgress : Form {
        readonly string sCalm = "Today's slow ";
        private readonly System.Diagnostics.Stopwatch m_sw = new System.Diagnostics.Stopwatch();
        int _frameCount = 0;

        delegate void TwoParamsDelegate(string value, int frame);
        public void UpdateStatus(string value, int frame) {
            if ( this.InvokeRequired ) {
                // We're not in the UI thread, so we need to call BeginInvoke
                this.BeginInvoke(new TwoParamsDelegate(this.UpdateStatus), new object[] { value, frame });
                return;
            }
            // must be on the UI thread if we've got this far
            this.labelText.Text = value;
            this.progressBar1.Value = Math.Min((int)(100 * (frame / (double)this._frameCount)), 100);
            this.labelPct.Text = this.progressBar1.Value.ToString() + "%";
            this.m_sw.Reset();
        }

        // progress text could be set from calling thread
        public string LabelText {
            set {
                string newText = value;
                Size textSize = TextRenderer.MeasureText(value, this.Font);
                if ( textSize.Width >= this.labelText.ClientSize.Width ) {
                    int charCount = 10;
                    string mid = " ... ";
                    do {
                        string lead = value.Substring(0, charCount);
                        string trail = value.Substring(value.Length - charCount, charCount);
                        newText = lead + mid + trail;
                        textSize = TextRenderer.MeasureText(newText, this.Font);
                        charCount++;
                    } while ( textSize.Width < this.labelText.ClientSize.Width - 5 );
                }
                this.labelText.Text = newText;
            }
        }
        // progress Percent could be set from calling thread
        public string LabelPercent {
            set {
                this.labelPct.Text = value;
            }
        }

        // optional cancel button
        public bool ShowCancelButton {
            get {
                return this.buttonCancel.Visible;
            }
            set {
                this.buttonCancel.Visible = value;
            }
        }

        // progress total frame count in mpeg
        public int FrameCount {
            get {
                return this._frameCount;
            }
            set {
                this._frameCount = value;
            }
        }

        // progress bar could be set from calling thread
        public int ProgressValue {
            get {
                this.m_sw.Reset();
                return this.progressBar1.Value;
            }
            set {
                this.progressBar1.Value = value;
                // start a stopwatch
                this.m_sw.Reset();
                this.m_sw.Start();
                // start a timer, when progressbar was set to 0
                if ( value == 0 ) {
                    this.timer1.Start();
                }
                // stop the timer
                if ( value > 98 ) {
                    this.timer1.Stop();
                }
            }
        }

        // constructor
        public SimpleProgress() {
            this.InitializeComponent();

            this.labelTimer.Text = "";
            this.labelText.Text = "";
        }

        // timer monitors the time between the progress updates 
        private void timer1_Tick(object sender, EventArgs e) {
            // if the update frequency is slower than 5s, we show an extra progress in label2 
            if ( this.m_sw.ElapsedMilliseconds > 5000 ) {
                if ( (this.m_sw.ElapsedMilliseconds > 5000) && (this.m_sw.ElapsedMilliseconds < 7000) ) {
                    this.labelTimer.Text = this.sCalm;
                } else {
                    if ( this.labelTimer.Text.Length < 60 ) {
                        this.labelTimer.Text += ".";
                    } else {
                        this.labelTimer.Text = this.sCalm;
                    }
                }
            } else {
                // delete progress update frequency is faster than 5s
                this.labelTimer.Text = "";
            }
        }

        // closing progress may cancel all running operations from caller
        private void buttonCancel_Click(object sender, EventArgs e) {
            this.Close();
        }
    }
}
