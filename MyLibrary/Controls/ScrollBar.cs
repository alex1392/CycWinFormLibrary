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

namespace MyLibrary.Controls
{
	[DefaultEvent("Scroll")]
	[DefaultProperty("Value")]
	public class ScrollBar : Control
	{
		#region Interface

		private MetroColorStyle metroStyle = MetroColorStyle.Blue;
		[Category("Metro Appearance")]
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
		[Category("Metro Appearance")]
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
		[Category("Metro Appearance")]
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
		[Category("Metro Appearance")]
		public bool UseBarColor
		{
			get { return _UseBarColor; }
			set { _UseBarColor = value; }
		}

		private int _Minimum = 0;
		[Category("Metro Appearance")]
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
		[Category("Metro Appearance")]
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
		[Category("Metro Appearance")]
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

		[Category("Metro Appearance")]
		private int CapRadius => Thickness / 2;
		public int Thickness => Orientation == ScrollBarOrientation.Vertical ? Width : Height;

		private int ValueLength => _Maximum - _Minimum;
		private int _Value = 0;
		[Category("Metro Appearance")]
		public int Value
		{
			get => _Value;
			set
			{
				int ValueOld = _Value;
				int tmpValue = MyMethods.Clamp(value, _Maximum, _Minimum);
				_Value = tmpValue;
				_ThumbFrontPosition = Value2Position(_Value);

				if (ValueOld != _Value)
					OnScroll(ScrollEventType.ThumbPosition, ValueOld, _Value, scrollOrientation);

				Invalidate();
			}
		}
		
		private int ThumbFrontPosition
		{
			get => _ThumbFrontPosition;
			set
			{
				int tmpValue = MyMethods.Clamp(value, ThumbFrontPositionMax, ThumbFrontPositionMin);
				_ThumbFrontPosition = tmpValue;
				_Value = Position2Value(_ThumbFrontPosition);
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
			return MyMethods.LinConvert(value, _Maximum, _Minimum, ThumbFrontPositionMax, ThumbFrontPositionMin);
			//return (int)(ThumbFrontPositionMin + (value - _Minimum) * ((float)ThumbPositionLength / ValueLength));
		}
		private int Position2Value(int position)
		{
			if (position == 0) // 避免除以0
				return _Minimum;
			else
				return MyMethods.LinConvert(position, ThumbFrontPositionMax, ThumbFrontPositionMin, _Maximum, _Minimum);
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

			Width = 200;
			Height = 10;
			ThumbLength = Height + 1;

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

		#endregion

		#region Paint Methods
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			// no painting here
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Color backColor, thumbColor, barColor;

			if (Parent != null)
			{
				if (Parent is IMetroControl)
				{
					backColor = MetroPaint.BackColor.Form(Theme);
				}
				else
				{
					backColor = Parent.BackColor;
				}
			}
			else
			{
				backColor = MetroPaint.BackColor.Form(Theme);
			}

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
				_MouseWheelBarPartitions = MyMethods.Clamp(value, int.MaxValue, 1);
			}
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			int v = (int)(e.Delta / 60f * ValueLength / MouseWheelBarPartitions);
			Value = orientation == ScrollBarOrientation.Vertical ? Value - v : Value + v; //Position Changed
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
				var ClickLocation = e.Location;
				ClickPosition = orientation == ScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;
				ThumbFrontPositionOld = ThumbFrontPosition;
				MousePositionMin = ClickPosition - (ThumbFrontPosition - ThumbFrontPositionMin);
				MousePositionMax = ClickPosition + (ThumbEndPositionMax - ThumbEndPosition);
				if (ThumbRectangle.Contains(ClickLocation))
				{
					IsThumbClicked = true;
					Invalidate(ThumbRectangle);
				}
				else
				{
					if (ClickPosition < ThumbFrontPosition)
						Value -= SmallChange; //Position Changed, Refresh
					else
						Value += SmallChange;
				}
			}
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

		protected override void OnMouseEnter(EventArgs e)
		{
			Focus();
			IsHovered = true;
			Invalidate();

			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			IsHovered = false;
			Invalidate();

			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (e.Button == MouseButtons.Left && IsThumbClicked)
			{
				int ValueOld = _Value;
				int MousePosition = orientation == ScrollBarOrientation.Vertical ? e.Location.Y : e.Location.X;
				MousePosition = MyMethods.Clamp(MousePosition, MousePositionMax, MousePositionMin);
				int DeltaPosition = MousePosition - ClickPosition;
				ThumbFrontPosition = ThumbFrontPositionOld + DeltaPosition; //Value Changed
				OnScroll(ScrollEventType.ThumbTrack, ValueOld, _Value, scrollOrientation);
				Invalidate();
			}
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
				_SmallChange = MyMethods.Clamp(value, _LargeChange, 0);
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
				_LargeChange = MyMethods.Clamp(value, ValueLength, _SmallChange);
			}
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

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
					break;
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

			BarLength = MyMethods.Clamp(BarLength, 10000, Thickness + 1);
			ThumbLength = MyMethods.Clamp(ThumbLength, BarLength, Thickness + 1);
			ThumbFrontPosition = MyMethods.Clamp(ThumbFrontPosition, ThumbFrontPositionMax, ThumbFrontPositionMin);

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
