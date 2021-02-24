using Emgu.CV;                                        // CV pro: allows to re position the frame to read from a file stream
using Emgu.CV.CvEnum;                                 // CV con: camera model & resolution selection is poor --> no issue with a pure video player
using System;
using System.Diagnostics;                             // stopwatch  
using System.Drawing;
using System.IO;                                      // Path File 
using System.Runtime.ExceptionServices;               // handle AccessViolationException
using System.Threading.Tasks;                         // Task
using System.Windows.Forms;

namespace VideoPlayer {
    public partial class VideoPlayerCtl : UserControl {
        readonly Timer _timer = new Timer();
        readonly Timer _timer1S = new Timer();
        string _file = "";
        readonly int _fps = 30;
        int _fpsFrames = 0;
        TimeSpan _lengthVideo;
        DateTime _stamp;
        Capture _capture = null;
        Mat _frame;
        double _totalFrames = 0;
        double _framePos = 0;
        double _framePosLast = 0;
        int _frameCounter = 0;
        Task _t = null;
        string _lastPath = Application.StartupPath;

        public string URL {
            get {
                return this._file;
            }
            set {
                this._file = value;
                // file to show as start argument?
                if ( File.Exists(this._file) ) {
                    this._lastPath = Path.GetDirectoryName(this._file);
                    this.openShowFile();
                    this.buttonPlay_Click(null, null);
                } else {
                    this.videoPlayerClosing();
                    this._file = "";
                }
            }
        }

        public VideoPlayerCtl() {
            this.InitializeComponent();

            // timer 1s only for status update
            this._timer1S.Interval = 1000;
            this._timer1S.Tick += new EventHandler(this.Timer1S_Tick);
            this._timer1S.Start();

            this.buttonFastBack.Tag = ButtonState.Up;
            this.buttonFastFwd.Tag = ButtonState.Up;
            this.buttonStepFwd.Tag = ButtonState.Up;
            this.buttonStepBack.Tag = ButtonState.Up;
        }

        // we update the title bar status 1x per second
        private void Timer1S_Tick(object sender, EventArgs e) {
            this.updateStatus();
        }
        private void updateStatus() {
            if ( this._capture == null ) {
                this.labelStatus.Text = "";
                return;
            }

            if ( this._framePos == -1 ) {
                this.labelStatus.Text = String.Format("camera mode @ {0}fps", this._frameCounter);
                this._frameCounter = 0;
            } else {
                string vidLen = "";
                string vidPos = "";
                try {
                    string fmt = @"hh\:mm\:ss";
                    vidLen = this._lengthVideo.ToString(fmt);
                    double msPos = this._capture.GetCaptureProperty(CapProp.PosMsec) / 1000f;
                    TimeSpan t = TimeSpan.FromSeconds(msPos);
                    vidPos = t.ToString(fmt);
                } catch {; }
                int fps = (int)(this._framePos - this._framePosLast);
                this._framePosLast = this._framePos;
                this.labelStatus.Text = String.Format("{0}   frame: {1}({2}) {3} ({4}) @{5}fps", Path.GetFileName(this._file), this._framePos, this._totalFrames, vidPos, vidLen, fps);
            }
        }

