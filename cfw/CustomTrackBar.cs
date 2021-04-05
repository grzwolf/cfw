#region Copyright (c) 2002-2006 X-Component, All Rights Reserved
/* ---------------------------------------------------------------------*
*                           X-Component,                              *
*              Copyright (c) 2002-2006 All Rights reserved              *
*                                                                       *
*                                                                       *
* This file and its contents are protected by Vietnam and               *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF ECONTECH JSC.,     *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ECONTECH JSC. PRODUCT.   *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF ECONTECH JSC.,     *
* THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO             *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2002-2006 X-Component, All Rights Reserved

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SliderBar {

    #region Declaration

    /// <summary>
    /// Represents the method that will handle a change in value.
    /// </summary>
    public delegate void ValueChangedHandler(object sender, decimal value);

    public enum CustomBorderStyle {
        /// <summary>
        /// No border.
        /// </summary>
        None,
        /// <summary>
        /// A dashed border.
        /// </summary>
        Dashed, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A dotted-line border.
        /// </summary>
        Dotted, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A sunken border.
        /// </summary>
        Inset, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A raised border.
        /// </summary>
        Outset, //from ButtonBorderStyle Enumeration
        /// <summary>
        /// A solid border.
        /// </summary>
        Solid, //from ButtonBorderStyle Enumeration

        /// <summary>
        /// The border is drawn outside the specified rectangle, preserving the dimensions of the rectangle for drawing.
        /// </summary>
        Adjust, //from Border3DStyle Enumeration
        /// <summary>
        /// The inner and outer edges of the border have a raised appearance.
        /// </summary>
        Bump, //from Border3DStyle Enumeration
        /// <summary>
        /// The inner and outer edges of the border have an etched appearance.
        /// </summary>
        Etched, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has no three-dimensional effects.
        /// </summary>
        Flat, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has raised inner and outer edges.
        /// </summary>
        Raised, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a raised inner edge and no outer edge.
        /// </summary>
        RaisedInner, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a raised outer edge and no inner edge.
        /// </summary>
        RaisedOuter, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has sunken inner and outer edges.
        /// </summary>
        Sunken, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a sunken inner edge and no outer edge.
        /// </summary>
        SunkenInner, //from Border3DStyle Enumeration
        /// <summary>
        /// The border has a sunken outer edge and no inner edge.
        /// </summary>
        SunkenOuter //from Border3DStyle Enumeration
    }

    #endregion

    /// <summary>
    ///	  <para>
    ///   <para>
    ///   CustomTrackBar supports the following features:
    ///   <list type="bullet">
    ///     <item><c>MAC style, Office2003 style, IDE2003 style and Plain style.</c></item>
    ///     <item><c>Vertical and Horizontal trackbar.</c></item>
    ///     <item><c>Supports many Text Tick styles: None, TopLeft, BottomRight, Both. You can change Text Font, ForeColor.</c></item> 
    ///     <item><c>Supports many Tick styles: None, TopLeft, BottomRight, Both.</c></item> 
    ///     <item><c>You can change <see cref="CustomTrackBar.TickColor"/>, <see cref="CustomTrackBar.TickFrequency"/>, <see cref="CustomTrackBar.TickHeight"/>.</c></item> 
    ///     <item><c>You can change <see cref="CustomTrackBar.TrackerColor"/> and <see cref="CustomTrackBar.TrackerSize"/>.</c></item> 
    ///     <item><c>You can change <see cref="CustomTrackBar.TrackLineColor"/> and <see cref="CustomTrackBar.TrackLineHeight"/>.</c></item> 	
    ///     <item><c>Easy to Use and Integrate in Visual Studio .NET.</c></item> 
    ///     <item><c>100% compatible to the standard control in VS.NET.</c></item> 
    ///     <item><c>100% managed code.</c></item> 
    ///     <item><c>No coding RAD component.</c></item> 
    ///   </list>
    ///   </para>
    /// </summary>
    [Description("CustomTrackBar is an advanced track bar.")]
    [Designer(typeof(CustomTrackBarDesigner))]
    [DefaultProperty("Maximum")]
    [DefaultEvent("ValueChanged")]
    public class CustomTrackBar : System.Windows.Forms.Control {

        #region Private Members

        // Instance fields
        private int _value = 0;
        private int _minimum = 0;
        private int _maximum = 10;

        private int _largeChange = 2;
        private int _smallChange = 1;

        private Orientation _orientation = Orientation.Horizontal;

        private CustomBorderStyle _borderStyle = CustomBorderStyle.None;
        private Color _borderColor = SystemColors.ActiveBorder;

        private Size _trackerSize = new Size(10, 20);
        private int _indentWidth = 6;
        private int _indentHeight = 6;

        private int _tickHeight = 2;
        private int _tickFrequency = 1;
        private Color _tickColor = Color.Black;
        private TickStyle _tickStyle = TickStyle.BottomRight;
        private TickStyle _textTickStyle = TickStyle.BottomRight;

        private int _trackLineHeight = 3;
        private Color _trackLineColor = SystemColors.Control;

        private Color _trackerColor = SystemColors.Control;
        public RectangleF _trackerRect = RectangleF.Empty;

        private bool _autoSize = true;

        private bool leftButtonDown = false;
        private float mouseStartPos = -1;

        /// <summary>
        /// Occurs when the property Value has been changed.
        /// </summary>
        public event ValueChangedHandler ValueChanged;
        /// <summary>
        /// Occurs when either a mouse or keyboard action moves the slider.
        /// </summary>
        public event EventHandler Scroll;

        #endregion

        #region Public Contruction

        /// <summary>
        /// Constructor method of <see cref="CustomTrackBar"/> class
        /// </summary>
        public CustomTrackBar() {
            base.MouseDown += new MouseEventHandler(this.OnMouseDownSlider);
            base.MouseUp += new MouseEventHandler(this.OnMouseUpSlider);
            base.MouseMove += new MouseEventHandler(this.OnMouseMoveSlider);

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true);

            this.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.ForeColor = Color.FromArgb(123, 125, 123);
            this.BackColor = Color.Transparent;

            this._tickColor = Color.FromArgb(148, 146, 148);
            this._tickHeight = 4;

            this._trackerColor = Color.FromArgb(24, 130, 198);
            this._trackerSize = new Size(16, 16);
            this._indentWidth = 6;
            this._indentHeight = 6;

            this._trackLineColor = Color.FromArgb(90, 93, 90);
            this._trackLineHeight = 3;

            this._borderStyle = CustomBorderStyle.None;
            this._borderColor = SystemColors.ActiveBorder;

            this._autoSize = true;
            this.Height = this.FitSize.Height;
        }

        #endregion

        #region Public Properties

        protected override void OnSizeChanged(EventArgs e) {
            base.OnSizeChanged(e);
            if ( this._autoSize ) {
                // Calculate the Position for children controls
                if ( this._orientation == Orientation.Horizontal ) {
                    this.Height = this.FitSize.Height;
                } else {
                    this.Width = this.FitSize.Width;
                }
                //=================================================
            }
        }


        /// <summary>
        /// Gets or sets a value indicating whether the height or width of the track bar is being automatically sized.
        /// </summary>
        /// <remarks>You can set the AutoSize property to true to cause the track bar to adjust either its height or width, depending on orientation, to ensure that the control uses only the required amount of space.</remarks>
        /// <value>true if the track bar is being automatically sized; otherwise, false. The default is true.</value>
        [Category("Behavior")]
        [Description("Gets or sets the height of track line.")]
        [DefaultValue(true)]
        public bool AutoSize {
            get { return this._autoSize; }

            set {
                if ( this._autoSize != value ) {
                    this._autoSize = value;
                    if ( this._autoSize == true )
                        this.Size = this.FitSize;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the <see cref="Value"/> property when the slider is moved a large distance.
        /// </summary>
        /// <remarks>
        /// When the user presses the PAGE UP or PAGE DOWN key or clicks the track bar on either side of the slider, the <see cref="Value"/> 
        /// property changes according to the value set in the <see cref="LargeChange"/> property. 
        /// You might consider setting the <see cref="LargeChange"/> value to a percentage of the <see cref="Control.Height"/> (for a vertically oriented track bar) or 
        /// <see cref="Control.Width"/> (for a horizontally oriented track bar) values. This keeps the distance your track bar moves proportionate to its size.
        /// </remarks>
        /// <value>A numeric value. The default value is 2.</value>
        [Category("Behavior")]
        [Description("Gets or sets a value to be added to or subtracted from the Value property when the slider is moved a large distance.")]
        [DefaultValue(2)]
        public int LargeChange {
            get { return this._largeChange; }

            set {
                this._largeChange = value;
                if ( this._largeChange < 1 )
                    this._largeChange = 1;
            }
        }

        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the <see cref="Value"/> property when the slider is moved a small distance.
        /// </summary>
        /// <remarks>
        /// When the user presses one of the arrow keys, the <see cref="Value"/> property changes according to the value set in the SmallChange property.
        /// You might consider setting the <see cref="SmallChange"/> value to a percentage of the <see cref="Control.Height"/> (for a vertically oriented track bar) or 
        /// <see cref="Control.Width"/> (for a horizontally oriented track bar) values. This keeps the distance your track bar moves proportionate to its size.
        /// </remarks>
        /// <value>A numeric value. The default value is 1.</value>
        [Category("Behavior")]
        [Description("Gets or sets a value to be added to or subtracted from the Value property when the slider is moved a small distance.")]
        [DefaultValue(1)]
        public int SmallChange {
            get { return this._smallChange; }

            set {
                this._smallChange = value;
                if ( this._smallChange < 1 )
                    this._smallChange = 1;
            }
        }

        /// <summary>
        /// Gets or sets the height of track line.
        /// </summary>
        /// <value>The default value is 4.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of track line.")]
        [DefaultValue(4)]
        public int TrackLineHeight {
            get { return this._trackLineHeight; }

            set {
                if ( this._trackLineHeight != value ) {
                    this._trackLineHeight = value;
                    if ( this._trackLineHeight < 1 )
                        this._trackLineHeight = 1;

                    if ( this._trackLineHeight > this._trackerSize.Height )
                        this._trackLineHeight = this._trackerSize.Height;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tick's <see cref="Color"/> of the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets or sets the tick's color of the control.")]
        public Color TickColor {
            get { return this._tickColor; }

            set {
                if ( this._tickColor != value ) {
                    this._tickColor = value;
                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets a value that specifies the delta between ticks drawn on the control.
        /// </summary>
        /// <remarks>
        /// For a <see cref="CustomTrackBar"/> with a large range of values between the <see cref="Minimum"/> and the 
        /// <see cref="Maximum"/>, it might be impractical to draw all the ticks for values on the control. 
        /// For example, if you have a control with a range of 100, passing in a value of 
        /// five here causes the control to draw 20 ticks. In this case, each tick 
        /// represents five units in the range of values.
        /// </remarks>
        /// <value>The numeric value representing the delta between ticks. The default is 1.</value>
        [Category("Appearance")]
        [Description("Gets or sets a value that specifies the delta between ticks drawn on the control.")]
        [DefaultValue(1)]
        public int TickFrequency {
            get { return this._tickFrequency; }

            set {
                if ( this._tickFrequency != value ) {
                    this._tickFrequency = value;
                    if ( this._tickFrequency < 1 )
                        this._tickFrequency = 1;
                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the height of tick.
        /// </summary>
        /// <value>The height of tick in pixels. The default value is 2.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of tick.")]
        [DefaultValue(6)]
        public int TickHeight {
            get { return this._tickHeight; }

            set {
                if ( this._tickHeight != value ) {
                    this._tickHeight = value;

                    if ( this._tickHeight < 1 )
                        this._tickHeight = 1;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the height of indent (or Padding-Y).
        /// </summary>
        /// <value>The height of indent in pixels. The default value is 6.</value>
        [Category("Appearance")]
        [Description("Gets or sets the height of indent.")]
        [DefaultValue(2)]
        public int IndentHeight {
            get { return this._indentHeight; }

            set {
                if ( this._indentHeight != value ) {
                    this._indentHeight = value;
                    if ( this._indentHeight < 0 )
                        this._indentHeight = 0;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the width of indent (or Padding-Y).
        /// </summary>
        /// <value>The width of indent in pixels. The default value is 6.</value>
        [Category("Appearance")]
        [Description("Gets or sets the width of indent.")]
        [DefaultValue(6)]
        public int IndentWidth {
            get { return this._indentWidth; }

            set {
                if ( this._indentWidth != value ) {
                    this._indentWidth = value;
                    if ( this._indentWidth < 0 )
                        this._indentWidth = 0;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tracker's size. 
        /// The tracker's width must be greater or equal to tracker's height.
        /// </summary>
        /// <value>The <see cref="Size"/> object that represents the height and width of the tracker in pixels.</value>
        [Category("Appearance")]
        [Description("Gets or sets the tracker's size.")]
        public Size TrackerSize {
            get { return this._trackerSize; }

            set {
                if ( this._trackerSize != value ) {
                    this._trackerSize = value;
                    if ( this._trackerSize.Width > this._trackerSize.Height )
                        this._trackerSize.Height = this._trackerSize.Width;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the text tick style of the trackbar.
        /// There are 4 styles for selection: None, TopLeft, BottomRight, Both. 
        /// </summary>
        /// <remarks>You can use the <see cref="Control.Font"/>, <see cref="Control.ForeColor"/>
        /// properties to customize the tick text.</remarks>
        /// <value>One of the <see cref="TickStyle"/> values. The default is <b>BottomRight</b>.</value>
        [Category("Appearance")]
        [Description("Gets or sets the text tick style.")]
        [DefaultValue(TickStyle.BottomRight)]
        public TickStyle TextTickStyle {
            get { return this._textTickStyle; }

            set {
                if ( this._textTickStyle != value ) {
                    this._textTickStyle = value;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tick style of the trackbar.
        /// There are 4 styles for selection: None, TopLeft, BottomRight, Both. 
        /// </summary>
        /// <remarks>You can use the <see cref="TickColor"/>, <see cref="TickFrequency"/>, 
        /// <see cref="TickHeight"/> properties to customize the trackbar's ticks.</remarks>
        /// <value>One of the <see cref="TickStyle"/> values. The default is <b>BottomRight</b>.</value>
        [Category("Appearance")]
        [Description("Gets or sets the tick style.")]
        [DefaultValue(TickStyle.BottomRight)]
        public TickStyle TickStyle {
            get { return this._tickStyle; }

            set {
                if ( this._tickStyle != value ) {
                    this._tickStyle = value;

                    if ( this._autoSize == true )
                        this.Size = this.FitSize;

                    this.Invalidate();
                }

            }
        }

        /// <summary>
        /// Gets or set tracker's color.
        /// </summary>
        /// <value>
        /// <remarks>You can change size of tracker by <see cref="TrackerSize"/> property.</remarks>
        /// A <see cref="Color"/> that represents the color of the tracker. 
        /// </value>
        [Description("Gets or set tracker's color.")]
        [Category("Appearance")]
        public Color TrackerColor {
            get {
                return this._trackerColor;
            }
            set {
                if ( this._trackerColor != value ) {
                    this._trackerColor = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a numeric value that represents the current position of the slider on the track bar.
        /// </summary>
        /// <remarks>The Value property contains the number that represents the current position of the slider on the track bar.</remarks>
        /// <value>A numeric value that is within the <see cref="Minimum"/> and <see cref="Maximum"/> range. 
        /// The default value is 0.</value>
        [Description("The current value for the CustomTrackBar, in the range specified by the Minimum and Maximum properties.")]
        [Category("Behavior")]
        public int Value {
            get {
                return this._value;
            }
            set {
                if ( this._value != value ) {
                    if ( value < this._minimum )
                        this._value = this._minimum;
                    else
                        if ( value > this._maximum )
                        this._value = this._maximum;
                    else
                        this._value = value;

                    this.OnValueChanged(this._value);

                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the lower limit of the range this <see cref="CustomTrackBar"/> is working with.
        /// </summary>
        /// <remarks>You can use the <see cref="SetRange"/> method to set both the <see cref="Maximum"/> and <see cref="Minimum"/> properties at the same time.</remarks>
        /// <value>The minimum value for the <see cref="CustomTrackBar"/>. The default value is 0.</value>
        [Description("The lower bound of the range this CustomTrackBar is working with.")]
        [Category("Behavior")]
        public int Minimum {
            get {
                return this._minimum;
            }
            set {
                this._minimum = value;

                if ( this._minimum > this._maximum )
                    this._maximum = this._minimum;
                if ( this._minimum > this._value )
                    this._value = this._minimum;

                if ( this._autoSize == true )
                    this.Size = this.FitSize;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the upper limit of the range this <see cref="CustomTrackBar"/> is working with.
        /// </summary>
        /// <remarks>You can use the <see cref="SetRange"/> method to set both the <see cref="Maximum"/> and <see cref="Minimum"/> properties at the same time.</remarks>
        /// <value>The maximum value for the <see cref="CustomTrackBar"/>. The default value is 10.</value>
        [Description("The uppper bound of the range this CustomTrackBar is working with.")]
        [Category("Behavior")]
        public int Maximum {
            get {
                return this._maximum;
            }
            set {
                this._maximum = value;

                if ( this._maximum < this._value )
                    this._value = this._maximum;
                if ( this._maximum < this._minimum )
                    this._minimum = this._maximum;

                if ( this._autoSize == true )
                    this.Size = this.FitSize;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the horizontal or vertical orientation of the track bar.
        /// </summary>
        /// <remarks>
        /// When the <b>Orientation</b> property is set to <b>Orientation.Horizontal</b>, 
        /// the slider moves from left to right as the <see cref="Value"/> increases. 
        /// When the <b>Orientation</b> property is set to <b>Orientation.Vertical</b>, the slider moves 
        /// from bottom to top as the <see cref="Value"/> increases.
        /// </remarks>
        /// <value>One of the <see cref="Orientation"/> values. The default value is <b>Horizontal</b>.</value>
        [Description("Gets or sets a value indicating the horizontal or vertical orientation of the track bar.")]
        [Category("Behavior")]
        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation {
            get {
                return this._orientation;
            }
            set {
                if ( value != this._orientation ) {
                    this._orientation = value;
                    if ( this._orientation == Orientation.Horizontal ) {
                        if ( this.Width < this.Height ) {
                            int temp = this.Width;
                            this.Width = this.Height;
                            this.Height = temp;
                        }
                    } else //Vertical 
                      {
                        if ( this.Width > this.Height ) {
                            int temp = this.Width;
                            this.Width = this.Height;
                            this.Height = temp;
                        }
                    }
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border type of the trackbar control.
        /// </summary>
        /// <value>A <see cref="CustomBorderStyle"/> that represents the border type of the trackbar control. 
        /// The default is <b>CustomBorderStyle.None</b>.</value>
        [Description("Gets or sets the border type of the trackbar control.")]
        [Category("Appearance"), DefaultValue(typeof(CustomBorderStyle), "None")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public CustomBorderStyle BorderStyle {
            get {
                return this._borderStyle;
            }
            set {
                if ( this._borderStyle != value ) {
                    this._borderStyle = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the border color of the control.
        /// </summary>
        /// <value>A <see cref="Color"/> object that represents the border color of the control.</value>
        [Category("Appearance")]
        [Description("Gets or sets the border color of the control.")]
        public Color BorderColor {
            get { return this._borderColor; }
            set {
                if ( value != this._borderColor ) {
                    this._borderColor = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the track line.
        /// </summary>
        /// <value>A <see cref="Color"/> object that represents the color of the track line.</value>
        [Category("Appearance")]
        [Description("Gets or sets the color of the track line.")]
        public Color TrackLineColor {
            get { return this._trackLineColor; }
            set {
                if ( value != this._trackLineColor ) {
                    this._trackLineColor = value;
                    this.Invalidate();
                }
            }
        }


        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the Size of area need for drawing.
        /// </summary>
        [Description("Gets the Size of area need for drawing.")]
        [Browsable(false)]
        private Size FitSize {
            get {
                Size fitSize;
                float textAreaSize;

                // Create a Graphics object for the Control.
                Graphics g = this.CreateGraphics();

                Rectangle workingRect = Rectangle.Inflate(this.ClientRectangle, -this._indentWidth, -this._indentHeight);
                float currentUsedPos = 0;

                if ( this._orientation == Orientation.Horizontal ) {
                    currentUsedPos = this._indentHeight;
                    //==========================================================================

                    // Get Height of Text Area
                    textAreaSize = g.MeasureString(this._maximum.ToString(), this.Font).Height;

                    if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both )
                        currentUsedPos += this._tickHeight + 1;

                    currentUsedPos += this._trackerSize.Height;

                    if ( this._tickStyle == TickStyle.BottomRight || this._tickStyle == TickStyle.Both ) {
                        currentUsedPos += 1;
                        currentUsedPos += this._tickHeight;
                    }

                    if ( this._textTickStyle == TickStyle.BottomRight || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    currentUsedPos += this._indentHeight;

                    fitSize = new Size(this.ClientRectangle.Width, (int)currentUsedPos);
                } else //_orientation == Orientation.Vertical
                  {
                    currentUsedPos = this._indentWidth;
                    //==========================================================================

                    // Get Width of Text Area
                    textAreaSize = g.MeasureString(this._maximum.ToString(), this.Font).Width;

                    if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both )
                        currentUsedPos += this._tickHeight + 1;

                    currentUsedPos += this._trackerSize.Height;

                    if ( this._tickStyle == TickStyle.BottomRight || this._tickStyle == TickStyle.Both ) {
                        currentUsedPos += 1;
                        currentUsedPos += this._tickHeight;
                    }

                    if ( this._textTickStyle == TickStyle.BottomRight || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    currentUsedPos += this._indentWidth;

                    fitSize = new Size((int)currentUsedPos, this.ClientRectangle.Height);

                }

                // Clean up the Graphics object.
                g.Dispose();

                return fitSize;
            }
        }


        /// <summary>
        /// Gets the rectangle containing the tracker.
        /// </summary>
        [Description("Gets the rectangle containing the tracker.")]
        private RectangleF TrackerRect {
            get {
                RectangleF trackerRect;
                float textAreaSize;

                // Create a Graphics object for the Control.
                Graphics g = this.CreateGraphics();

                Rectangle workingRect = Rectangle.Inflate(this.ClientRectangle, -this._indentWidth, -this._indentHeight);
                float currentUsedPos = 0;

                if ( this._orientation == Orientation.Horizontal ) {
                    currentUsedPos = this._indentHeight;
                    //==========================================================================

                    // Get Height of Text Area
                    textAreaSize = g.MeasureString(this._maximum.ToString(), this.Font).Height;

                    if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both )
                        currentUsedPos += this._tickHeight + 1;


                    //==========================================================================
                    // Caculate the Tracker's rectangle
                    //==========================================================================
                    float currentTrackerPos;
                    if ( this._maximum == this._minimum )
                        currentTrackerPos = workingRect.Left;
                    else
                        currentTrackerPos = (workingRect.Width - this._trackerSize.Width) * (this._value - this._minimum) / (this._maximum - this._minimum) + workingRect.Left;
                    trackerRect = new RectangleF(currentTrackerPos, currentUsedPos, this._trackerSize.Width, this._trackerSize.Height);// Remember this for drawing the Tracker later
                    trackerRect.Inflate(0, -1);
                } else //_orientation == Orientation.Vertical
                  {
                    currentUsedPos = this._indentWidth;
                    //==========================================================================

                    // Get Width of Text Area
                    textAreaSize = g.MeasureString(this._maximum.ToString(), this.Font).Width;

                    if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both )
                        currentUsedPos += textAreaSize;

                    if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both )
                        currentUsedPos += this._tickHeight + 1;

                    //==========================================================================
                    // Caculate the Tracker's rectangle
                    //==========================================================================
                    float currentTrackerPos;
                    if ( this._maximum == this._minimum )
                        currentTrackerPos = workingRect.Top;
                    else
                        currentTrackerPos = (workingRect.Height - this._trackerSize.Width) * (this._value - this._minimum) / (this._maximum - this._minimum);

                    trackerRect = new RectangleF(currentUsedPos, workingRect.Bottom - currentTrackerPos - this._trackerSize.Width, this._trackerSize.Height, this._trackerSize.Width);// Remember this for drawing the Tracker later
                    trackerRect.Inflate(-1, 0);


                }

                // Clean up the Graphics object.
                g.Dispose();

                return trackerRect;
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Raises the ValueChanged event.
        /// </summary>
        /// <param name="value">The new value</param>
        public virtual void OnValueChanged(int value) {
            // Any attached event handlers?
            if ( ValueChanged != null )
                ValueChanged(this, value);

        }

        /// <summary>
        /// Raises the Scroll event.
        /// </summary>
        public virtual void OnScroll() {
            try {
                // Any attached event handlers?
                if ( Scroll != null )
                    Scroll(this, new System.EventArgs());
            } catch ( Exception Err ) {
                MessageBox.Show("OnScroll Exception: " + Err.Message);
            }

        }


        /// <summary>
        /// Call the Increment() method to increase the value displayed by an integer you specify 
        /// </summary>
        /// <param name="value"></param>
        public void Increment(int value) {
            if ( this._value < this._maximum ) {
                this._value += value;
                if ( this._value > this._maximum )
                    this._value = this._maximum;
            } else
                this._value = this._maximum;

            this.OnValueChanged(this._value);
            this.Invalidate();
        }

        /// <summary>
        /// Call the Decrement() method to decrease the value displayed by an integer you specify 
        /// </summary>
        /// <param name="value"> The value to decrement</param>
        public void Decrement(int value) {
            if ( this._value > this._minimum ) {
                this._value -= value;
                if ( this._value < this._minimum )
                    this._value = this._minimum;
            } else
                this._value = this._minimum;

            this.OnValueChanged(this._value);
            this.Invalidate();
        }

        /// <summary>
        /// Sets the minimum and maximum values for a TrackBar.
        /// </summary>
        /// <param name="minValue">The lower limit of the range of the track bar.</param>
        /// <param name="maxValue">The upper limit of the range of the track bar.</param>
        public void SetRange(int minValue, int maxValue) {
            this._minimum = minValue;

            if ( this._minimum > this._value )
                this._value = this._minimum;

            this._maximum = maxValue;

            if ( this._maximum < this._value )
                this._value = this._maximum;
            if ( this._maximum < this._minimum )
                this._minimum = this._maximum;

            this.Invalidate();
        }

        /// <summary>
        /// Reset the appearance properties.
        /// </summary>
        public void ResetAppearance() {
            this.Font = new Font("Verdana", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            this.ForeColor = Color.FromArgb(123, 125, 123);
            this.BackColor = Color.Transparent;

            this._tickColor = Color.FromArgb(148, 146, 148);
            this._tickHeight = 4;

            this._trackerColor = Color.FromArgb(24, 130, 198);
            this._trackerSize = new Size(16, 16);
            //_trackerRect.Size = _trackerSize;

            this._indentWidth = 6;
            this._indentHeight = 6;

            this._trackLineColor = Color.FromArgb(90, 93, 90);
            this._trackLineHeight = 3;

            this._borderStyle = CustomBorderStyle.None;
            this._borderColor = SystemColors.ActiveBorder;

            //==========================================================================

            if ( this._autoSize == true )
                this.Size = this.FitSize;
            this.Invalidate();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// The OnCreateControl method is called when the control is first created.
        /// </summary>
        protected override void OnCreateControl() {
        }

        /// <summary>
        /// This member overrides <see cref="Control.OnLostFocus">Control.OnLostFocus</see>.
        /// </summary>
        protected override void OnLostFocus(EventArgs e) {
            this.Invalidate();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// This member overrides <see cref="Control.OnGotFocus">Control.OnGotFocus</see>.
        /// </summary>
        protected override void OnGotFocus(EventArgs e) {
            this.Invalidate();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// This member overrides <see cref="Control.OnClick">Control.OnClick</see>.
        /// </summary>
        protected override void OnClick(EventArgs e) {
            this.Focus();
            this.Invalidate();
            base.OnClick(e);
        }

        /// <summary>
        /// This member overrides <see cref="Control.ProcessCmdKey">Control.ProcessCmdKey</see>.
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            bool blResult = true;

            /// <summary>
            /// Specified WM_KEYDOWN enumeration value.
            /// </summary>
            const int WM_KEYDOWN = 0x0100;

            /// <summary>
            /// Specified WM_SYSKEYDOWN enumeration value.
            /// </summary>
            const int WM_SYSKEYDOWN = 0x0104;


            if ( (msg.Msg == WM_KEYDOWN) || (msg.Msg == WM_SYSKEYDOWN) ) {
                switch ( keyData ) {
                    case Keys.Left:
                    case Keys.Down:
                        this.Decrement(this._smallChange);
                        break;
                    case Keys.Right:
                    case Keys.Up:
                        this.Increment(this._smallChange);
                        break;

                    case Keys.PageUp:
                        this.Increment(this._largeChange);
                        break;
                    case Keys.PageDown:
                        this.Decrement(this._largeChange);
                        break;

                    case Keys.Home:
                        this.Value = this._maximum;
                        break;
                    case Keys.End:
                        this.Value = this._minimum;
                        break;

                    default:
                        blResult = base.ProcessCmdKey(ref msg, keyData);
                        break;
                }
            }

            return blResult;
        }

        /// <summary>
        /// Dispose of instance resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
        }

        #endregion

        #region Painting Methods

        /// <summary>
        /// This member overrides <see cref="Control.OnPaint">Control.OnPaint</see>.
        /// </summary>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
            Brush brush;
            RectangleF rectTemp, drawRect;
            float textAreaSize;

            Rectangle workingRect = Rectangle.Inflate(this.ClientRectangle, -this._indentWidth, -this._indentHeight);
            float currentUsedPos = 0;

            //==========================================================================
            // Draw the background of the ProgressBar control.
            //==========================================================================
            brush = new SolidBrush(this.BackColor);
            rectTemp = this.ClientRectangle;
            e.Graphics.FillRectangle(brush, rectTemp);
            brush.Dispose();
            //==========================================================================

            //==========================================================================
            if ( this._orientation == Orientation.Horizontal ) {
                currentUsedPos = this._indentHeight;
                //==========================================================================

                // Get Height of Text Area
                textAreaSize = e.Graphics.MeasureString(this._maximum.ToString(), this.Font).Height;

                if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 1st Text Line.
                    //==========================================================================
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, textAreaSize);
                    drawRect.Inflate(-this._trackerSize.Width / 2, 0);
                    currentUsedPos += textAreaSize;

                    this.DrawTickTextLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this.ForeColor, this.Font, this._orientation);
                    //==========================================================================
                }

                if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 1st Tick Line.
                    //==========================================================================
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, this._tickHeight);
                    drawRect.Inflate(-this._trackerSize.Width / 2, 0);
                    currentUsedPos += this._tickHeight + 1;

                    this.DrawTickLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this._tickColor, this._orientation);
                    //==========================================================================
                }

                //==========================================================================
                // Caculate the Tracker's rectangle
                //==========================================================================
                float currentTrackerPos;
                if ( this._maximum == this._minimum )
                    currentTrackerPos = workingRect.Left;
                else
                    currentTrackerPos = (workingRect.Width - this._trackerSize.Width) * (this._value - this._minimum) / (this._maximum - this._minimum) + workingRect.Left;
                this._trackerRect = new RectangleF(currentTrackerPos, currentUsedPos, this._trackerSize.Width, this._trackerSize.Height);// Remember this for drawing the Tracker later
                                                                                                                                         //_trackerRect.Inflate(0,-1);

                //==========================================================================
                // Draw the Track Line
                //==========================================================================
                drawRect = new RectangleF(workingRect.Left, currentUsedPos + this._trackerSize.Height / 2 - this._trackLineHeight / 2, workingRect.Width, this._trackLineHeight);
                this.DrawTrackLine(e.Graphics, drawRect);
                currentUsedPos += this._trackerSize.Height;


                //==========================================================================

                if ( this._tickStyle == TickStyle.BottomRight || this._tickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 2st Tick Line.
                    //==========================================================================
                    currentUsedPos += 1;
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, this._tickHeight);
                    drawRect.Inflate(-this._trackerSize.Width / 2, 0);
                    currentUsedPos += this._tickHeight;

                    this.DrawTickLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this._tickColor, this._orientation);
                    //==========================================================================
                }

                if ( this._textTickStyle == TickStyle.BottomRight || this._textTickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 2st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(workingRect.Left, currentUsedPos, workingRect.Width, textAreaSize);
                    drawRect.Inflate(-this._trackerSize.Width / 2, 0);
                    currentUsedPos += textAreaSize;

                    this.DrawTickTextLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this.ForeColor, this.Font, this._orientation);
                    //==========================================================================
                }
            } else //_orientation == Orientation.Vertical
              {
                currentUsedPos = this._indentWidth;
                //==========================================================================

                // Get Width of Text Area
                textAreaSize = e.Graphics.MeasureString(this._maximum.ToString(), this.Font).Width;

                if ( this._textTickStyle == TickStyle.TopLeft || this._textTickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 1st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, textAreaSize, workingRect.Height);
                    drawRect.Inflate(0, -this._trackerSize.Width / 2);
                    currentUsedPos += textAreaSize;

                    this.DrawTickTextLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this.ForeColor, this.Font, this._orientation);
                    //==========================================================================
                }

                if ( this._tickStyle == TickStyle.TopLeft || this._tickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 1st Tick Line.
                    //==========================================================================
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, this._tickHeight, workingRect.Height);
                    drawRect.Inflate(0, -this._trackerSize.Width / 2);
                    currentUsedPos += this._tickHeight + 1;

                    this.DrawTickLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this._tickColor, this._orientation);
                    //==========================================================================
                }

                //==========================================================================
                // Caculate the Tracker's rectangle
                //==========================================================================
                float currentTrackerPos;
                if ( this._maximum == this._minimum )
                    currentTrackerPos = workingRect.Top;
                else
                    currentTrackerPos = (workingRect.Height - this._trackerSize.Width) * (this._value - this._minimum) / (this._maximum - this._minimum);

                this._trackerRect = new RectangleF(currentUsedPos, workingRect.Bottom - currentTrackerPos - this._trackerSize.Width, this._trackerSize.Height, this._trackerSize.Width);// Remember this for drawing the Tracker later
                                                                                                                                                                                        //_trackerRect.Inflate(-1,0);

                rectTemp = this._trackerRect;//Testing

                //==========================================================================
                // Draw the Track Line
                //==========================================================================
                drawRect = new RectangleF(currentUsedPos + this._trackerSize.Height / 2 - this._trackLineHeight / 2, workingRect.Top, this._trackLineHeight, workingRect.Height);
                this.DrawTrackLine(e.Graphics, drawRect);
                currentUsedPos += this._trackerSize.Height;
                //==========================================================================

                if ( this._tickStyle == TickStyle.BottomRight || this._tickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 2st Tick Line.
                    //==========================================================================
                    currentUsedPos += 1;
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, this._tickHeight, workingRect.Height);
                    drawRect.Inflate(0, -this._trackerSize.Width / 2);
                    currentUsedPos += this._tickHeight;

                    this.DrawTickLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this._tickColor, this._orientation);
                    //==========================================================================
                }

                if ( this._textTickStyle == TickStyle.BottomRight || this._textTickStyle == TickStyle.Both ) {
                    //==========================================================================
                    // Draw the 2st Text Line.
                    //==========================================================================
                    // Get Height of Text Area
                    drawRect = new RectangleF(currentUsedPos, workingRect.Top, textAreaSize, workingRect.Height);
                    drawRect.Inflate(0, -this._trackerSize.Width / 2);
                    currentUsedPos += textAreaSize;

                    this.DrawTickTextLine(e.Graphics, drawRect, this._tickFrequency, this._minimum, this._maximum, this.ForeColor, this.Font, this._orientation);
                    //==========================================================================
                }
            }

            //==========================================================================
            // Check for special values of Max, Min & Value
            if ( this._maximum == this._minimum ) {
                // Draw border only and exit;
                this.DrawBorder(e.Graphics);
                return;
            }
            //==========================================================================

            //==========================================================================
            // Draw the Tracker
            //==========================================================================
            this.DrawTracker(e.Graphics, this._trackerRect);
            //==========================================================================

            // Draw border
            this.DrawBorder(e.Graphics);
            //==========================================================================

            // Draws a focus rectangle
            //if(this.Focused && this.BackColor != Color.Transparent)
            if ( this.Focused )
                ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(this.ClientRectangle, -2, -2));
            //==========================================================================
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawRect"></param>
        private void DrawTrackLine(Graphics g, RectangleF drawRect) {
            DrawStyleHelper.DrawAquaPillSingleLayer(g, drawRect, this._trackLineColor, this._orientation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="trackerRect"></param>
        private void DrawTracker(Graphics g, RectangleF trackerRect) {
            DrawStyleHelper.DrawAquaPill(g, trackerRect, this._trackerColor, this._orientation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawRect"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="foreColor"></param>
        /// <param name="font"></param>
        /// <param name="orientation"></param>
        private void DrawTickTextLine(Graphics g, RectangleF drawRect, int tickFrequency, int minimum, int maximum, Color foreColor, Font font, Orientation orientation) {

            //Check input value
            if ( maximum == minimum )
                return;

            //Caculate tick number
            int tickCount = (maximum - minimum) / tickFrequency;
            if ( (maximum - minimum) % tickFrequency == 0 )
                tickCount -= 1;

            //Prepare for drawing Text
            //===============================================================
            StringFormat stringFormat;
            stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoWrap;
            stringFormat.LineAlignment = StringAlignment.Center;
            stringFormat.Alignment = StringAlignment.Center;
            stringFormat.Trimming = StringTrimming.EllipsisCharacter;
            stringFormat.HotkeyPrefix = HotkeyPrefix.Show;

            Brush brush = new SolidBrush(foreColor);
            string text;
            float tickFrequencySize;
            //===============================================================

            if ( this._orientation == Orientation.Horizontal ) {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Width * tickFrequency / (maximum - minimum);

                //===============================================================

                // Draw each tick text
                for ( int i = 0; i <= tickCount; i++ ) {
                    text = Convert.ToString(this._minimum + tickFrequency * i, 10);
                    g.DrawString(text, font, brush, drawRect.Left + tickFrequencySize * i, drawRect.Top + drawRect.Height / 2, stringFormat);

                }
                // Draw last tick text at Maximum
                text = Convert.ToString(this._maximum, 10);
                g.DrawString(text, font, brush, drawRect.Right, drawRect.Top + drawRect.Height / 2, stringFormat);

                //===============================================================
            } else //Orientation.Vertical
              {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Height * tickFrequency / (maximum - minimum);
                //===============================================================

                // Draw each tick text
                for ( int i = 0; i <= tickCount; i++ ) {
                    text = Convert.ToString(this._minimum + tickFrequency * i, 10);
                    g.DrawString(text, font, brush, drawRect.Left + drawRect.Width / 2, drawRect.Bottom - tickFrequencySize * i, stringFormat);
                }
                // Draw last tick text at Maximum
                text = Convert.ToString(this._maximum, 10);
                g.DrawString(text, font, brush, drawRect.Left + drawRect.Width / 2, drawRect.Top, stringFormat);
                //===============================================================

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawRect"></param>
        /// <param name="tickFrequency"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <param name="tickColor"></param>
        /// <param name="orientation"></param>
        private void DrawTickLine(Graphics g, RectangleF drawRect, int tickFrequency, int minimum, int maximum, Color tickColor, Orientation orientation) {
            //Check input value
            if ( maximum == minimum )
                return;

            //Create the Pen for drawing Ticks
            Pen pen = new Pen(tickColor, 1);
            float tickFrequencySize;

            //Caculate tick number
            int tickCount = (maximum - minimum) / tickFrequency;
            if ( (maximum - minimum) % tickFrequency == 0 )
                tickCount -= 1;

            if ( this._orientation == Orientation.Horizontal ) {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Width * tickFrequency / (maximum - minimum);

                //===============================================================

                // Draw each tick
                for ( int i = 0; i <= tickCount; i++ ) {
                    g.DrawLine(pen, drawRect.Left + tickFrequencySize * i, drawRect.Top, drawRect.Left + tickFrequencySize * i, drawRect.Bottom);
                }
                // Draw last tick at Maximum
                g.DrawLine(pen, drawRect.Right, drawRect.Top, drawRect.Right, drawRect.Bottom);
                //===============================================================
            } else //Orientation.Vertical
              {
                // Calculate tick's setting
                tickFrequencySize = drawRect.Height * tickFrequency / (maximum - minimum);
                //===============================================================

                // Draw each tick
                for ( int i = 0; i <= tickCount; i++ ) {
                    g.DrawLine(pen, drawRect.Left, drawRect.Bottom - tickFrequencySize * i, drawRect.Right, drawRect.Bottom - tickFrequencySize * i);
                }
                // Draw last tick at Maximum
                g.DrawLine(pen, drawRect.Left, drawRect.Top, drawRect.Right, drawRect.Top);
                //===============================================================
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void DrawBorder(Graphics g) {

            switch ( this._borderStyle ) {
                case CustomBorderStyle.Dashed: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, this.ClientRectangle, this._borderColor, ButtonBorderStyle.Dashed);
                    break;
                case CustomBorderStyle.Dotted: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, this.ClientRectangle, this._borderColor, ButtonBorderStyle.Dotted);
                    break;
                case CustomBorderStyle.Inset: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, this.ClientRectangle, this._borderColor, ButtonBorderStyle.Inset);
                    break;
                case CustomBorderStyle.Outset: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, this.ClientRectangle, this._borderColor, ButtonBorderStyle.Outset);
                    break;
                case CustomBorderStyle.Solid: //from ButtonBorderStyle Enumeration
                    ControlPaint.DrawBorder(g, this.ClientRectangle, this._borderColor, ButtonBorderStyle.Solid);
                    break;

                case CustomBorderStyle.Adjust: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Adjust);
                    break;
                case CustomBorderStyle.Bump: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Bump);
                    break;
                case CustomBorderStyle.Etched: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Etched);
                    break;
                case CustomBorderStyle.Flat: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Flat);
                    break;
                case CustomBorderStyle.Raised: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Raised);
                    break;
                case CustomBorderStyle.RaisedInner: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.RaisedInner);
                    break;
                case CustomBorderStyle.RaisedOuter: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.RaisedOuter);
                    break;
                case CustomBorderStyle.Sunken: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.Sunken);
                    break;
                case CustomBorderStyle.SunkenInner: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.SunkenInner);
                    break;
                case CustomBorderStyle.SunkenOuter: //from Border3DStyle Enumeration
                    ControlPaint.DrawBorder3D(g, this.ClientRectangle, Border3DStyle.SunkenOuter);
                    break;
                case CustomBorderStyle.None:
                default:
                    break;
            }
        }


        #endregion

        #region Private Methods

        private void OnMouseDownSlider(object sender, MouseEventArgs e) {
            int offsetValue = 0;
            int oldValue = 0;
            PointF currentPoint;

            currentPoint = new PointF(e.X, e.Y);
            if ( this._trackerRect.Contains(currentPoint) ) {
                if ( !this.leftButtonDown ) {
                    this.leftButtonDown = true;
                    this.Capture = true;
                    switch ( this._orientation ) {
                        case Orientation.Horizontal:
                            this.mouseStartPos = currentPoint.X - this._trackerRect.X;
                            break;

                        case Orientation.Vertical:
                            this.mouseStartPos = currentPoint.Y - this._trackerRect.Y;
                            break;
                    }
                }
            } else {
                switch ( this._orientation ) {
                    case Orientation.Horizontal:
                        if ( currentPoint.X + this._trackerSize.Width / 2 >= this.Width - this._indentWidth )
                            offsetValue = this._maximum - this._minimum;
                        else if ( currentPoint.X - this._trackerSize.Width / 2 <= this._indentWidth )
                            offsetValue = 0;
                        else
                            offsetValue = (int)(((currentPoint.X - this._indentWidth - this._trackerSize.Width / 2) * (this._maximum - this._minimum)) / (this.Width - 2 * this._indentWidth - this._trackerSize.Width) + 0.5);

                        break;

                    case Orientation.Vertical:
                        if ( currentPoint.Y + this._trackerSize.Width / 2 >= this.Height - this._indentHeight )
                            offsetValue = 0;
                        else if ( currentPoint.Y - this._trackerSize.Width / 2 <= this._indentHeight )
                            offsetValue = this._maximum - this._minimum;
                        else
                            offsetValue = (int)(((this.Height - currentPoint.Y - this._indentHeight - this._trackerSize.Width / 2) * (this._maximum - this._minimum)) / (this.Height - 2 * this._indentHeight - this._trackerSize.Width) + 0.5);

                        break;

                    default:
                        break;
                }

                oldValue = this._value;
                this._value = this._minimum + offsetValue;
                this.Invalidate();

                if ( oldValue != this._value ) {
                    this.OnScroll();
                    this.OnValueChanged(this._value);
                }
            }

        }

        private void OnMouseUpSlider(object sender, MouseEventArgs e) {
            this.leftButtonDown = false;
            this.Capture = false;

        }

        private void OnMouseMoveSlider(object sender, MouseEventArgs e) {
            int offsetValue = 0;
            int oldValue = 0;
            PointF currentPoint;

            currentPoint = new PointF(e.X, e.Y);

            if ( this.leftButtonDown ) {
                try {
                    switch ( this._orientation ) {
                        case Orientation.Horizontal:
                            if ( (currentPoint.X + this._trackerSize.Width - this.mouseStartPos) >= this.Width - this._indentWidth )
                                offsetValue = this._maximum - this._minimum;
                            else if ( currentPoint.X - this.mouseStartPos <= this._indentWidth )
                                offsetValue = 0;
                            else
                                offsetValue = (int)(((currentPoint.X - this.mouseStartPos - this._indentWidth) * (this._maximum - this._minimum)) / (this.Width - 2 * this._indentWidth - this._trackerSize.Width) + 0.5);

                            break;

                        case Orientation.Vertical:
                            if ( currentPoint.Y + this._trackerSize.Width / 2 >= this.Height - this._indentHeight )
                                offsetValue = 0;
                            else if ( currentPoint.Y + this._trackerSize.Width / 2 <= this._indentHeight )
                                offsetValue = this._maximum - this._minimum;
                            else
                                offsetValue = (int)(((this.Height - currentPoint.Y + this._trackerSize.Width / 2 - this.mouseStartPos - this._indentHeight) * (this._maximum - this._minimum)) / (this.Height - 2 * this._indentHeight) + 0.5);

                            break;
                    }

                } catch ( Exception ) { } finally {
                    oldValue = this._value;
                    this.Value = this._minimum + offsetValue;
                    this.Invalidate();

                    if ( oldValue != this._value ) {
                        this.OnScroll();
                        this.OnValueChanged(this._value);
                    }
                }
            }

        }


        #endregion

    }

    /// <summary>
    /// Summary description for DrawStyleHelper.
    /// </summary>
    public sealed class DrawStyleHelper {
        /// <summary>
        /// The contructor 
        /// </summary>
        private DrawStyleHelper() {
            //
            // TODO: Add constructor logic here
            //
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawRectF"></param>
        /// <param name="drawColor"></param>
        /// <param name="orientation"></param>
        public static void DrawAquaPill(Graphics g, RectangleF drawRectF, Color drawColor, Orientation orientation) {
            Color color1;
            Color color2;
            Color color3;
            Color color4;
            Color color5;
            System.Drawing.Drawing2D.LinearGradientBrush gradientBrush;
            System.Drawing.Drawing2D.ColorBlend colorBlend = new System.Drawing.Drawing2D.ColorBlend();

            color1 = ColorHelper.OpacityMix(Color.White, ColorHelper.SoftLightMix(drawColor, Color.Black, 100), 40);
            color2 = ColorHelper.OpacityMix(Color.White, ColorHelper.SoftLightMix(drawColor, ColorHelper.CreateColorFromRGB(64, 64, 64), 100), 20);
            color3 = ColorHelper.SoftLightMix(drawColor, ColorHelper.CreateColorFromRGB(128, 128, 128), 100);
            color4 = ColorHelper.SoftLightMix(drawColor, ColorHelper.CreateColorFromRGB(192, 192, 192), 100);
            color5 = ColorHelper.OverlayMix(ColorHelper.SoftLightMix(drawColor, Color.White, 100), Color.White, 75);

            //			
            colorBlend.Colors = new Color[] { color1, color2, color3, color4, color5 };
            colorBlend.Positions = new float[] { 0, 0.25f, 0.5f, 0.75f, 1 };
            if ( orientation == Orientation.Horizontal )
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top - 1), new Point((int)drawRectF.Left, (int)drawRectF.Top + (int)drawRectF.Height + 1), color1, color5);
            else
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left - 1, (int)drawRectF.Top), new Point((int)drawRectF.Left + (int)drawRectF.Width + 1, (int)drawRectF.Top), color1, color5);
            gradientBrush.InterpolationColors = colorBlend;
            FillPill(gradientBrush, drawRectF, g);

            //
            color2 = Color.White;
            colorBlend.Colors = new Color[] { color2, color3, color4, color5 };
            colorBlend.Positions = new float[] { 0, 0.5f, 0.75f, 1 };
            if ( orientation == Orientation.Horizontal )
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left + 1, (int)drawRectF.Top), new Point((int)drawRectF.Left + 1, (int)drawRectF.Top + (int)drawRectF.Height - 1), color2, color5);
            else
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top + 1), new Point((int)drawRectF.Left + (int)drawRectF.Width - 1, (int)drawRectF.Top + 1), color2, color5);
            gradientBrush.InterpolationColors = colorBlend;
            FillPill(gradientBrush, RectangleF.Inflate(drawRectF, -3, -3), g);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawRectF"></param>
        /// <param name="drawColor"></param>
        /// <param name="orientation"></param>
        public static void DrawAquaPillSingleLayer(Graphics g, RectangleF drawRectF, Color drawColor, Orientation orientation) {
            Color color1;
            Color color2;
            Color color3;
            Color color4;
            System.Drawing.Drawing2D.LinearGradientBrush gradientBrush;
            System.Drawing.Drawing2D.ColorBlend colorBlend = new System.Drawing.Drawing2D.ColorBlend();

            color1 = drawColor;
            color2 = ControlPaint.Light(color1);
            color3 = ControlPaint.Light(color2);
            color4 = ControlPaint.Light(color3);

            colorBlend.Colors = new Color[] { color1, color2, color3, color4 };
            colorBlend.Positions = new float[] { 0, 0.25f, 0.65f, 1 };

            if ( orientation == Orientation.Horizontal )
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top), new Point((int)drawRectF.Left, (int)drawRectF.Top + (int)drawRectF.Height), color1, color4);
            else
                gradientBrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point((int)drawRectF.Left, (int)drawRectF.Top), new Point((int)drawRectF.Left + (int)drawRectF.Width, (int)drawRectF.Top), color1, color4);
            gradientBrush.InterpolationColors = colorBlend;

            FillPill(gradientBrush, drawRectF, g);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="rect"></param>
        /// <param name="g"></param>
        public static void FillPill(Brush b, RectangleF rect, Graphics g) {
            if ( rect.Width > rect.Height ) {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top, rect.Height, rect.Height));
                g.FillEllipse(b, new RectangleF(rect.Left + rect.Width - rect.Height, rect.Top, rect.Height, rect.Height));

                float w = rect.Width - rect.Height;
                float l = rect.Left + ((rect.Height) / 2);
                g.FillRectangle(b, new RectangleF(l, rect.Top, w, rect.Height));
                g.SmoothingMode = SmoothingMode.Default;
            } else if ( rect.Width < rect.Height ) {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top, rect.Width, rect.Width));
                g.FillEllipse(b, new RectangleF(rect.Left, rect.Top + rect.Height - rect.Width, rect.Width, rect.Width));

                float t = rect.Top + (rect.Width / 2);
                float h = rect.Height - rect.Width;
                g.FillRectangle(b, new RectangleF(rect.Left, t, rect.Width, h));
                g.SmoothingMode = SmoothingMode.Default;
            } else if ( rect.Width == rect.Height ) {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.FillEllipse(b, rect);
                g.SmoothingMode = SmoothingMode.Default;
            }
        }

    }

    /// <summary>
    /// Summary description for ColorHelper.
    /// </summary>
    internal class ColorHelper {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public static Color CreateColorFromRGB(int red, int green, int blue) {
            //Corect Red element
            int r = red;
            if ( r > 255 ) {
                r = 255;
            }
            if ( r < 0 ) {
                r = 0;
            }
            //Corect Green element
            int g = green;
            if ( g > 255 ) {
                g = 255;
            }
            if ( g < 0 ) {
                g = 0;
            }
            //Correct Blue Element
            int b = blue;
            if ( b > 255 ) {
                b = 255;
            }
            if ( b < 0 ) {
                b = 0;
            }
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blendColor"></param>
        /// <param name="baseColor"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static Color OpacityMix(Color blendColor, Color baseColor, int opacity) {
            int r1;
            int g1;
            int b1;
            int r2;
            int g2;
            int b2;
            int r3;
            int g3;
            int b3;
            r1 = blendColor.R;
            g1 = blendColor.G;
            b1 = blendColor.B;
            r2 = baseColor.R;
            g2 = baseColor.G;
            b2 = baseColor.B;
            r3 = (int)(((r1 * ((float)opacity / 100)) + (r2 * (1 - ((float)opacity / 100)))));
            g3 = (int)(((g1 * ((float)opacity / 100)) + (g2 * (1 - ((float)opacity / 100)))));
            b3 = (int)(((b1 * ((float)opacity / 100)) + (b2 * (1 - ((float)opacity / 100)))));
            return CreateColorFromRGB(r3, g3, b3);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="blendColor"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static Color SoftLightMix(Color baseColor, Color blendColor, int opacity) {
            int r1;
            int g1;
            int b1;
            int r2;
            int g2;
            int b2;
            int r3;
            int g3;
            int b3;
            r1 = baseColor.R;
            g1 = baseColor.G;
            b1 = baseColor.B;
            r2 = blendColor.R;
            g2 = blendColor.G;
            b2 = blendColor.B;
            r3 = SoftLightMath(r1, r2);
            g3 = SoftLightMath(g1, g2);
            b3 = SoftLightMath(b1, b2);
            return OpacityMix(CreateColorFromRGB(r3, g3, b3), baseColor, opacity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="blendColor"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        public static Color OverlayMix(Color baseColor, Color blendColor, int opacity) {
            int r1;
            int g1;
            int b1;
            int r2;
            int g2;
            int b2;
            int r3;
            int g3;
            int b3;
            r1 = baseColor.R;
            g1 = baseColor.G;
            b1 = baseColor.B;
            r2 = blendColor.R;
            g2 = blendColor.G;
            b2 = blendColor.B;
            r3 = OverlayMath(baseColor.R, blendColor.R);
            g3 = OverlayMath(baseColor.G, blendColor.G);
            b3 = OverlayMath(baseColor.B, blendColor.B);
            return OpacityMix(CreateColorFromRGB(r3, g3, b3), baseColor, opacity);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ibase"></param>
        /// <param name="blend"></param>
        /// <returns></returns>
        private static int SoftLightMath(int ibase, int blend) {
            float dbase;
            float dblend;
            dbase = (float)ibase / 255;
            dblend = (float)blend / 255;
            if ( dblend < 0.5 ) {
                return (int)(((2 * dbase * dblend) + (Math.Pow(dbase, 2)) * (1 - (2 * dblend))) * 255);
            } else {
                return (int)(((Math.Sqrt(dbase) * (2 * dblend - 1)) + ((2 * dbase) * (1 - dblend))) * 255);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ibase"></param>
        /// <param name="blend"></param>
        /// <returns></returns>
        public static int OverlayMath(int ibase, int blend) {
            double dbase;
            double dblend;
            dbase = (double)ibase / 255;
            dblend = (double)blend / 255;
            if ( dbase < 0.5 ) {
                return (int)((2 * dbase * dblend) * 255);
            } else {
                return (int)((1 - (2 * (1 - dbase) * (1 - dblend))) * 255);
            }
        }

    }

}