using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DesktopColorPicker {
    /// <summary>
    /// A fixed magnifying glass for placing on a control
    /// </summary>
    public partial class MagnifyingGlass : UserControl {
        private readonly System.Windows.Forms.Timer _UpdateTimer = new System.Windows.Forms.Timer();
        private int _PixelSize = 5;
        private int _PixelRange = 10;
        private bool _ShowPixel = true;
        private bool _ShowPosition = true;
        private string _PosFormat = "#x ; #y";
        private bool _FollowCursor = false;
        internal Bitmap _ScreenShot = null;
        internal MovingMagnifyingGlass _DisplayForm = null;
        private Point _LastPosition = Point.Empty;
        private readonly MovingMagnifyingGlass _MovingGlass = null;
        private ContentAlignment _PosAlign = ContentAlignment.TopLeft;
        private bool _UseMovingGlass = false;

        /// <summary>
        /// Instance of the magnifying glass with moving glass, if the user clicks on this one
        /// </summary>
        public MagnifyingGlass()
            : this(true) {
        }

        /// <summary>
        /// Instance of the magnifying glass
        /// </summary>
        /// <param name="movingGlass">Create a moving glass if the user clicks on this one?</param>
        public MagnifyingGlass(bool movingGlass) {
            if ( movingGlass ) {
                // Moving glass is enabled
                this._MovingGlass = new MovingMagnifyingGlass();
                this.MovingGlass.MagnifyingGlass.ShowPosition = false;
                this.MovingGlass.MagnifyingGlass.DisplayUpdated += new DisplayUpdatedDelegate(this.MagnifyingGlass_DisplayUpdated);
                this.MovingGlass.MagnifyingGlass.Click += new EventHandler(this.MovingGlass_Click);
                MouseWheel += new MouseEventHandler(this.MagnifyingGlass_MouseWheel);
                this.Cursor = Cursors.SizeAll;
                this.UseMovingGlass = true;
            }
            this._UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
            Click += new System.EventHandler(this.MagnifyingGlass_Click);
            this.CalculateSize();
        }

        #region Properties
        [Description("Magnifying ratio (calculate PixelRange*PixelSize*2+PixelSize for the final control size, min. 3)")]
        public int PixelSize {
            get {
                return this._PixelSize;
            }
            set {
                int temp = value;
                if ( temp < 3 ) {
                    // Minimum size
                    temp = 3;
                }
                if ( (double)temp / 2 == (double)Math.Floor((double)temp / 2) ) {
                    // Use only integers that can't be divided by 2
                    temp++;
                }
                this._PixelSize = temp;
                this.CalculateSize();
            }
        }


        [Description("Get/set if the moving glass feature should be used")]
        public bool UseMovingGlass {
            get {
                return this._UseMovingGlass;
            }
            set {
                if ( this.MovingGlass != null ) {
                    this._UseMovingGlass = value;
                }
            }
        }

        [Description("Get/set the align of the position (choose everything, but not the middle")]
        public ContentAlignment PosAlign {
            get {
                return this._PosAlign;
            }
            set {
                this._PosAlign = (!value.ToString().ToLower().StartsWith("middle")) ? value : ContentAlignment.TopLeft;
            }
        }

        [Description("Get/set the position display string format (you have to use #x and #y for the corrdinates values)")]
        public string PosFormat {
            get {
                return this._PosFormat;
            }
            set {
                // Settings without the #x and #y variables will be ignored
                this._PosFormat = (value != null && value != "" && value.Contains("#x") && value.Contains("#y")) ? value : "#x ; #y";
                this.Invalidate();
            }
        }

        [Description("The moving glass, if the user clicks on this")]
        public MovingMagnifyingGlass MovingGlass {
            get {
                return this._MovingGlass;
            }
        }

        /// <summary>
        /// Returns true, if enabled, visible and not in designer mode
        /// </summary>
        [Browsable(false)]
        public bool IsEnabled {
            get {
                return this.Visible && this.Enabled && !this.DesignMode;
            }
        }

        [Browsable(false)]
        internal bool FollowCursor {
            get {
                return this._FollowCursor;
            }
            set {
                if ( !(this._FollowCursor = value) ) {
                    // Exit the following mode
                    if ( this._ScreenShot != null ) {
                        this._ScreenShot.Dispose();
                        this._ScreenShot = null;
                    }
                }
            }
        }

        [Description("Get/set the pixel range (calculate PixelRange*PixelSize*2+PixelSize for the final control size, min. 1)")]
        public int PixelRange {
            get {
                return this._PixelRange;
            }
            set {
                int temp = value;
                if ( temp < 1 ) {
                    // Minimum range is one pixel
                    temp = 1;
                }
                this._PixelRange = temp;
                this.CalculateSize();
            }
        }

        [Description("Get/set if the active pixel should be shown")]
        public bool ShowPixel {
            get {
                return this._ShowPixel;
            }
            set {
                this._ShowPixel = value;
                this.Invalidate();
            }
        }

        [Description("Get/set if the current cursor position should be shown")]
        public bool ShowPosition {
            get {
                return this._ShowPosition;
            }
            set {
                this._ShowPosition = value;
                this.Invalidate();
            }
        }

        [Description("Get the control size (settings will be ignored)")]
        new public Size Size {
            get {
                return base.Size;
            }
            set {
                // Settings will be ignored 'cause size will be calculated internal
            }
        }

        [Description("Get the timer that updates the display in an interval")]
        public Timer UpdateTimer {
            get {
                return this._UpdateTimer;
            }
        }

        [Description("Get the color of the current pixel")]
        public Color PixelColor {
            get {
                Bitmap bmp = null;
                try {
                    // Make a screenshot of the pixel from the current cursor position
                    bmp = new Bitmap(1, 1);
                    using ( Graphics g = Graphics.FromImage(bmp) ) {
                        bool makeScreenshot = !this.FollowCursor;// Make a real screenshot?
                        if ( makeScreenshot ) {
                            if ( this.MovingGlass != null ) {
                                //Only make a real screenshot if the moving glass is inactive
                                makeScreenshot &= !this.MovingGlass.Visible;
                            }
                        }
                        if ( !this.FollowCursor ) {
                            // Make a real screenshot
                            g.CopyFromScreen(Cursor.Position, new Point(0, 0), bmp.Size);
                        } else {
                            // Use the screen image for the screenshot
                            bool createScreenshot = false;// Did we create a screenshot for this?
                            if ( this.FollowCursor ) {
                                // Create the screenshot only if it wasn't done yet
                                createScreenshot = this._ScreenShot == null;
                            } else {
                                // Create the screenshot only of the moving glass has not done it yet
                                createScreenshot = this.MovingGlass.MagnifyingGlass._ScreenShot == null;
                            }
                            if ( createScreenshot ) {
                                // Create a new screen image
                                this.MakeScreenshot();
                            }
                            if ( this.FollowCursor ) {
                                // We're the moving glass
                                g.DrawImage(this._ScreenShot, new Rectangle(new Point(0, 0), new Size(1, 1)), new Rectangle(Cursor.Position, new Size(1, 1)), GraphicsUnit.Pixel);
                            } else {
                                // Use the moving glasses screenshot
                                g.DrawImage(this.MovingGlass.MagnifyingGlass._ScreenShot, new Rectangle(new Point(0, 0), new Size(1, 1)), new Rectangle(Cursor.Position, new Size(1, 1)), GraphicsUnit.Pixel);
                            }
                            if ( createScreenshot ) {
                                // Destroy the screenshot if we only needed to create one for this
                                this._ScreenShot.Dispose();
                            }
                        }
                    }
                    // Return the pixel color
                    return bmp.GetPixel(0, 0);
                } finally {
                    bmp.Dispose();
                }
            }
        }
        #endregion

        #region Painting
        protected override void OnPaintBackground(PaintEventArgs e) {
            // Only paint the background, if we're disabled or in DesignMode
            if ( !this.IsEnabled ) {
                base.OnPaintBackground(e);
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            if ( !this.IsEnabled ) {
                // Draw only if visible, enabled and not in DesignMode
                return;
            }
            // Set the InterpolationMode to NearestNeighbor to see the pixels clearly
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            // Prepare some shortcut variables for a better overview
            Point pos = Cursor.Position;
            Rectangle scr = Screen.PrimaryScreen.Bounds;// The screen size
            Point zeroPoint = new Point(0, 0);
            #region Set the new display window location if we follow the cursor
            if ( this.FollowCursor ) {
                Point loc = new Point(Cursor.Position.X - this.PixelRange * this.PixelSize, Cursor.Position.Y - this.PixelRange * this.PixelSize);
                if ( loc.X < 0 ) {
                    loc = new Point(0, loc.Y);
                }
                if ( loc.X + this.Width > Screen.PrimaryScreen.Bounds.Width ) {
                    loc = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, loc.Y);
                }
                if ( loc.Y < 0 ) {
                    loc = new Point(loc.X, 0);
                }
                if ( loc.Y + this.Height > Screen.PrimaryScreen.Bounds.Height ) {
                    loc = new Point(loc.X, Screen.PrimaryScreen.Bounds.Height - this.Height);
                }
                this._DisplayForm.Location = loc;
            }
            #endregion
            #region Make the screenshot
            Rectangle shot = new Rectangle(zeroPoint, new Size(this.Size.Width / this.PixelSize, this.Size.Height / this.PixelSize));// The final screenshot size and position
            Point defaultLocation = new Point(pos.X - this.PixelRange, pos.Y - this.PixelRange);// The screenshot default location
            shot.Location = defaultLocation;
            if ( shot.Location.X < 0 ) {
                // The area is going over the left screen border
                shot.Size = new Size(shot.Size.Width + shot.Location.X, shot.Size.Height);
                shot.Location = new Point(0, shot.Location.Y);
            } else if ( shot.Location.X > scr.Width ) {
                // The area is going over the right screen border
                shot.Size = new Size(shot.Location.X - scr.Width, shot.Size.Height);
            }
            if ( shot.Location.Y < 0 ) {
                // The area is going over the upper screen border
                shot.Size = new Size(shot.Size.Width, shot.Size.Height + shot.Location.Y);
                shot.Location = new Point(shot.Location.X, 0);
            } else if ( shot.Location.Y > scr.Height ) {
                // The area is going over the bottom screen border
                shot.Size = new Size(shot.Size.Width, shot.Location.Y - scr.Height);
            }

            if ( shot.Width <= 0 )
                shot.Width = 1;
            if ( shot.Height <= 0 )
                shot.Height = 1;

            Bitmap screenShot = new Bitmap(shot.Width, shot.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);// The screenshot imag;
            using ( Graphics g = Graphics.FromImage(screenShot) ) {
                bool makeScreenshot = !this.FollowCursor;// Make areal screenshot?
                if ( makeScreenshot ) {
                    if ( this.MovingGlass != null ) {
                        // Only make a real screenshot if the moving glass is inactive
                        makeScreenshot &= !this.MovingGlass.Visible;
                    }
                }
                if ( makeScreenshot ) {
                    // Make screenshot
                    g.CopyFromScreen(shot.Location, zeroPoint, shot.Size);
                } else {
                    // Copy from work screenshot
                    if ( this.FollowCursor ) {
                        // We're the moving glass
                        g.DrawImage(this._ScreenShot, new Rectangle(zeroPoint, screenShot.Size), shot, GraphicsUnit.Pixel);
                    } else {
                        // We're not the moving glass, but we should use the work screenshot 
                        // of the moving glass, 'cause if it's fully visible we'd copy the 
                        // moving glass display area...
                        g.DrawImage(this.MovingGlass.MagnifyingGlass._ScreenShot, new Rectangle(zeroPoint, screenShot.Size), shot, GraphicsUnit.Pixel);
                    }
                }
            }
            #endregion
            #region Paint the screenshot scaled to the display
            Rectangle display = new Rectangle(zeroPoint, this.Size);// The rectangle within the display to show the screenshot
            Size displaySize = new Size(shot.Width * this.PixelSize, shot.Height * this.PixelSize);// The default magnified screenshot size
            if ( defaultLocation.X < 0 || defaultLocation.X > scr.Width ) {
                if ( defaultLocation.X < 0 ) {
                    // Display the screenshot with right align
                    display.Location = new Point(display.Width - displaySize.Width, display.Location.Y);
                }
                // Change the display area width to the width of the magnified screenshot
                display.Size = new Size(displaySize.Width, display.Size.Height);
            }
            if ( defaultLocation.Y < 0 || defaultLocation.Y > scr.Height ) {
                if ( defaultLocation.Y < 0 ) {
                    // Display the screenshot with bottom align
                    display.Location = new Point(display.Location.X, display.Height - displaySize.Height);
                }
                // Change the display area height to the height of the magnified screenshot
                display.Size = new Size(display.Size.Width, displaySize.Height);
            }
            if ( displaySize != this.Size ) {
                // Paint the background 'cause the magnified screenshot size is different from the display size and we have a out-of-screen area
                e.Graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(zeroPoint, this.Size));
            }
            // Scale and paint the screenshot
            e.Graphics.DrawImage(screenShot, display);
            screenShot.Dispose();
            #endregion
            #region Paint everything else to the display
            // Show the current pixel in a black/white bordered rectangle in the middle of the display
            if ( this.ShowPixel ) {
                int xy = this.PixelSize * this.PixelRange;                                                      // -3 sonst wird das Pixel an falscher Position angezeigt
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.Black)), new Rectangle(new Point(xy - 3, xy - 3), new Size(this.PixelSize + 1, this.PixelSize + 1)));
                e.Graphics.DrawRectangle(new Pen(new SolidBrush(Color.White)), new Rectangle(new Point(xy - 2, xy - 2), new Size(this.PixelSize - 1, this.PixelSize - 1)));
            }
            // Show the cursor position coordinates on a fixed colored background rectangle in the display
            if ( this.ShowPosition ) {
                // Parse the format string
                string posText = this.PosFormat;
                posText = posText.Replace("#x", pos.X.ToString());
                posText = posText.Replace("#y", pos.Y.ToString());
                // Calculate where to paint
                Size textSize = e.Graphics.MeasureString(posText, this.Font).ToSize();
                if ( textSize.Width + 6 <= this.Width && textSize.Height + 6 <= this.Height )// Continue only if the display is bigger or equal to the needed size
                {
                    string posString = this.PosAlign.ToString().ToLower();// The align as text (for less code)
                    Point posZero = Point.Empty;// The zero coordinates for the position display
                    if ( posString.StartsWith("top") ) {
                        posZero = new Point(0, 0);
                    } else {
                        posZero = new Point(0, this.Height - textSize.Height);
                    }
                    if ( posString.Contains("center") ) {
                        posZero = new Point((int)Math.Ceiling((double)(this.Width - textSize.Width) / 2), posZero.Y);
                    } else if ( posString.Contains("right") ) {
                        posZero = new Point(this.Width - textSize.Width - 6, posZero.Y);
                    }
                    // Paint the text background rectangle and the text on it
                    //                    e.Graphics.FillRectangle(new SolidBrush(BackColor), new Rectangle(posZero, new Size(textSize.Width + 6, textSize.Height + 6)));
                    //                    e.Graphics.DrawString(posText, Font, new SolidBrush(ForeColor), new PointF(posZero.X + 3, posZero.Y + 3));
                }
            }
            #endregion
        }
        #endregion

        /// <summary>
        /// Set a new size
        /// </summary>
        /// <param name="pixelSize">Pixel size value</param>
        /// <param name="pixelRange">Pixel range value</param>
        public void SetNewSize(int pixelSize, int pixelRange) {
            this.SuspendLayout();
            this.PixelSize = pixelSize;
            this.PixelRange = pixelRange;
            this.ResumeLayout(true);
        }

        private void CalculateSize() {
            // Calculate the new control size depending on the magnifying ratio and the pixel range to display
            int wh = this.PixelSize * (this.PixelRange * 2 + 1);
            base.Size = new Size(wh, wh);
        }

        private void UpdateTimer_Tick(object sender, EventArgs e) {
            try {
                // Redraw and continue the timer if we're visible, enabled and not in DesignMode
                // The timer is also disabled here because the Timer component seems to have an error (it will crashafter a while!?). Restarting the timer is a workaround.
                this.UpdateTimer.Stop();
                if ( this.IsEnabled ) {
                    //if (_LastPosition == Cursor.Position)
                    //{
                    //    // Refresh only if the position has changed
                    //    return;
                    //}
                    // Remember the current cursor position
                    this._LastPosition = Cursor.Position;
                    // Repaint everything
                    this.Invalidate();
                    // Release the event after the display has been updated
                    this.OnDisplayUpdated();
                }
            } finally {
                // Restart the timer
                this.UpdateTimer.Start();
            }
        }

        /// <summary>
        /// Delegate for the DisplayUpdated event
        /// </summary>
        /// <param name="sender">The sending MagnifyingGlass control</param>
        public delegate void DisplayUpdatedDelegate(MagnifyingGlass sender);
        /// <summary>
        /// Fired after the display has been refreshed by the UpdateTimer or the moving glass
        /// </summary>
        public event DisplayUpdatedDelegate DisplayUpdated;
        private void OnDisplayUpdated() {
            if ( DisplayUpdated != null ) {
                DisplayUpdated(this);
            }
        }

        #region Moving glass related methods
        private void MagnifyingGlass_Click(object sender, EventArgs e) {
            // Show the moving glass
            if ( this.MovingGlass != null && this.IsEnabled && this.UseMovingGlass ) {
            }
        }

        private void MagnifyingGlass_MouseWheel(object sender, MouseEventArgs e) {
            // Resize on mouse wheel actions
            if ( this._DisplayForm != null && e.Delta != 0 ) {
                if ( e.Delta > 0 ) {
                    if ( (this.PixelRange + 1) * this.PixelRange * 2 <= Screen.PrimaryScreen.Bounds.Width && (this.PixelRange + 1) * this.PixelRange * 2 <= Screen.PrimaryScreen.Bounds.Height ) {
                        this.PixelRange++;
                        this.PixelSize += 2;
                    }
                } else {
                    if ( this.PixelRange - 1 >= 5 ) {
                        this.PixelRange--;
                    }
                    if ( this.PixelSize > 3 ) {
                        this.PixelSize -= 2;
                    }
                }
            }
        }

        private void MovingGlass_Click(object sender, EventArgs e) {
            // Hide the moving glass on mouse click
            this.MovingGlass.Hide();
        }

        private void MagnifyingGlass_DisplayUpdated(MagnifyingGlass sender) {
            // Refresh if the moving one has refreshed
            this.Invalidate();
            this.OnDisplayUpdated();
        }

        internal void MakeScreenshot() {
            // Copy the current screen without this control for the following glass
            this.OnBeforeMakingScreenshot();
            this._ScreenShot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using ( Graphics g = Graphics.FromImage(this._ScreenShot) ) {
                bool visible = this._DisplayForm.Visible;
                if ( visible ) {
                    this._DisplayForm.Visible = false;
                }
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), this._ScreenShot.Size);
                g.Flush();
                if ( visible ) {
                    this._DisplayForm.Visible = true;
                }
            }
            this.OnAfterMakingScreenshot();
        }

        /// <summary>
        /// Delegate for the BeforeMakingScreenshot and the AfterMakingScreenshot events
        /// </summary>
        /// <param name="sender">The sending MagnifyingGlass object</param>
        public delegate void MakingScreenshotDelegate(object sender);
        /// <summary>
        /// Fired before making a screenshot
        /// </summary>
        public event MakingScreenshotDelegate BeforeMakingScreenshot;
        /// <summary>
        /// Fired after making a screenshot
        /// </summary>
        public event MakingScreenshotDelegate AfterMakingScreenshot;
        private void OnBeforeMakingScreenshot() {
            if ( BeforeMakingScreenshot != null ) {
                BeforeMakingScreenshot(this);
            }
        }
        private void OnAfterMakingScreenshot() {
            if ( AfterMakingScreenshot != null ) {
                AfterMakingScreenshot(this);
            }
        }
        #endregion
    }

    /// <summary>
    /// A free magnifying glass that follows the cursor
    /// </summary>
    public class MovingMagnifyingGlass : Form {
        private readonly MagnifyingGlass _MagnifyingGlass = new MagnifyingGlass(false);

        public MovingMagnifyingGlass() {
            this.Opacity = .75;// Added because it makes things easier
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.MagnifyingGlass.PixelSize = 10;
            this.MagnifyingGlass.PixelRange = 5;
            this.MagnifyingGlass.BackColor = Color.Black;
            this.MagnifyingGlass.ForeColor = Color.White;
            this.MagnifyingGlass.UpdateTimer.Interval = 200;
            this.MagnifyingGlass._DisplayForm = this;
            this.MagnifyingGlass.FollowCursor = true;
            this.MagnifyingGlass.BorderStyle = BorderStyle.FixedSingle;
            this.MagnifyingGlass.Resize += new EventHandler(this.MagnifyingGlass_Resize);
            this.MagnifyingGlass.Location = new Point(0, 0);
            this.Controls.Add(this.MagnifyingGlass);
            this.Size = this.MagnifyingGlass.Size;
            this.Text = "Moving magnifying glass";
        }

        /// <summary>
        /// Show the window and enable the timer
        /// </summary>
        new public void Show() {
            this.MagnifyingGlass.MakeScreenshot();
            Cursor.Position = new Point(0, 0);
            base.Show();
            this.MagnifyingGlass.UpdateTimer.Start();
            Cursor.Hide();
        }

        /// <summary>
        /// Hide the window and disable the timer
        /// </summary>
        new public void Hide() {
            base.Hide();
            this.MagnifyingGlass.UpdateTimer.Stop();
            Cursor.Show();
            this.MagnifyingGlass._ScreenShot.Dispose();
            this.MagnifyingGlass._ScreenShot = null;
        }

        private void MagnifyingGlass_Resize(object sender, EventArgs e) {
            // Always stay as big as the glass
            this.Size = this.MagnifyingGlass.Size;
        }

        /// <summary>
        /// The magnifying glass object
        /// </summary>
        [Description("The magnifying glass object")]
        public MagnifyingGlass MagnifyingGlass {
            get {
                return this._MagnifyingGlass;
            }
        }
    }
}
