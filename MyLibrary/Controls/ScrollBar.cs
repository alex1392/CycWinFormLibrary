using MetroFramework;
using MetroFramework.Components;
using MetroFramework.Design;
using MetroFramework.Drawing;
using MetroFramework.Interfaces;
using MetroFramework.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static MyLibrary.MyMethods;

namespace MyLibrary.Controls
{
	[DefaultEvent("Scroll")]
	[DefaultProperty("Value")]
	public class ScrollBar : Control
	{
		#region Interface

		private MetroColorStyle metroStyle = MetroColorStyle.Blue;
		[Category("Appearance")]
		public MetroColorStyle Style
		{
			get
			{
				if (StyleManager != null)
					return StyleManager.Style;

				return metroStyle;
			}
			set { metroStyle = value; }
		}

		private MetroThemeStyle metroTheme = MetroThemeStyle.Light;
		[Category("Appearance")]
		public MetroThemeStyle Theme
		{
			get
			{
				if (StyleManager != null)
					return StyleManager.Theme;

				return metroTheme;
			}
			set { metroTheme = value; }
		}

		private MetroStyleManager metroStyleManager = null;
		[Browsable(false)]
		public MetroStyleManager StyleManager
		{
			get { return metroStyleManager; }
			set { metroStyleManager = value; }
		}

		#endregion

		#region Events

		public event ScrollEventHandler Scroll;
		private bool IsFirstScrollEventVertical = true;
		private bool IsFirstScrollEventHorizontal = true;
		private void OnScroll(ScrollEventType type, int oldValue, int newValue, ScrollOrientation orientation)
		{
			//Console.WriteLine("{0}", _Value);
			if (Scroll == null) return;

			if (orientation == ScrollOrientation.HorizontalScroll)
			{
				if (type != ScrollEventType.EndScroll && IsFirstScrollEventHorizontal)
				{
					type = ScrollEventType.First;
				}
				else if (!IsFirstScrollEventHorizontal && type == ScrollEventType.EndScroll)
				{
					IsFirstScrollEventHorizontal = true;
				}
			}
			else
			{
				if (type != ScrollEventType.EndScroll && IsFirstScrollEventVertical)
				{
					type = ScrollEventType.First;
				}
				else if (!IsFirstScrollEventHorizontal && type == ScrollEventType.EndScroll)
				{
					IsFirstScrollEventVertical = true;
				}
			}

			Scroll(this, new ScrollEventArgs(type, oldValue, newValue, orientation));
		}

		#endregion

		#region Fields

		private ScrollOrientation scrollOrientation = ScrollOrientation.HorizontalScroll;
		private ScrollBarOrientation orientation = ScrollBarOrientation.Horizontal;
		[Category("Appearance")]
		public ScrollBarOrientation Orientation
		{
			get { return orientation; }

			set
			{
				if (value == orientation)
					return;

				orientation = value;

				if (value == ScrollBarOrientation.Vertical)
					scrollOrientation = ScrollOrientation.VerticalScroll;
				else
					scrollOrientation = ScrollOrientation.HorizontalScroll;

				this.Size = new Size(Height, Width);
				Refresh();
			}
		}

		private bool _UseBarColor = true;
		[Category("Appearance")]
		public bool UseBarColor
		{
			get { return _UseBarColor; }
			set { _UseBarColor = value; }
		}

		private int _Minimum = 0;
		[Category("Appearance")]
		public int Minimum
		{
			get { return _Minimum; }
			set
			{
				if (_Minimum == value || value >= _Maximum)
					return;

				_Minimum = value;

				if (_Value < _Minimum)
					Value = _Minimum; //Position Changed

				Invalidate();
			}
		}
		private int _Maximum = 100;
		[Category("Appearance")]
		public int Maximum
		{
			get { return _Maximum; }
			set
			{
				if (value == _Maximum || value <= _Minimum)
					return;

				_Maximum = value;

				if (_Value > _Maximum)
					Value = _Maximum; //Position Changed

				Invalidate();
			}
		}

		private int _ThumbLength;
		[Category("Appearance")]
		public int ThumbLength
		{
			get => _ThumbLength;
			set
			{
				if (value < Thickness)
					return;
				_ThumbLength = value;
				Invalidate();
			}
		}

		[Category("Appearance")]
		private int CapRadius => Thickness / 2;
		[Category("Appearance")]
		public int Thickness => Orientation == ScrollBarOrientation.Vertical ? Width : Height;