        // open a file medium and show its first content
        void openShowFile() {
            // release all old stuff
            if ( this._capture != null ) {
                this._capture.Dispose();
                this._capture = null;
            }

            // file to show: if _file is empty, we ask for a file
            if ( this._file.Length == 0 ) {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.InitialDirectory = this._lastPath;
                dlg.Filter = "All Files (*.*)|*.*|AVI Files (*.avi)|*.avi|WMV Files (*.wmv)|*.wmv|MP4 Files (*.mp4)|*.mp4";
                if ( dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                    this._file = dlg.FileName;
                    this._lastPath = Path.GetDirectoryName(this._file);
                } else {
                    return;
                }
            }

            // open file + get total frame count 
            this._capture = new Capture(this._file);
            this._totalFrames = this._capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
            this._fpsFrames = (int)this._capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
            this._framePos = this._totalFrames - 1;
            this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, this._framePos);
            double realPos = this._capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames);
            // some WMV do have wrong number of totalFrames - here we adjust this starnge behaviour
            if ( this._framePos > realPos ) {
                int breaker = 0;
                while ( this._framePos > realPos ) {
                    this._totalFrames = realPos;
                    this._framePos = this._totalFrames - 1;
                    this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, this._framePos);
                    realPos = this._capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames);
                    if ( breaker++ > 10 ) {
                        this._totalFrames = realPos;
                        break;
                    }
                }
            }
            double secondsPos = this._capture.GetCaptureProperty(CapProp.PosMsec) / 1000f;
            this._lengthVideo = TimeSpan.FromSeconds(secondsPos);
            FileInfo fi = new FileInfo(Path.GetFileName(this._file));
            this._stamp = fi.LastWriteTime;
            this._stamp = this._stamp - this._lengthVideo;
            this._capture = new Capture(this._file);
            this._framePos = 0;

            // empty picturebox
            if ( this.pictureBox.Image != null ) {
                this.pictureBox.Image.Dispose();
            }
            this.pictureBox.Image = null;

            // positions
            if ( this._totalFrames < 0 ) {
                return;
            }
            this._framePos = 0;
            this._framePosLast = this._framePos;

            // set trackbar accordingly
            this.trackBar.Minimum = 0;
            this.trackBar.Maximum = Math.Max(0, (int)this._totalFrames - 1);
            this.trackBar.Value = 0;

            // we need to make sure, we have _t completed
            this._t = new Task(() => this.nonsense());
            this._t.Start();
            do {
                Application.DoEvents();
            } while ( !this._t.IsCompleted );

            // show the first image of a video
            this.syncFrameToTrackCursor();

            // button text
            this.buttonFile.Text = "- stop -";
        }
        void nonsense() {
        }

        [HandleProcessCorruptedStateExceptions]
        private bool showFrameAtPos(double pos) {
            bool retVal = false;

            // set frame position in stream, if pos == -1 we read from the current frame position
            if ( pos != -1 ) {
                try {
                    this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, pos);
                } catch {
                    return false;
                }
            }

            // w/o this Dispose we would have a memory leak
            if ( this._frame != null ) {
                // red cross: NEVER dispose the picturebox connected frame
                if ( this._framePos > this._totalFrames - 1 ) {
                    return true;
                } else {
                    this._frame.Dispose();
                }
            }
            // get frame from source
            try {
                this._frame = this._capture.QueryFrame();
            } catch {
                return false;
            }
            if ( (this._frame != null) && (this._frame.Width != 0) ) {
                // a frame counter
                this._frameCounter++;
                // show frame
                this.pictureBox.Image = (Image)this._frame.Bitmap.Clone();
                // adjust trackbar
                try {
                    if ( this.trackBar.Value != this._framePos ) {
                        this.Invoke(new Action(() => { this.trackBar.Value = (int)Math.Min(Math.Max(this._framePos, 0), this.trackBar.Maximum); }));
                    }
                } catch {
                    return retVal;
                }
                retVal = true;
            } else {
                // TBD: frame error counter
            }

            return retVal;
        }
        // timer play event handler 
        private void _timer_Tick(object sender, EventArgs e) {
            if ( this._capture == null ) {
                return;
            }

            // play forward
            if ( this.buttonPlay.Text == "||" ) {
                // get frame pos
                this._framePos = this._capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames);
                // max pos reached?
                if ( this._framePos > this._totalFrames - 1 ) {
                    // repeat?
                    if ( this.checkBoxRepeat.Checked ) {
                        // jump to begin
                        this._framePos = 0;
                        this._framePosLast = this._framePos;
                        this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, this._framePos);
                    } else {
                        // stop
                        this.buttonPlay_Click(null, null);
                    }
                }
                // show current frame
                this.showFrameAtPos(-1);
            }
            // play backward
            if ( this.buttonReverse.Text == "||" ) {
                // if task is still running, we return to prevent evil
                if ( !this._t.IsCompleted ) {
                    return;
                }
                // we need to reposition the frame position to read
                this._framePos = Math.Max(this._framePos - 1, 0);
                // async call to reposition frame: in certain files (wmv) _capture.SetCaptureProperty() hangs for a while 
                this._t = new Task(() => this.showFrameAtPos(this._framePos));
                this._t.Start();
                // begin reached?
                if ( this._framePos <= 0 ) {
                    // endless loop?
                    if ( this.checkBoxRepeat.Checked ) {
                        // we need to wait, until _t is finished
                        DateTime begin = DateTime.Now;
                        while ( !this._t.IsCompleted ) {
                            Application.DoEvents();
                            if ( (begin - DateTime.Now).Milliseconds > 2000 ) {
                                this.checkBoxRepeat.Checked = false;
                                return;
                            }
                        }
                        // jump to end: method takes care about silly length info from wmv-files 
                        this.buttonEnd_Click(null, null);
                    } else {
                        // stop
                        this.buttonReverse_Click(null, null);
                    }
                }
            }
        }
        private void buttonPlay_Click(object sender, EventArgs e) {
            if ( this._capture == null ) {
                return;
            }
            if ( this._totalFrames == 0 ) {
                return;
            }

            if ( this.buttonPlay.Text == ">" ) {
                this._timer.Interval = 1000 / this._fps;
                this._timer.Tick += new EventHandler(this._timer_Tick);
                this._timer.Start();
                this.buttonReverse.Enabled = false;
                this.buttonPlay.Text = "||";
                this.buttonPlay.BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
            } else {
                this._timer.Stop();
                this._timer.Tick -= new EventHandler(this._timer_Tick);
                this.buttonReverse.Enabled = true;
                this.buttonPlay.Text = ">";
                this.buttonPlay.BackColor = SystemColors.Control;
            }
        }
        private void buttonReverse_Click(object sender, EventArgs e) {
            if ( this._capture == null ) {
                return;
            }
            if ( this._totalFrames == 0 ) {
                return;
            }
            if ( this._framePos == -1 ) {
                return;
            }

            if ( this.buttonReverse.Text == "<" ) {
                this._timer.Interval = 1000 / this._fps;
                this._timer.Tick += new EventHandler(this._timer_Tick);
                this._timer.Start();
                this.buttonPlay.Enabled = false;
                this.buttonReverse.Text = "||";
                this.buttonReverse.BackColor = System.Drawing.Color.FromArgb(255, 192, 192);
            } else {
                this._timer.Stop();
                this._timer.Tick -= new EventHandler(this._timer_Tick);
                this.buttonPlay.Enabled = true;
                this.buttonReverse.Text = "<";
                this.buttonReverse.BackColor = SystemColors.Control;
            }
        }

        private enum ButtonState { None, Down, Up };
        private void buttonFastFwd_MouseDown(object sender, MouseEventArgs e) {
            this.buttonFastFwd.Tag = ButtonState.Down;
            this.timerRepeater_Tick(null, null);
            this.timerRepeater.Interval = 200;
            this.timerRepeater.Start();
        }
        private void buttonFastFwd_MouseUp(object sender, MouseEventArgs e) {
            this.buttonFastFwd.Tag = ButtonState.Up;
            this.timerRepeater.Stop();
        }
        private void buttonFastBack_MouseDown(object sender, MouseEventArgs e) {
            this.buttonFastBack.Tag = ButtonState.Down;
            this.timerRepeater_Tick(null, null);
            this.timerRepeater.Interval = 200;
            this.timerRepeater.Start();
        }
        private void buttonFastBack_MouseUp(object sender, MouseEventArgs e) {
            this.buttonFastBack.Tag = ButtonState.Up;
            this.timerRepeater.Stop();
        }
        private void buttonStepFwd_MouseDown(object sender, MouseEventArgs e) {
            this.buttonStepFwd.Tag = ButtonState.Down;
            this.timerRepeater_Tick(null, null);
            this.timerRepeater.Interval = 200;
            this.timerRepeater.Start();
        }
        private void buttonStepFwd_MouseUp(object sender, MouseEventArgs e) {
            this.buttonStepFwd.Tag = ButtonState.Up;
            this.timerRepeater.Stop();
        }
        private void buttonBck_MouseDown(object sender, MouseEventArgs e) {
            this.buttonStepBack.Tag = ButtonState.Down;
            this.timerRepeater_Tick(null, null);
            this.timerRepeater.Interval = 200;
            this.timerRepeater.Start();
        }
        private void buttonBck_MouseUp(object sender, MouseEventArgs e) {
            this.buttonStepBack.Tag = ButtonState.Up;
            this.timerRepeater.Stop();
        }
        private void timerRepeater_Tick(object sender, EventArgs e) {
            // remove 200ms delay between the 1st "click" and repetition
            this.timerRepeater.Interval = 100;

            double step = 0;
            if ( (ButtonState)this.buttonFastFwd.Tag == ButtonState.Down ) {
                step = this.trackBar.Maximum / 30f;
                if ( (this.trackBar.Value == this.trackBar.Maximum) && this.checkBoxRepeat.Checked ) {
                    this.trackBar.Value = 0;
                } else {
                    this.trackBar.Value = Math.Min(this.trackBar.Value + (int)step, this.trackBar.Maximum);
                }
            }
            if ( (ButtonState)this.buttonFastBack.Tag == ButtonState.Down ) {
                step = this.trackBar.Maximum / 30f;
                if ( (this.trackBar.Value == 0) && this.checkBoxRepeat.Checked ) {
                    this.trackBar.Value = this.trackBar.Maximum;
                } else {
                    this.trackBar.Value = Math.Max(this.trackBar.Value - (int)step, 0);
                }
            }
            if ( (ButtonState)this.buttonStepFwd.Tag == ButtonState.Down ) {
                step = this.trackBar.Value + 1;
                if ( (step >= this.trackBar.Maximum + 1) && this.checkBoxRepeat.Checked ) {
                    this.trackBar.Value = 0;
                } else {
                    this.trackBar.Value = Math.Min((int)step, this.trackBar.Maximum);
                }
            }
            if ( (ButtonState)this.buttonStepBack.Tag == ButtonState.Down ) {
                step = this.trackBar.Value - 1;
                if ( (step <= -1) && this.checkBoxRepeat.Checked ) {
                    this.trackBar.Value = this.trackBar.Maximum;
                } else {
                    this.trackBar.Value = Math.Max((int)step, 0);
                }
            }
            Application.DoEvents();
            this.syncFrameToTrackCursor();
        }
        // jump to the extreme positions inside the video
        private void buttonBeg_Click(object sender, EventArgs e) {
            if ( this._capture == null ) {
                return;
            }
            if ( this._framePos == -1 ) {
                return;
            }

            this._framePos = 0;
            // sometimes (WMV) is much faster to reopen the file, as to re position the frame
            this._capture.Dispose();
            this._capture = new Capture(this._file);
            this.showFrameAtPos(-1);
            this.updateStatus();
        }
        private void buttonEnd_Click(object sender, EventArgs e) {
            if ( this._capture == null ) {
                return;
            }
            if ( this._totalFrames == 0 ) {
                return;
            }
            if ( this._framePos == -1 ) {
                return;
            }
            if ( !this._t.IsCompleted ) {
                return;
            }

            this._framePos = this._totalFrames - 1;
            this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.PosFrames, this._framePos);
            this.showFrameAtPos(-1);
        }
        // open a file: could be any video or image file
        private void buttonFile_Click(object sender, EventArgs e) {
            if ( this.buttonFile.Text == "Open Media" ) {
                this._file = "";
                this.openShowFile();
            } else {
                this.closeFile();
                this.buttonFile.Text = "Open Media";
            }
        }

        // sync the current frame with respect to the trackbar position
        void syncFrameToTrackCursor() {
            if ( this._framePos == -1 ) {
                return;
            }
            if ( (this._t == null) || !this._t.IsCompleted ) {
                return;
            }

            if ( this.trackBar.Value == 0 ) {
                this.buttonBeg_Click(null, null);
                return;
            }
            if ( this.trackBar.Value == this.trackBar.Maximum ) {
                this.buttonEnd_Click(null, null);
                return;
            }

            this._framePos = this.trackBar.Value;
            this._framePosLast = this._framePos;
            this.showFrameAtPos(this._framePos);
            Application.DoEvents();

            this.updateStatus();
        }
        // mouse lets the video jump to the mouse down position
        private void trackBar_MouseDown(object sender, MouseEventArgs e) {
            this._busy = false;

            if ( this._capture == null ) {
                return;
            }

            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                this.syncFrameToTrackCursor();
                Application.DoEvents();
            }
        }
        // mouse moves trackbar
        bool _busy = false;
        private void trackBar_MouseMove(object sender, MouseEventArgs e) {
            if ( this._busy ) {
                return;
            }

            if ( this._capture == null ) {
                this._busy = false;
                return;
            }

            if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
                this._busy = true;
                bool timerEnabled = this._timer.Enabled;
                this._timer.Stop();
                this.syncFrameToTrackCursor();
                Application.DoEvents();
                this._busy = false;
                if ( timerEnabled ) {
                    this._timer.Start();
                }
            }
        }
        // image snapshot
        private void buttonSnapshot_Click(object sender, EventArgs e) {
            if ( this.pictureBox.Image == null ) {
                return;
            }

            this.ShowSnapshot((Bitmap)this.pictureBox.Image.Clone());
        }
        private void ShowSnapshot(Bitmap snapshot) {
            SnapshotForm snapshotForm = new SnapshotForm();
            snapshotForm.SetImage(snapshot, this._lastPath);
            snapshotForm.Show();
        }

        // save to INI when closing
        private void videoPlayerClosing() {
            // make sure media is stopped
            if ( this.buttonFile.Text != "Open Media" ) {
                this.closeFile();
                Stopwatch sw = new Stopwatch();
                while ( this.buttonFile.Text != "Open Media" ) {
                    Application.DoEvents();
                    if ( sw.ElapsedMilliseconds > 300 ) {
                        break;
                    }
                }
            }
        }

        // close media / file
        private void closeFile() {
            if ( this.buttonPlay.Text == "||" ) {
                this.buttonPlay_Click(null, null);
            }
            if ( this.buttonReverse.Text == "||" ) {
                this.buttonReverse_Click(null, null);
            }
            if ( this._capture != null ) {
                this._capture.Stop();
                this._capture.Dispose();
                this._capture = null;
            }
            this.buttonFile.Text = "Open Media";
        }

    }

    public class CustomPaintTrackBar : TrackBar {
        public event PaintEventHandler PaintOver;
        int _lastPos = 0;
        public CustomPaintTrackBar() : base() {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }
        protected override void WndProc(ref Message m) {
            base.WndProc(ref m);
            // WM_PAINT
            //if ( m.Msg == 0x0F ) {
            //    using ( Graphics lgGraphics = Graphics.FromHwndInternal(m.HWnd) ) {
            //        OnPaintOver(new PaintEventArgs(lgGraphics, this.ClientRectangle));
            //    }
            //}
        }
        protected virtual void OnPaintOver(PaintEventArgs e) {
            if ( PaintOver != null ) {
                PaintOver(this, e);
            }
            // Paint over code here
            e.Graphics.DrawRectangle(Pens.LightGray, this._lastPos, 0, 6, 6);
            int pos = (int)((this.Value / (double)this.Maximum) * (this.Width - 15));
            e.Graphics.DrawRectangle(Pens.Red, pos + 4, 0, 6, 6);
            this._lastPos = pos;
        }
    }
}
