using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CyotecViewer {
    // Cyotek ImageBox
    // Copyright (c) 2010 Cyotek. All Rights Reserved.
    // http://cyotek.com

    [DefaultProperty("Image"), ToolboxBitmap(typeof(ImageBox))]
    public partial class ImageBox : ScrollableControl {

        #region  Private Class Member Declarations  

        private static readonly int MinZoom = 10;
        private static readonly int MaxZoom = 3500;
        private readonly String[] m_Labels =
        {
        "2k0g2v0m", "6k0g2v0m", "av0g2v0m", "ev0g2v0m",
        "2kdi2v0m", "6kdi2v0m", "avdi2v0m", "evdi2v0m",
        "6k251t10", "99251t10", "bz251t10",
        "6kbm1t10", "99bm1t10", "bzbm1t10",
        "2l57101t", "2l7k101t",
        "gp57101t", "gp7k101t",
        "4d290x0x", "4dbg0x0x",
        "2m3d0x0x", "2mac0x0x",
        "f0290x0x", "f0bg0x0x",
        "grac0x0x", "gr3d0x0x",
        "433yc774"
    };
        readonly Font m_labelFont = new Font("Courier New", 14);       // Font für Labels

        #endregion  Private Class Member Declarations  

        #region  Private Member Declarations  

        private bool _autoCenter;
        private bool _autoPan;
        private BorderStyle _borderStyle;
        private int _gridCellSize;
        private Color _gridColor;
        private Color _gridColorAlternate;
        [Category("Property Changed")]
        private ImageBoxGridDisplayMode _gridDisplayMode;
        private ImageBoxGridScale _gridScale;
        private Bitmap _gridTile;
        private System.Drawing.Image _image;
        private InterpolationMode _interpolationMode;
        private bool _invertMouse;
        private bool _isPanning;
        private bool _sizeToFit;
        private Point _startMousePosition;
        private Point _startScrollPosition;
        private TextureBrush _texture;
        private int _zoom;
        private int _zoomIncrement;

        #endregion  Private Member Declarations  

        #region  Public Constructors  

        public ImageBox() {
            this.InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.StandardDoubleClick, true);
            this.UpdateStyles();

            this.BackColor = Color.White;
            this.AutoSize = true;
            this.GridScale = ImageBoxGridScale.Small;
            this.GridDisplayMode = ImageBoxGridDisplayMode.Client;
            this.GridColor = Color.Gainsboro;
            this.GridColorAlternate = Color.White;
            this.GridCellSize = 8;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.AutoPan = true;
            this.Zoom = 100;
            this.ZoomIncrement = 20;
            this.InterpolationMode = InterpolationMode.Default;
            this.AutoCenter = true;

        }

        #endregion  Public Constructors  

        #region  Events  

        [Category("Property Changed")]
        public event EventHandler AutoCenterChanged;

        [Category("Property Changed")]
        public event EventHandler AutoPanChanged;

        [Category("Property Changed")]
        public event EventHandler BorderStyleChanged;

        [Category("Property Changed")]
        public event EventHandler GridCellSizeChanged;

        [Category("Property Changed")]
        public event EventHandler GridColorAlternateChanged;

        [Category("Property Changed")]
        public event EventHandler GridColorChanged;

        [Category("Property Changed")]
        public event EventHandler GridDisplayModeChanged;

        [Category("Property Changed")]
        public event EventHandler GridScaleChanged;

        [Category("Property Changed")]
        public event EventHandler ImageChanged;

        [Category("Property Changed")]
        public event EventHandler InterpolationModeChanged;

        [Category("Property Changed")]
        public event EventHandler InvertMouseChanged;

        [Category("Property Changed")]
        public event EventHandler PanEnd;

        [Category("Property Changed")]
        public event EventHandler PanStart;

        [Category("Property Changed")]
        public event EventHandler SizeToFitChanged;

        [Category("Property Changed")]
        public event EventHandler ZoomChanged;

        [Category("Property Changed")]
        public event EventHandler ZoomIncrementChanged;

        #endregion  Events  

        #region  Overriden Properties  

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), DefaultValue(true)]
        public override bool AutoSize {
            get { return base.AutoSize; }
            set {
                if ( base.AutoSize != value ) {
                    base.AutoSize = value;
                    this.AdjustLayout();
                }
            }
        }

        [DefaultValue(typeof(Color), "White")]
        public override Color BackColor {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Image BackgroundImage {
            get { return base.BackgroundImage; }
            set { base.BackgroundImage = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override ImageLayout BackgroundImageLayout {
            get { return base.BackgroundImageLayout; }
            set { base.BackgroundImageLayout = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Font Font {
            get { return base.Font; }
            set { base.Font = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string Text {
            get { return base.Text; }
            set { base.Text = value; }
        }

        #endregion  Overriden Properties  

        #region  Public Overridden Methods  

        public override Size GetPreferredSize(Size proposedSize) {
            Size size;

            if ( this.Image != null ) {
                int width;
                int height;

                // get the size of the image
                width = this.ScaledImageWidth;
                height = this.ScaledImageHeight;

                // add an offset based on padding
                width += this.Padding.Horizontal;
                height += this.Padding.Vertical;

                // add an offset based on the border style
                width += this.GetBorderOffset();
                height += this.GetBorderOffset();

                size = new Size(width, height);
            } else
                size = base.GetPreferredSize(proposedSize);

            return size;
        }

        #endregion  Public Overridden Methods  

        #region  Protected Overridden Methods  

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if ( disposing ) {
                if ( this.components != null )
                    this.components.Dispose();

                if ( this._texture != null ) {
                    this._texture.Dispose();
                    this._texture = null;
                }

                if ( this._gridTile != null ) {
                    this._gridTile.Dispose();
                    this._gridTile = null;
                }
            }
            base.Dispose(disposing);
        }

        protected override bool IsInputKey(Keys keyData) {
            bool result;

            if ( (keyData & Keys.Right) == Keys.Right | (keyData & Keys.Left) == Keys.Left | (keyData & Keys.Up) == Keys.Up | (keyData & Keys.Down) == Keys.Down )
                result = true;
            else
                result = base.IsInputKey(keyData);

            return result;
        }

        protected override void OnBackColorChanged(EventArgs e) {
            base.OnBackColorChanged(e);

            this.Invalidate();
        }

        protected override void OnDockChanged(EventArgs e) {
            base.OnDockChanged(e);

            if ( this.Dock != DockStyle.None )
                this.AutoSize = false;
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);

            switch ( e.KeyCode ) {
                case Keys.Left:
                    this.AdjustScroll(-(e.Modifiers == Keys.None ? this.HorizontalScroll.SmallChange : this.HorizontalScroll.LargeChange), 0);
                    break;
                case Keys.Right:
                    this.AdjustScroll(e.Modifiers == Keys.None ? this.HorizontalScroll.SmallChange : this.HorizontalScroll.LargeChange, 0);
                    break;
                case Keys.Up:
                    this.AdjustScroll(0, -(e.Modifiers == Keys.None ? this.VerticalScroll.SmallChange : this.VerticalScroll.LargeChange));
                    break;
                case Keys.Down:
                    this.AdjustScroll(0, e.Modifiers == Keys.None ? this.VerticalScroll.SmallChange : this.VerticalScroll.LargeChange);
                    break;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e) {
            if ( e.Button == MouseButtons.Right ) {
                if ( this.Image != null ) {
                    if ( this.Image.Width == 1280 )
                        this.Zoom = 100;
                    else
                        this.ZoomToFit();
                }
                this.Invalidate(true);
                this.Update();
            }

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseClick(MouseEventArgs e) {
            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            // convert e-position with respect to image scroll position
            Point cl = new Point();
            Rectangle src = this.GetSourceImageRegion();
            cl.X = e.X + src.X;
            cl.Y = e.Y + src.Y;

            base.OnMouseDown(e);

            if ( !this.Focused )
                this.Focus();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            // convert e-position with respect to image scroll position
            Point cl = new Point(0, 0);
            Rectangle src = this.GetSourceImageRegion();
            cl.X = e.X + src.X;
            cl.Y = e.Y + src.Y;

            // Bild mit Maus bewegen, aka "panning" 
            if ( e.Button == MouseButtons.Left && this.AutoPan && this.Image != null ) {
                if ( !this.IsPanning ) {
                    this._startMousePosition = e.Location;
                    this.IsPanning = true;
                }

                if ( this.IsPanning ) {
                    int x;
                    int y;
                    Point position;

                    if ( !this.InvertMouse ) {
                        x = -this._startScrollPosition.X + (this._startMousePosition.X - e.Location.X);
                        y = -this._startScrollPosition.Y + (this._startMousePosition.Y - e.Location.Y);
                    } else {
                        x = -(this._startScrollPosition.X + (this._startMousePosition.X - e.Location.X));
                        y = -(this._startScrollPosition.Y + (this._startMousePosition.Y - e.Location.Y));
                    }

                    position = new Point(x, y);

                    this.UpdateScrollPosition(position);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            base.OnMouseUp(e);

            if ( this.IsPanning )
                this.IsPanning = false;
        }

        protected override void OnPaddingChanged(System.EventArgs e) {
            base.OnPaddingChanged(e);
            this.AdjustLayout();
        }

        protected override void OnPaint(PaintEventArgs e) {
            Rectangle innerRectangle;

            // draw the borders
            switch ( this.BorderStyle ) {
                case BorderStyle.FixedSingle:
                    ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, this.ForeColor, ButtonBorderStyle.Solid);
                    break;
                case BorderStyle.Fixed3D:
                    ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, Border3DStyle.Sunken);
                    break;
            }

            innerRectangle = this.GetInsideViewPort();

            // draw the background
            using ( SolidBrush brush = new SolidBrush(this.BackColor) )
                e.Graphics.FillRectangle(brush, innerRectangle);

            if ( this._texture != null && this.GridDisplayMode != ImageBoxGridDisplayMode.None ) {
                switch ( this.GridDisplayMode ) {
                    case ImageBoxGridDisplayMode.Image:
                        Rectangle fillRectangle;

                        fillRectangle = this.GetImageViewPort();
                        e.Graphics.FillRectangle(this._texture, fillRectangle);

                        if ( !fillRectangle.Equals(innerRectangle) ) {
                            fillRectangle.Inflate(1, 1);
                            ControlPaint.DrawBorder(e.Graphics, fillRectangle, this.ForeColor, ButtonBorderStyle.Solid);
                        }
                        break;
                    case ImageBoxGridDisplayMode.Client:
                        e.Graphics.FillRectangle(this._texture, innerRectangle);
                        break;
                }
            }

            // draw the image
            if ( this.Image != null )
                this.DrawImage(e.Graphics);

            base.OnPaint(e);
        }

        protected override void OnParentChanged(System.EventArgs e) {
            base.OnParentChanged(e);
            this.AdjustLayout();
        }

        protected override void OnResize(EventArgs e) {
            this.AdjustLayout();

            base.OnResize(e);
        }

        protected override void OnScroll(ScrollEventArgs se) {
            if ( se.Type == ScrollEventType.ThumbTrack ) {
                if ( se.ScrollOrientation == ScrollOrientation.HorizontalScroll ) {
                    this.UpdateScrollPosition(new Point(se.NewValue, this.VerticalScroll.Value));
                }
                if ( se.ScrollOrientation == ScrollOrientation.VerticalScroll ) {
                    this.UpdateScrollPosition(new Point(this.HorizontalScroll.Value, se.NewValue));
                }
                return;
            }

            this.Invalidate();

            base.OnScroll(se);
        }

        // zoom change via mousewheel
        protected override void OnMouseWheel(MouseEventArgs e) {
            if ( !this.SizeToFit ) {
                int increment;

                if ( Control.ModifierKeys == Keys.None )
                    increment = this.ZoomIncrement;
                else
                    increment = this.ZoomIncrement * 5;

                if ( e.Delta < 0 )
                    increment = -increment;

                this.Zoom += increment;

            }
        }

        #endregion  Protected Overridden Methods  

        #region  Public Methods  

        // zoom change via faked mousewheel
        public void ZoomChange(int direction) {
            if ( this.Image != null ) {
                MouseEventArgs mea = new MouseEventArgs(MouseButtons.None, 0, 0, 0, direction);
                this.OnMouseWheel(mea);
            }
        }

        public virtual Rectangle GetImageViewPort() {
            Rectangle viewPort;

            if ( this.Image != null ) {
                Rectangle innerRectangle;
                Point offset;

                innerRectangle = this.GetInsideViewPort();

                if ( this.AutoCenter ) {
                    int x;
                    int y;

                    x = !this.HScroll ? (innerRectangle.Width - (this.ScaledImageWidth + this.Padding.Horizontal)) / 2 : 0;
                    y = !this.VScroll ? (innerRectangle.Height - (this.ScaledImageHeight + this.Padding.Vertical)) / 2 : 0;

                    offset = new Point(x, y);
                } else
                    offset = Point.Empty;

                viewPort = new Rectangle(offset.X + innerRectangle.Left + this.Padding.Left, offset.Y + innerRectangle.Top + this.Padding.Top, innerRectangle.Width - (this.Padding.Horizontal + (offset.X * 2)), innerRectangle.Height - (this.Padding.Vertical + (offset.Y * 2)));
            } else
                viewPort = Rectangle.Empty;

            return viewPort;
        }

        public Rectangle GetInsideViewPort() {
            return this.GetInsideViewPort(false);
        }

        public virtual Rectangle GetInsideViewPort(bool includePadding) {
            int left;
            int top;
            int width;
            int height;
            int borderOffset;

            borderOffset = this.GetBorderOffset();
            left = borderOffset;
            top = borderOffset;
            width = this.ClientSize.Width - (borderOffset * 2);
            height = this.ClientSize.Height - (borderOffset * 2);

            if ( includePadding ) {
                left += this.Padding.Left;
                top += this.Padding.Top;
                width -= this.Padding.Horizontal;
                height -= this.Padding.Vertical;
            }

            return new Rectangle(left, top, width, height);
        }

        public virtual Rectangle GetSourceImageRegion() {
            int sourceLeft;
            int sourceTop;
            int sourceWidth;
            int sourceHeight;
            Rectangle viewPort;
            Rectangle region;

            if ( this.Image != null ) {
                double zoom = this.ZoomFactor;
                if ( zoom == 0 ) {
                    zoom = 1;
                }

                viewPort = this.GetImageViewPort();
                sourceLeft = (int)(-this.AutoScrollPosition.X / zoom);
                sourceTop = (int)(-this.AutoScrollPosition.Y / zoom);
                sourceWidth = (int)(viewPort.Width / zoom);
                sourceHeight = (int)(viewPort.Height / zoom);

                region = new Rectangle(sourceLeft, sourceTop, sourceWidth, sourceHeight);
            } else
                region = Rectangle.Empty;

            return region;
        }

        public virtual void ZoomToFit() {
            if ( this.Image != null ) {
                Rectangle innerRectangle;
                double zoom;
                double aspectRatio;

                this.AutoScrollMinSize = Size.Empty;

                innerRectangle = this.GetInsideViewPort(true);

                if ( this.Image.Width > this.Image.Height ) {
                    aspectRatio = innerRectangle.Width / ((double)this.Image.Width);
                    zoom = aspectRatio * 100.0;

                    if ( innerRectangle.Height < ((this.Image.Height * zoom) / 100.0) ) {
                        aspectRatio = innerRectangle.Height / ((double)this.Image.Height);
                        zoom = aspectRatio * 100.0;
                    }
                } else {
                    aspectRatio = innerRectangle.Height / ((double)this.Image.Height);
                    zoom = aspectRatio * 100.0;

                    if ( innerRectangle.Width < ((this.Image.Width * zoom) / 100.0) ) {
                        aspectRatio = innerRectangle.Width / ((double)this.Image.Width);
                        zoom = aspectRatio * 100.0;
                    }
                }

                this.Zoom = (int)Math.Round(Math.Floor(zoom));
            }
        }

        #endregion  Public Methods  

        #region  Public Properties  

        [DefaultValue(true), Category("Appearance")]
        public bool AutoCenter {
            get { return this._autoCenter; }
            set {
                if ( this._autoCenter != value ) {
                    this._autoCenter = value;
                    this.OnAutoCenterChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(true), Category("Behavior")]
        public bool AutoPan {
            get { return this._autoPan; }
            set {
                if ( this._autoPan != value ) {
                    this._autoPan = value;
                    this.OnAutoPanChanged(EventArgs.Empty);

                    if ( value )
                        this.SizeToFit = false;
                }
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size AutoScrollMinSize {
            get { return base.AutoScrollMinSize; }
            set { base.AutoScrollMinSize = value; }
        }

        [Category("Appearance"), DefaultValue(typeof(BorderStyle), "FixedSingle")]
        public BorderStyle BorderStyle {
            get { return this._borderStyle; }
            set {
                if ( this._borderStyle != value ) {
                    this._borderStyle = value;
                    this.OnBorderStyleChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance"), DefaultValue(8)]
        public int GridCellSize {
            get { return this._gridCellSize; }
            set {
                if ( this._gridCellSize != value ) {
                    this._gridCellSize = value;
                    this.OnGridCellSizeChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance"), DefaultValue(typeof(Color), "Gainsboro")]
        public Color GridColor {
            get { return this._gridColor; }
            set {
                if ( this._gridColor != value ) {
                    this._gridColor = value;
                    this.OnGridColorChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance"), DefaultValue(typeof(Color), "White")]
        public Color GridColorAlternate {
            get { return this._gridColorAlternate; }
            set {
                if ( this._gridColorAlternate != value ) {
                    this._gridColorAlternate = value;
                    this.OnGridColorAlternateChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(ImageBoxGridDisplayMode.Client), Category("Appearance")]
        public ImageBoxGridDisplayMode GridDisplayMode {
            get { return this._gridDisplayMode; }
            set {
                if ( this._gridDisplayMode != value ) {
                    this._gridDisplayMode = value;
                    this.OnGridDisplayModeChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(typeof(ImageBoxGridScale), "Small"), Category("Appearance")]
        public ImageBoxGridScale GridScale {
            get { return this._gridScale; }
            set {
                if ( this._gridScale != value ) {
                    this._gridScale = value;
                    this.OnGridScaleChanged(EventArgs.Empty);
                }
            }
        }

        [Category("Appearance"), DefaultValue(null)]
        public virtual Image Image {
            get { return this._image; }
            set {
                // 20161016: img rotation/turn 
                //        if (_image != value)
                //        {
                this._image = value;
                this.OnImageChanged(EventArgs.Empty);
                //        }
            }
        }

        [DefaultValue(InterpolationMode.Default), Category("Appearance")]
        public InterpolationMode InterpolationMode {
            get { return this._interpolationMode; }
            set {
                if ( value == InterpolationMode.Invalid )
                    value = InterpolationMode.NearestNeighbor; //.Default;

                if ( this._interpolationMode != value ) {
                    this._interpolationMode = value;
                    this.OnInterpolationModeChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), Category("Behavior")]
        public bool InvertMouse {
            get { return this._invertMouse; }
            set {
                if ( this._invertMouse != value ) {
                    this._invertMouse = value;
                    this.OnInvertMouseChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public bool IsPanning {
            get { return this._isPanning; }
            protected set {
                if ( this._isPanning != value ) {
                    this._isPanning = value;
                    this._startScrollPosition = this.AutoScrollPosition;

                    if ( value ) {
                        this.Cursor = Cursors.SizeAll;
                        this.OnPanStart(EventArgs.Empty);
                    } else {
                        this.Cursor = Cursors.Default;
                        this.OnPanEnd(EventArgs.Empty);
                    }
                }
            }
        }

        [DefaultValue(false), Category("Appearance")]
        public bool SizeToFit {
            get { return this._sizeToFit; }
            set {
                if ( this._sizeToFit != value ) {
                    this._sizeToFit = value;
                    this.OnSizeToFitChanged(EventArgs.Empty);

                    if ( value )
                        this.AutoPan = false;
                }
            }
        }

        [DefaultValue(100), Category("Appearance")]
        public int Zoom {
            get { return this._zoom; }
            set {
                if ( value < ImageBox.MinZoom )
                    value = ImageBox.MinZoom;
                else if ( value > ImageBox.MaxZoom )
                    value = ImageBox.MaxZoom;

                if ( this._zoom != value ) {
                    this._zoom = value;
                    this.OnZoomChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(20), Category("Behavior")]
        public int ZoomIncrement {
            get { return this._zoomIncrement; }
            set {
                if ( this._zoomIncrement != value ) {
                    this._zoomIncrement = value;
                    this.OnZoomIncrementChanged(EventArgs.Empty);
                }
            }
        }

        #endregion  Public Properties  

        #region  Private Methods  

        private int GetBorderOffset() {
            int offset;

            switch ( this.BorderStyle ) {
                case BorderStyle.Fixed3D:
                    offset = 2;
                    break;
                case BorderStyle.FixedSingle:
                    offset = 1;
                    break;
                default:
                    offset = 0;
                    break;
            }

            return offset;
        }

        private void InitializeGridTile() {
            if ( this._texture != null )
                this._texture.Dispose();

            if ( this._gridTile != null )
                this._gridTile.Dispose();

            if ( this.GridDisplayMode != ImageBoxGridDisplayMode.None && this.GridCellSize != 0 ) {
                this._gridTile = this.CreateGridTileImage(this.GridCellSize, this.GridColor, this.GridColorAlternate);
                this._texture = new TextureBrush(this._gridTile);
            }

            this.Invalidate();
        }

        #endregion  Private Methods  

        #region  Protected Properties  

        protected virtual int ScaledImageHeight { get { return this.Image != null ? (int)(this.Image.Size.Height * this.ZoomFactor) : 0; } }

        protected virtual int ScaledImageWidth { get { return this.Image != null ? (int)(this.Image.Size.Width * this.ZoomFactor) : 0; } }

        protected virtual double ZoomFactor { get { return (double)this.Zoom / 100; } }

        #endregion  Protected Properties  

        #region  Protected Methods  

        protected virtual void AdjustLayout() {
            if ( this.AutoSize )
                this.AdjustSize();
            else if ( this.SizeToFit )
                this.ZoomToFit();
            else if ( this.AutoScroll )
                this.AdjustViewPort();
            this.Invalidate();
        }

        protected virtual void AdjustScroll(int x, int y) {
            Point scrollPosition;

            scrollPosition = new Point(this.HorizontalScroll.Value + x, this.VerticalScroll.Value + y);

            this.UpdateScrollPosition(scrollPosition);
        }

        protected virtual void AdjustSize() {
            if ( this.AutoSize && this.Dock == DockStyle.None )
                base.Size = base.PreferredSize;
        }

        protected virtual void AdjustViewPort() {
            if ( this.AutoScroll && this.Image != null )
                this.AutoScrollMinSize = new Size(this.ScaledImageWidth + this.Padding.Horizontal, this.ScaledImageHeight + this.Padding.Vertical);
        }

        protected virtual Bitmap CreateGridTileImage(int cellSize, Color firstColor, Color secondColor) {
            Bitmap result;
            int width;
            int height;
            float scale;

            // rescale the cell size
            switch ( this.GridScale ) {
                case ImageBoxGridScale.Medium:
                    scale = 1.5F;
                    break;
                case ImageBoxGridScale.Large:
                    scale = 2;
                    break;
                default:
                    scale = 1;
                    break;
            }

            cellSize = (int)(cellSize * scale);

            // draw the tile
            width = cellSize * 2;
            height = cellSize * 2;
            result = new Bitmap(width, height);
            using ( Graphics g = Graphics.FromImage(result) ) {
                using ( SolidBrush brush = new SolidBrush(firstColor) )
                    g.FillRectangle(brush, new Rectangle(0, 0, width, height));

                using ( SolidBrush brush = new SolidBrush(secondColor) ) {
                    g.FillRectangle(brush, new Rectangle(0, 0, cellSize, cellSize));
                    g.FillRectangle(brush, new Rectangle(cellSize, cellSize, cellSize, cellSize));
                }
            }

            return result;
        }

        protected virtual void DrawImage(Graphics g) {
            g.InterpolationMode = InterpolationMode.NearestNeighbor; //this.InterpolationMode;
            g.DrawImage(this.Image, this.GetImageViewPort(), this.GetSourceImageRegion(), GraphicsUnit.Pixel);
        }

        protected virtual void OnAutoCenterChanged(EventArgs e) {
            this.Invalidate();

            if ( AutoCenterChanged != null )
                AutoCenterChanged(this, e);
        }

        protected virtual void OnAutoPanChanged(EventArgs e) {
            if ( AutoPanChanged != null )
                AutoPanChanged(this, e);
        }

        protected virtual void OnBorderStyleChanged(EventArgs e) {
            this.AdjustLayout();

            if ( BorderStyleChanged != null )
                BorderStyleChanged(this, e);
        }

        protected virtual void OnGridCellSizeChanged(EventArgs e) {
            this.InitializeGridTile();

            if ( GridCellSizeChanged != null )
                GridCellSizeChanged(this, e);
        }

        protected virtual void OnGridColorAlternateChanged(EventArgs e) {
            this.InitializeGridTile();

            if ( GridColorAlternateChanged != null )
                GridColorAlternateChanged(this, e);
        }

        protected virtual void OnGridColorChanged(EventArgs e) {
            this.InitializeGridTile();

            if ( GridColorChanged != null )
                GridColorChanged(this, e);

        }

        protected virtual void OnGridDisplayModeChanged(EventArgs e) {
            this.InitializeGridTile();
            this.Invalidate();

            if ( GridDisplayModeChanged != null )
                GridDisplayModeChanged(this, e);
        }

        protected virtual void OnGridScaleChanged(EventArgs e) {
            this.InitializeGridTile();

            if ( GridScaleChanged != null )
                GridScaleChanged(this, e);
        }

        protected virtual void OnImageChanged(EventArgs e) {
            this.AdjustLayout();

            if ( ImageChanged != null )
                ImageChanged(this, e);
        }

        protected virtual void OnInterpolationModeChanged(EventArgs e) {
            this.Invalidate();

            if ( InterpolationModeChanged != null )
                InterpolationModeChanged(this, e);
        }

        protected virtual void OnInvertMouseChanged(EventArgs e) {
            if ( InvertMouseChanged != null )
                InvertMouseChanged(this, e);
        }

        protected virtual void OnPanEnd(EventArgs e) {
            if ( PanEnd != null )
                PanEnd(this, e);
        }

        protected virtual void OnPanStart(EventArgs e) {
            if ( PanStart != null )
                PanStart(this, e);
        }

        protected virtual void OnSizeToFitChanged(EventArgs e) {
            this.AdjustLayout();

            if ( SizeToFitChanged != null )
                SizeToFitChanged(this, e);
        }

        protected virtual void OnZoomChanged(EventArgs e) {
            this.AdjustLayout();

            if ( ZoomChanged != null )
                ZoomChanged(this, e);
        }

        protected virtual void OnZoomIncrementChanged(EventArgs e) {
            if ( ZoomIncrementChanged != null )
                ZoomIncrementChanged(this, e);
        }

        protected virtual void UpdateScrollPosition(Point position) {
            this.AutoScrollPosition = position;
            this.Invalidate();
            this.OnScroll(new ScrollEventArgs(ScrollEventType.ThumbPosition, 0));
        }

        #endregion  Protected Methods  

    }
}