		private int ValueLength => _Maximum - _Minimum;
		private int _Value = 0;
		[Category("Appearance")]
		public int Value
		{
			get => _Value;
			set
			{
				_Value = Clamp(value, _Maximum, _Minimum);
				_ThumbFrontPosition = Value2Position(_Value);
				Invalidate();
			}
		}
		
		private int ThumbFrontPosition
		{
			get => _ThumbFrontPosition;
			set
			{
				_ThumbFrontPosition = Clamp(value, ThumbFrontPositionMax, ThumbFrontPositionMin);
				_Value = Position2Value(_ThumbFrontPosition);
				Invalidate();
			}
		}
		private int _ThumbFrontPosition;
		private int ThumbEndPosition => ThumbFrontPosition + ThumbLength; 
		private int ThumbEndPositionMax => orientation == ScrollBarOrientation.Vertical ?
				ClientRectangle.Bottom : ClientRectangle.Right;
		private int ThumbFrontPositionMax => ThumbEndPositionMax - ThumbLength;
		private int ThumbFrontPositionMin = 0;
		private int ThumbPositionLength => ThumbFrontPositionMax - ThumbFrontPositionMin;
		private Rectangle ThumbRectangle => orientation == ScrollBarOrientation.Vertical ?
				new Rectangle(0, ThumbFrontPosition, Thickness, ThumbLength) :
				new Rectangle(ThumbFrontPosition, 0, ThumbLength, Thickness);
		[Category("Appearance")]
		public int BarLength
		{
			get => orientation == ScrollBarOrientation.Vertical ? Height : Width;
			set
			{
				if (orientation == ScrollBarOrientation.Vertical)
					Height = value;
				else
					Width = value;
			}
		}

		private int Value2Position(int value)
		{
			return LinConvert(value, _Maximum, _Minimum, ThumbFrontPositionMax, ThumbFrontPositionMin);
			//return (int)(ThumbFrontPositionMin + (value - _Minimum) * ((float)ThumbPositionLength / ValueLength));
		}
		private int Position2Value(int position)
		{
			if (position == 0) // 避免除以0
				return _Minimum;
			else
				return LinConvert(position, ThumbFrontPositionMax, ThumbFrontPositionMin, _Maximum, _Minimum);
				//return (int)(_Minimum + (position - ThumbFrontPositionMin) * ((float)ValueLength / ThumbPositionLength));
		}
		#endregion

		#region Constructor
		public ScrollBar()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer |
							 ControlStyles.ResizeRedraw |
							 ControlStyles.Selectable |
							 ControlStyles.AllPaintingInWmPaint |
							 ControlStyles.UserPaint, true);

