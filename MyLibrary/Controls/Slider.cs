using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using MetroFramework;
using MetroFramework.Drawing;
using MetroFramework.Components;
using MetroFramework.Interfaces;
using System.Drawing.Drawing2D;
using static MyLibrary.MyMethods;

namespace MyLibrary.Controls
{
	[DefaultEvent("Scroll")]
	public class Slider : Control, IMetroControl //繼承自Control，自己打造Slider
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
		[Category("Metro Events")]
		[Description("滑桿數值變更時觸發")]
		public event EventHandler ValueChanged; 
		private void OnValueChanged()
		{
			ValueChanged?.Invoke(this, EventArgs.Empty);
		}
		[Category("Metro Events")]
		[Description("滑桿移動時觸發")]	
		public event ScrollEventHandler Scroll; 
		private void OnScroll(int newValue)
		{
			Scroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, newValue));
		}
		#endregion

		#region Private Properties
		private int OrientWidth
		{
			get
			{
				return (orientation == SliderOrientation.Down ||
					orientation == SliderOrientation.Up) ? Width : Height;
			}
		}
		private int OrientHeight
		{
			get
			{
				return (orientation == SliderOrientation.Down ||
					orientation == SliderOrientation.Up) ? Height : Width;
			}
		}

		private int ThumbEdgeWidth { get { return Clamp(OrientHeight * (2f / 20f), int.MaxValue, 1); } }
		private int ThumbHeight { get { return Clamp(OrientHeight * (10f / 20f), int.MaxValue, 6); } }
		private int ThumbWidth { get { return ThumbHeight; } }
		private int BarHeightY { get { return Clamp(OrientHeight * (5f / 20f), int.MaxValue, 4); } }
		private int BarY { get { return (int)(OrientHeight * 0.5); } }
		private float FontSize { get { return ThumbWidth * 0.4f; } }

		private float RatioAxPixel { get { return (float)BarWidth / BarWidthX; } }
		private float RatioPixelAx { get { return (float)BarWidthX / BarWidth; } }
		private int OffsetBoundaryX { get { return ThumbWidth / 2 + ThumbEdgeWidth; } }

		private int BarMaxX { get { return (!Reverse) ? OrientWidth - OffsetBoundaryX : OffsetBoundaryX; } }
		private int BarMinX { get { return (!Reverse) ? OffsetBoundaryX : OrientWidth - OffsetBoundaryX; } }
		private int BarWidth { get { return BarMax - BarMin; } }
		private int BarWidthX { get { return BarMaxX - BarMinX; } }

		private int ValueX { get { return ax2pixel(Value); } }
		
		//以左上角點開始 順時針排序
		private int[] ThumbXs
		{
			get
			{
				switch (orientation)
				{
					case SliderOrientation.Down:
						return new int[] { -1, 1, 1, 0, -1 };
					case SliderOrientation.Right:
						return new int[] { -1, 1, 2, 1, -1 };
					case SliderOrientation.Up:
						return new int[] { -1, 0, 1, 1, -1 };
					case SliderOrientation.Left:
						return new int[] { -1, 1, 1, -1, -2 };
					default:
						return null;
				}
			}
		}
		private int[] ThumbYs
		{
			get
			{
				switch (orientation)
				{
					case SliderOrientation.Down:
						return new int[] { -1, -1, 1, 2, 1 };
					case SliderOrientation.Right:
						return new int[] { -1, -1, 0, 1, 1 };
					case SliderOrientation.Up:
						return new int[] { -1, -2, -1, 1, 1 };
					case SliderOrientation.Left:
						return new int[] { -1, -1, 1, 1, 0 };
					default:
						return null;
				}
			}
		}

		private bool IsHover = false;
		private bool IsPressed = false;
		private int MouseX;

		#endregion

		#region Fields

		private SliderOrientation orientation = SliderOrientation.Down;
		[Category("Metro Appearance")]
		[Description("滑桿軸的方向")]
		public SliderOrientation Orientation
		{
			get { return orientation; }
			set
			{
				if (((orientation == SliderOrientation.Up || orientation == SliderOrientation.Down) &&
					(value == SliderOrientation.Right || value == SliderOrientation.Left)) ||
					((orientation == SliderOrientation.Right || orientation == SliderOrientation.Left) &&
					(value == SliderOrientation.Down || value == SliderOrientation.Up)))
				{
					int tmp = Width;
					Width = Height;
					Height = tmp;
				}
				orientation = value;
			}
		}
		[Category("Metro Appearance")]
		[Description("是否使用自訂背景")]
		public bool CustomBackground { get; set; } = false;
		[Category("Metro Appearance")]
		[Description("是否使滑桿數線方向相反(預設是由上至下或由左至右)")]
		public bool Reverse { get; set; } = false;

		[Category("Metro Data")]
		[Description("滑桿之數值")]
		public int Value { get; set; } = 25;
		[Category("Metro Data")]
		[Description("滑桿軸之最小值")]
		public int BarMin { get; set; } = 0;
		[Category("Metro Data")]
		[Description("滑桿軸之最大值")]
		public int BarMax { get; set; } = 100;
		[Category("Metro Data")]
		[Description("按一下方向鍵使滑桿移動之數值")]
		public uint ArrowChange { get; set; } = 1;
		[Category("Metro Data")]
		[Description("按一下PageUp/PageDown使滑桿移動之數值")]
		public uint PageChange { get; set; } = 5;
		[Category("Metro Data")]
		[Description("操作滑鼠滾輪使滑桿移動之數值")]
		public uint ScrollChange { get; set; } = 10;

		#endregion
		
		#region Constructor

		public Slider()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
							 ControlStyles.OptimizedDoubleBuffer |
							 ControlStyles.ResizeRedraw |
							 ControlStyles.Selectable |
							 ControlStyles.SupportsTransparentBackColor |
							 ControlStyles.UserMouse |
							 ControlStyles.UserPaint, true);

			BackColor = Color.Transparent;
			SetProperties();
		}

		private void SetProperties()
		{
			HoverTimer = new Timer();
			HoverTimer.Tick += HoverTimer_Tick;
			HoverTimer.Interval = 10;
		}

		#endregion

		#region Paint Methods
		private Color backColor => (CustomBackground) ? BackColor : MetroPaint.BackColor.Form(Theme);
		private Color thumbColor;
		private Color barColor;
		private Color foreColor;
		protected override void OnPaint(PaintEventArgs e)
		{
			if (!Enabled)
			{
				thumbColor = MetroPaint.BackColor.Slider.Thumb.Disabled(Theme);
				barColor = MetroPaint.BackColor.Slider.Bar.Disabled(Theme);
				foreColor = MetroPaint.ForeColor.Slider.Disabled(Theme);
			}
			else if (IsHover)
			{
				thumbColor = MetroPaint.BackColor.Slider.Thumb.Hover(Theme);
				barColor = MetroPaint.BackColor.Slider.Bar.Hover(Theme);
				foreColor = MetroPaint.ForeColor.Slider.Hover(Theme);
			}
			else if (IsPressed)
			{
				thumbColor = MetroPaint.BackColor.Slider.Thumb.Pressed(Theme);
				barColor = MetroPaint.BackColor.Slider.Bar.Pressed(Theme);
				foreColor = MetroPaint.ForeColor.Slider.Pressed(Theme);
			}
			else
			{
				thumbColor = MetroPaint.BackColor.Slider.Thumb.Normal(Theme);
				barColor = MetroPaint.BackColor.Slider.Bar.Normal(Theme);
				foreColor = MetroPaint.ForeColor.Slider.Normal(Theme);
			}

			if (HoverTimer.Enabled)
			{
				barColor = Interpolate(MetroPaint.BackColor.Slider.Bar.Normal(Theme), MetroPaint.BackColor.Slider.Bar.Hover(Theme), HoverRatio);
				thumbColor = Interpolate(MetroPaint.BackColor.Slider.Thumb.Normal(Theme), MetroPaint.BackColor.Slider.Thumb.Hover(Theme), HoverRatio);
				foreColor = Interpolate(MetroPaint.ForeColor.Slider.Normal(Theme), MetroPaint.ForeColor.Slider.Hover(Theme), HoverRatio);
			}

			e.Graphics.Clear(backColor);
			DrawSlider(e.Graphics, thumbColor, barColor, foreColor);

			if (false && IsHover)
				ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
		}
		
		private void DrawSlider(Graphics g, Color thumbColor, Color barColor, Color foreColor)
		{
			SolidBrush thumbBrush = MetroPaint.GetStyleBrush(Style);
			Pen thumbPen = new Pen(thumbColor) { Width = ThumbEdgeWidth };
			Pen barLPen = new Pen(MetroPaint.GetStyleColor(Style)) { StartCap = LineCap.Round, EndCap = LineCap.Round, Width = BarHeightY };
			Pen barRPen = new Pen(barColor) { StartCap = LineCap.Round, EndCap = LineCap.Round, Width = BarHeightY };
			Font font = new Font("Segoe UI", FontSize, FontStyle.Bold, GraphicsUnit.Pixel);

			Point barLPtL, barLPtR, barRPtL, barRPtR;
			Point[] thumbPts;
			Rectangle txtRect;
			int thumbY;
			if (orientation == SliderOrientation.Down || orientation == SliderOrientation.Right)
				thumbY = (int)(BarY * 0.7);
			else
				thumbY = (int)(BarY * 1.3);
			if (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up)
			{
				barLPtL = new Point(BarMinX, BarY);
				barLPtR = new Point(ValueX, BarY);
				barRPtL = new Point(ValueX, BarY);
				barRPtR = new Point(BarMaxX, BarY);
				
				thumbPts = getThumbPts(ValueX, thumbY, ThumbWidth / 2, ThumbHeight / 2);
			}
			else
			{
				barLPtL = new Point(BarY, BarMinX);
				barLPtR = new Point(BarY, ValueX);
				barRPtL = new Point(BarY, ValueX);
				barRPtR = new Point(BarY, BarMaxX);
				
				thumbPts = getThumbPts(thumbY, ValueX, ThumbWidth / 2, ThumbHeight / 2);
			}
			g.DrawLine(barLPen, barLPtL, barLPtR);
			g.DrawLine(barRPen, barRPtL, barRPtR);
			g.DrawPolygon(thumbPen, thumbPts);
			g.FillPolygon(thumbBrush, thumbPts);
			txtRect = new Rectangle(thumbPts[0].X, thumbPts[0].Y, ThumbWidth, ThumbHeight);
			TextRenderer.DrawText(g, Value.ToString(), font, txtRect, foreColor, Color.Transparent, MetroPaint.GetTextFormatFlags(ContentAlignment.MiddleCenter));
		}

		#endregion

		#region Animation
		private Timer HoverTimer;
		private float HoverRatio; // 0~1
		private bool IsIncreasing;
		private void HoverTimer_Tick(object sender, EventArgs e)
		{
			HoverRatio = (IsIncreasing) ? HoverRatio + 0.05f : HoverRatio - 0.05f;
			if (!IsIn(HoverRatio, 1, 0))
			{
				HoverTimer.Stop();
			}
			Refresh();
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

		#region Keyboard Methods

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!IsHover) return;

			int delta = 0;
			switch (e.KeyCode)
			{
				case Keys.Up:
					delta = (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up) ? 0 : -(int)ArrowChange;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Down:
					delta = (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up) ? 0 : (int)ArrowChange;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Left:
					delta = (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up) ? (int)-ArrowChange : 0;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Right:
					delta = (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up) ? (int)ArrowChange : 0;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Home:
					delta = -BarWidth;
					break;
				case Keys.End:
					delta = BarWidth;
					break;
				case Keys.PageDown:
					delta = -(int)PageChange;
					break;
				case Keys.PageUp:
					delta = +(int)PageChange;
					break;
			}
			Value = Clamp(Value + delta, BarMax, BarMin);

			OnValueChanged();
			OnScroll(Value);

			Invalidate();
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if (keyData == Keys.Tab | ModifierKeys == Keys.Shift)
				return base.ProcessDialogKey(keyData);
			else
			{
				OnKeyUp(new KeyEventArgs(keyData));
				return true;
			}
		}

		#endregion

		#region Mouse Methods

		private void Mouse2Value(MouseEventArgs e)
		{
			MouseX = (orientation == SliderOrientation.Down || orientation == SliderOrientation.Up) ? e.Location.X : e.Location.Y;
			MouseX = Clamp(MouseX, BarMaxX, BarMinX);
			Value = pixel2ax(MouseX);
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			Focus();
			IsHover = true;
			IsIncreasing = true;
			HoverRatio = 0;
			HoverTimer.Start();
			Invalidate();

			base.OnMouseEnter(e);
		}
		protected override void OnMouseLeave(EventArgs e)
		{
			IsHover = false;

			IsIncreasing = false;
			if (!HoverTimer.Enabled)
			{
				HoverRatio = 1;
				HoverTimer.Start();
			}
			Invalidate();

			base.OnMouseLeave(e);
		}


		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left) { return; }
			IsPressed = true;
			Mouse2Value(e);

			OnScroll(Value);
			HoverTimer.Stop();
			Refresh();
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			IsPressed = false;
			base.OnMouseUp(e);
			OnValueChanged();
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!(IsPressed & e.Button == MouseButtons.Left)) return;
			Mouse2Value(e);

			OnScroll(Value);
			Invalidate();
		}


		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (!IsHover) return;

			int delta = (int)(e.Delta / Math.Abs(e.Delta) * ScrollChange);
			Value = Clamp(Value + delta, BarMax, BarMin);

			OnValueChanged();
			OnScroll(Value);
			Invalidate(); //更新畫面
		}
		#endregion

		#region Overridden Methods

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			Invalidate();
		}

		#endregion

		#region Helper Methods

		private int pixel2ax(int pixel)
		{
			return (int)((pixel - BarMinX) * RatioAxPixel + BarMin);
		}

		private int ax2pixel(int ax)
		{
			return (int)((ax - BarMin) * RatioPixelAx + BarMinX);
		}

		private Point[] getThumbPts(int x, int y, int scaleX, int scaleY)
		{
			int[] xs = ThumbXs;
			int[] ys = ThumbYs;
			Point[] thumbPts = new Point[5];

			for (int i = 0; i < 5; i++)
			{
				xs[i] *= scaleX;
				xs[i] += x;
				ys[i] *= scaleY;
				ys[i] += y;
				thumbPts[i] = new Point(xs[i], ys[i]);
			}
			return thumbPts;
		}

		#endregion
	}
}