			SetProperties();
			Refresh();
		}
		public ScrollBar(ScrollBarOrientation orientation)
				: this()
		{
			Orientation = orientation;
		}
		public ScrollBar(ScrollBarOrientation orientation, int width)
				: this(orientation)
		{
			Width = width;
		}
		public bool HitTest(Point point)
		{
			return ThumbRectangle.Contains(point);
		}
		private void SetProperties()
		{
			Width = 200;
			Height = 10;
			ThumbLength = Height + 1;

			HoverTimer = new Timer();
			HoverTimer.Tick += HoverTimer_Tick;
			HoverTimer.Interval = 10;
		}
		#endregion

		#region Paint Methods
		
		private Color backColor => (Parent is IMetroControl || Parent == null) ? MetroPaint.BackColor.Form(Theme) : Parent.BackColor;
		private Color thumbColor;
		private Color barColor;
		protected override void OnPaint(PaintEventArgs e)
		{
			if (IsHovered && !IsPressed && Enabled)
			{
				thumbColor = MetroPaint.BackColor.ScrollBar.Thumb.Hover(Theme);
				barColor = MetroPaint.BackColor.ScrollBar.Bar.Hover(Theme);
			}
			else if (IsHovered && IsPressed && Enabled)
			{
				thumbColor = MetroPaint.BackColor.ScrollBar.Thumb.Pressed(Theme);
				barColor = MetroPaint.BackColor.ScrollBar.Bar.Pressed(Theme);
			}
			else if (!Enabled)
			{
				thumbColor = MetroPaint.BackColor.ScrollBar.Thumb.Disabled(Theme);
				barColor = MetroPaint.BackColor.ScrollBar.Bar.Disabled(Theme);
			}
			else
			{
				thumbColor = MetroPaint.BackColor.ScrollBar.Thumb.Normal(Theme);
				barColor = MetroPaint.BackColor.ScrollBar.Bar.Normal(Theme);
			}

			if (HoverTimer.Enabled)
			{
				barColor = Interpolate(MetroPaint.BackColor.ScrollBar.Bar.Normal(Theme), MetroPaint.BackColor.ScrollBar.Bar.Hover(Theme), HoverRatio);
				thumbColor = Interpolate(MetroPaint.BackColor.ScrollBar.Thumb.Normal(Theme), MetroPaint.BackColor.ScrollBar.Thumb.Hover(Theme), HoverRatio);
			}

			e.Graphics.Clear(backColor);
			DrawScrollBar(e.Graphics, backColor, thumbColor, barColor);
		}

		private Point BarPoint1 => new Point(CapRadius, CapRadius);
		private Point BarPoint2 => orientation == ScrollBarOrientation.Vertical ?
			new Point(CapRadius, ClientRectangle.Bottom - CapRadius) :
			new Point(ClientRectangle.Right - CapRadius, CapRadius);
		private Point ThumbPoint1 => orientation == ScrollBarOrientation.Vertical ?
			new Point(CapRadius, ThumbFrontPosition + CapRadius) :
			new Point(ThumbFrontPosition + CapRadius, CapRadius);
		private Point ThumbPoint2 => orientation == ScrollBarOrientation.Vertical ?
			new Point(CapRadius, ThumbEndPosition - CapRadius) :
			new Point(ThumbEndPosition - CapRadius, CapRadius);
		private void DrawScrollBar(Graphics g, Color backColor, Color thumbColor, Color barColor)
		{
			if (_UseBarColor)
			{
				using (var pen = new Pen(barColor)
				{
					EndCap = System.Drawing.Drawing2D.LineCap.Round,
					StartCap = System.Drawing.Drawing2D.LineCap.Round,
					Width = Thickness
				})
				{
					g.DrawLine(pen, BarPoint1, BarPoint2);
				}
			}
			if (Enabled)
			{
				using (var pen = new Pen(thumbColor)
				{
					EndCap = System.Drawing.Drawing2D.LineCap.Round,
					StartCap = System.Drawing.Drawing2D.LineCap.Round,
					Width = Thickness,
				})
				{
					g.DrawLine(pen, ThumbPoint1, ThumbPoint2);
				}
			}
		}

		#endregion

		#region Animation
		private Timer HoverTimer;
		private float HoverRatio; // 0~1
		private bool IsIncreasing;
		private void HoverTimer_Tick(object sender, EventArgs e)
		{
			HoverRatio = (IsIncreasing) ? HoverRatio + 0.05f : HoverRatio - 0.05f;
			Refresh();

			if (!IsIn(HoverRatio, 1, 0))
			{
				HoverTimer.Stop();
			}
		}
		#endregion

		#region Focus Methods

		protected override void OnGotFocus(EventArgs e)
		{
			Invalidate();

			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			Invalidate();

			base.OnLostFocus(e);
		}

		protected override void OnEnter(EventArgs e)
		{
			Invalidate();

			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			Invalidate();

			base.OnLeave(e);
		}

		#endregion

		#region Mouse Methods
		private int _MouseWheelBarPartitions;
		public int MouseWheelBarPartitions
		{
			get
			{
				if (_MouseWheelBarPartitions == 0)
					return 20;
				else
					return _MouseWheelBarPartitions;
			}
			set
			{
				_MouseWheelBarPartitions = Clamp(value, int.MaxValue, 1);
			}
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			int ValueOld = _Value;
			int v = (int)(e.Delta / 60f * ValueLength / MouseWheelBarPartitions);
			Value = orientation == ScrollBarOrientation.Vertical ? Value - v : Value + v; //Position Changed
			OnScroll(ScrollEventType.ThumbPosition, ValueOld, _Value, scrollOrientation);
		}

		private bool IsHovered;
		private bool IsPressed;
		private bool IsThumbClicked;
		private int ClickPosition;
		private int ThumbFrontPositionOld;
		private int MousePositionMax;
		private int MousePositionMin;
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button == MouseButtons.Left)
			{
				IsPressed = true;
				Point ClickLocation = e.Location;
				ClickPosition = orientation == ScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;
				ThumbFrontPositionOld = ThumbFrontPosition;
				MousePositionMin = ClickPosition - (ThumbFrontPosition - ThumbFrontPositionMin);
				MousePositionMax = ClickPosition + (ThumbEndPositionMax - ThumbEndPosition);
				if (ThumbRectangle.Contains(ClickLocation))
				{
					IsThumbClicked = true;
				}
				else
				{
					if (ClickPosition < ThumbFrontPosition)
						Value -= SmallChange; //Position Changed, Refresh
					else
						Value += SmallChange;
				}
			}
			HoverTimer.Stop();
			Refresh();
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			IsPressed = false;

			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				if (IsThumbClicked)
				{
					IsThumbClicked = false;
					OnScroll(ScrollEventType.EndScroll, -1, _Value, scrollOrientation);
				}
				Invalidate();
			}
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button == MouseButtons.Left && IsThumbClicked)
			{
				int ValueOld = _Value;
				int MousePosition = orientation == ScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;
				MousePosition = Clamp(MousePosition, MousePositionMax, MousePositionMin);
				int DeltaPosition = MousePosition - ClickPosition;
				ThumbFrontPosition = ThumbFrontPositionOld + DeltaPosition; //Value Changed
				OnScroll(ScrollEventType.ThumbTrack, ValueOld, _Value, scrollOrientation);
				Invalidate();
			}
		}
		protected override void OnMouseEnter(EventArgs e)
		{
			Focus();
			IsHovered = true;

			HoverRatio = 0;
			IsIncreasing = true;
			HoverTimer.Start();

			Invalidate();

			base.OnMouseEnter(e);
		}
		protected override void OnMouseLeave(EventArgs e)
		{
			IsHovered = false;

			IsIncreasing = false;
			if (!HoverTimer.Enabled)
			{
				HoverRatio = 1;
				HoverTimer.Start();
			}

			Invalidate();

			base.OnMouseLeave(e);
		}
		#endregion

		#region Keyboard Methods
		private int _SmallChange;
		public int SmallChange
		{
			get
			{
				if (_SmallChange == 0)
					return (int)(ValueLength / 10f);
				else
					return _SmallChange;
			}
			set
			{
				if (_LargeChange == 0)
					_LargeChange = _SmallChange + 1;
				_SmallChange = Clamp(value, _LargeChange, 0);
			}
		}
		private int _LargeChange;
		public int LargeChange
		{
			get
			{
				if (_LargeChange == 0)
					return (int)(ValueLength / 5f);
				else
					return _LargeChange;
			}
			set
			{
				_LargeChange = Clamp(value, ValueLength, _SmallChange);
			}
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			bool isScroll = true;
			int ValueOld = _Value;
			switch (e.KeyCode)
			{
				case Keys.Up:
					{
						if (orientation == ScrollBarOrientation.Vertical)
							Value -= SmallChange; //Position Changed, Refresh
						break;
					}
				case Keys.Down:
					{
						if (orientation == ScrollBarOrientation.Vertical)
							Value += SmallChange;
						break;
					}
				case Keys.Right:
					{
						if (orientation == ScrollBarOrientation.Horizontal)
							Value += SmallChange;
						break;
					}
				case Keys.Left:
					{
						if (orientation == ScrollBarOrientation.Horizontal)
							Value -= SmallChange;
						break;
					}
				case Keys.PageUp:
					{
						Value -= LargeChange;
						break;
					}
				case Keys.PageDown:
					{
						Value += LargeChange;
						break;
					}
				case Keys.End:
					{
						Value = _Maximum;
						break;
					}
				case Keys.Home:
					{
						Value = _Minimum;
						break;
					}
				default:
					{
						isScroll = false;
						break;
					}
			}
			if (isScroll)
			{
				OnScroll(ScrollEventType.ThumbPosition, ValueOld, _Value, scrollOrientation);
			}
			Invalidate();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab | ModifierKeys == Keys.Shift)
				return base.ProcessDialogKey(keyData);
			else
			{
				OnKeyDown(new KeyEventArgs(keyData));
				return true;
			}
		}
		#endregion

		#region Management Methods
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore(x, y, width, height, specified);

			if (DesignMode)
			{
				//SetupScrollBar();
				Invalidate();
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			BarLength = Clamp(BarLength, 10000, Thickness + 1);
			ThumbLength = Clamp(ThumbLength, BarLength, Thickness + 1);
			ThumbFrontPosition = Clamp(ThumbFrontPosition, ThumbFrontPositionMax, ThumbFrontPositionMin);

			ThumbFrontPosition = ThumbFrontPosition; // 更新Value
			Refresh();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			Invalidate();
		}
		#endregion
	}
}
