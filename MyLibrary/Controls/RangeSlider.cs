﻿/* RangeSlider user control
 * Extended by Alex from MetroFramework
 */
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;

using MetroFramework.Drawing;
using MetroFramework.Components;
using MetroFramework.Interfaces;
using MetroFramework;

namespace MyLibrary.Controls
{
	[ToolboxBitmap(typeof(TrackBar))] //為工具箱提供點陣圖
	[DefaultEvent("Scroll")]
	public class RangeSlider : Control, IMetroControl //繼承自Control，自己打造TrackBar
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

		[Browsable(false)]
		public MetroStyleManager StyleManager { set; get; } = null;

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
		public event EventHandler Scroll;
		private void OnScroll()
		{
			Scroll?.Invoke(this, EventArgs.Empty);
		}
		#endregion

		#region Private Properties
		private int orientWidth { get { return (Orientation == SliderOrientation.Horizontal) ? Width : Height; } }
		private int orientHeight { get { return (Orientation == SliderOrientation.Horizontal) ? Height: Width; } }

		private int thumbRadius { get { return MyMethods.Clamp(orientHeight * (6.5f / 20f), int.MaxValue, 3); } }
		private int thumbEdgeWidth { get { return MyMethods.Clamp(orientHeight * (2f / 20f), int.MaxValue, 1); } }
		private int barY { get { return orientHeight / 2; } }
		private int barHeightY { get { return MyMethods.Clamp(orientHeight * (8f / 20f), int.MaxValue, 4); } }
		private float fontSize { get { return thumbRadius * 0.7f; } }
		
		private int offsetMaxMinX { get { return (!Reverse) ? thumbRadius * 2 : -thumbRadius * 2; } }
		private int offsetMaxMin { get { return (int)(offsetMaxMinX * ratioAxPixel); } }
		private int offsetBoundaryX { get { return thumbRadius + thumbEdgeWidth; } }

		private int barMaxX { get { return (!Reverse) ? orientWidth - offsetBoundaryX : offsetBoundaryX; } }
		private int barMinX { get { return (!Reverse) ? offsetBoundaryX : orientWidth - offsetBoundaryX; } }
		private int barWidth { get { return BarMax - BarMin; } }
		private int barWidthX { get { return barMaxX - barMinX; } }

		private float ratioAxPixel { get { return (float)barWidth / barWidthX; } }
		private float ratioPixelAx { get { return (float)barWidthX / barWidth; } }

		private int rangeMaxX { get { return ax2pixel(RangeMax); } }
		private int rangeMinX { get { return ax2pixel(RangeMin); } }
		private int rangeWidth { get { return RangeMax - RangeMin; } }
		private int rangeWidthX { get { return rangeMaxX - rangeMinX; } }

		private bool isPressed = false;
		private bool isFocused = false;

		private string selectOn;
		private int pressX;
		private int mouseX;
		private int disMinMin;
		private int disMaxMax;
		private int disMinPress;
		private int disMaxPress;

		#endregion

		#region Fields

		private SliderOrientation orientation = SliderOrientation.Horizontal;
		[Category("Metro Appearance")]
		[Description("滑桿軸的方向")]
		public SliderOrientation Orientation
		{
			get { return orientation; }
			set
			{
				if (value != orientation)
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
		[Description("滑桿範圍之最小值")]
		public int RangeMin { get; set; } = 50;
		[Category("Metro Data")]
		[Description("滑桿範圍之最大值")]
		public int RangeMax { get; set; } = 200;
		[Category("Metro Data")]
		[Description("滑桿軸之最小值")]
		public int BarMin { get; set; } = 0;
		[Category("Metro Data")]
		[Description("滑桿軸之最大值")]
		public int BarMax { get; set; } = 255;
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

		public RangeSlider()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
							 ControlStyles.OptimizedDoubleBuffer |
							 ControlStyles.ResizeRedraw |
							 ControlStyles.Selectable |
							 ControlStyles.SupportsTransparentBackColor |
							 ControlStyles.UserMouse |
							 ControlStyles.UserPaint, true);

			BackColor = Color.Transparent;
		}

		#endregion

		#region Paint Methods

		protected override void OnPaint(PaintEventArgs e)
		{
			Color backColor, thumbColor, barColor, foreColor;

			if (CustomBackground)
				backColor = BackColor;
			else
				backColor = MetroPaint.BackColor.Form(Theme);

			if (!Enabled)
			{
				thumbColor = MetroPaint.BackColor.RangeSlider.Thumb.Disabled(Theme);
				barColor = MetroPaint.BackColor.RangeSlider.Bar.Disabled(Theme);
				foreColor = MetroPaint.ForeColor.RangeSlider.Disabled(Theme);
			}
			else if (isFocused)
			{
				thumbColor = MetroPaint.BackColor.RangeSlider.Thumb.Focused(Theme);
				barColor = MetroPaint.BackColor.RangeSlider.Bar.Focused(Theme);
				foreColor = MetroPaint.ForeColor.RangeSlider.Focused(Theme);
			} 
			else
			{
				thumbColor = MetroPaint.BackColor.RangeSlider.Thumb.Normal(Theme);
				barColor = MetroPaint.BackColor.RangeSlider.Bar.Normal(Theme);
				foreColor = MetroPaint.ForeColor.RangeSlider.Normal(Theme);
			}
			e.Graphics.Clear(backColor); // 清除整個繪圖介面，並使用指定的背景色彩填滿它。
			DrawRangeSlider(e.Graphics, barColor, thumbColor, foreColor);

			if (false && isFocused)
				ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
		}

		private void DrawRangeSlider(Graphics g, Color barColor, Color thumbColor, Color foreColor)
		{
			SolidBrush thumbBrushInner = MetroPaint.GetStyleBrush(Style);
			Pen thumbPen = new Pen(thumbColor) { Width = thumbEdgeWidth };
			Pen barPen = new Pen(barColor) { StartCap = LineCap.Round, EndCap = LineCap.Round, Width = barHeightY };
			Pen rangePen = new Pen(MetroPaint.GetStyleColor(Style)) { Width = barHeightY };
			Font font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

			Point barLPtL, barLPtR, barRPtL, barRPtR, rangePtL, rangePtR;
			Rectangle rangeMinRect, rangeMaxRect;
			if (Orientation == SliderOrientation.Horizontal)
			{
				barLPtL = new Point(barMinX, barY);
				barLPtR = new Point(rangeMinX, barY);
				barRPtL = new Point(rangeMaxX, barY);
				barRPtR = new Point(barMaxX, barY);
				rangePtL = new Point(rangeMinX, barY);
				rangePtR = new Point(rangeMaxX, barY);
				rangeMinRect = new Rectangle(rangeMinX - thumbRadius, barY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
				rangeMaxRect = new Rectangle(rangeMaxX - thumbRadius, barY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
			}
			else
			{
				barLPtL = new Point(barY, barMinX);
				barLPtR = new Point(barY, rangeMinX);
				barRPtL = new Point(barY, rangeMaxX);
				barRPtR = new Point(barY, barMaxX);
				rangePtL = new Point(barY, rangeMinX);
				rangePtR = new Point(barY, rangeMaxX);
				rangeMinRect = new Rectangle(barY - thumbRadius, rangeMinX - thumbRadius, thumbRadius * 2, thumbRadius * 2);
				rangeMaxRect = new Rectangle(barY - thumbRadius, rangeMaxX - thumbRadius, thumbRadius * 2, thumbRadius * 2);
			}
			g.DrawLine(barPen, barLPtL, barLPtR);
			g.DrawLine(barPen, barRPtL, barRPtR);
			g.DrawLine(rangePen, rangePtL, rangePtR);
			g.DrawEllipse(thumbPen, rangeMinRect);
			g.FillEllipse(thumbBrushInner, rangeMinRect);
			g.DrawEllipse(thumbPen, rangeMaxRect);
			g.FillEllipse(thumbBrushInner, rangeMaxRect);
			TextRenderer.DrawText(g, " " + RangeMin.ToString(), font, rangeMinRect, foreColor, Color.Transparent, MetroPaint.GetTextFormatFlags(ContentAlignment.MiddleCenter));
			TextRenderer.DrawText(g, " " + RangeMax.ToString(), font, rangeMaxRect, foreColor, Color.Transparent, MetroPaint.GetTextFormatFlags(ContentAlignment.MiddleCenter));

		}

		#endregion

		#region Focus Methods

		protected override void OnGotFocus(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnGotFocus(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			isFocused = false;
			isFocused = false;
			isPressed = false;
			Invalidate();

			base.OnLostFocus(e);
		}

		protected override void OnEnter(EventArgs e)
		{
			isFocused = true;
			Invalidate();

			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			isFocused = false;
			isFocused = false;
			isPressed = false;
			Invalidate();

			base.OnLeave(e);
		}

		#endregion

		#region Keyboard Methods

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!isFocused || selectOn == null) return;

			int delta = 0;
			switch (e.KeyCode)
			{
				case Keys.Up:
					delta = (Orientation == SliderOrientation.Horizontal) ? 0 : -(int)ArrowChange;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Down:
					delta = (Orientation == SliderOrientation.Horizontal) ? 0 : +(int)ArrowChange;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Left:
					delta = (Orientation == SliderOrientation.Horizontal) ? -(int)ArrowChange : 0;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Right:
					delta = (Orientation == SliderOrientation.Horizontal) ? +(int)ArrowChange : 0;
					delta = (!Reverse) ? delta : -delta;
					break;
				case Keys.Home:
					delta = -barWidth;
					break;
				case Keys.End:
					delta = barWidth;
					break;
				case Keys.PageDown:
					delta = -(int)PageChange;
					break;
				case Keys.PageUp:
					delta = +(int)PageChange;
					break;
			}
			switch (selectOn)
			{
				case ("Max"):
					RangeMax = MyMethods.Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
					break;
				case ("Min"):
					RangeMin = MyMethods.Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
					break;
				case ("range"):
					RangeMax = MyMethods.Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
					RangeMin = MyMethods.Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
					break;
			}

			OnValueChanged();
			OnScroll();

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

		#region Mouse Methods

		private void Mouse2Value(MouseEventArgs e)
		{
			mouseX = (Orientation == SliderOrientation.Horizontal) ? e.Location.X : e.Location.Y;
			switch (selectOn)
			{
				case ("Max"):
					mouseX = MyMethods.Clamp(mouseX, barMaxX, rangeMinX + offsetMaxMinX);
					RangeMax = pixel2ax(mouseX);
					break;
				case ("Min"):
					mouseX = MyMethods.Clamp(mouseX, rangeMaxX - offsetMaxMinX, barMinX);
					RangeMin = pixel2ax(mouseX);
					break;
				case ("range"):
					mouseX = MyMethods.Clamp(mouseX, pressX + disMaxMax, pressX - disMinMin);
					RangeMin = pixel2ax(mouseX - disMinPress);
					RangeMax = pixel2ax(mouseX + disMaxPress);
					break;
				default:
					break;
			}
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			Focus();
			isFocused = true;
			Invalidate();

			base.OnMouseEnter(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (e.Button != MouseButtons.Left) return;

			isPressed = true;

			pressX = (Orientation == SliderOrientation.Horizontal) ? e.Location.X : e.Location.Y;
			disMinMin = rangeMinX - barMinX; 
		  disMaxMax = barMaxX - rangeMaxX; 
		  disMinPress = pressX - rangeMinX; 
		  disMaxPress = rangeMaxX - pressX;

			if (Math.Abs(disMaxPress) < thumbRadius)
				selectOn = "Max";
			else if (Math.Abs(disMinPress) < thumbRadius)
				selectOn = "Min";
			else if (MyMethods.IsIn<int>(pressX, rangeMaxX, rangeMinX)) 
				selectOn = "range";
			else
				selectOn = null;

			Mouse2Value(e);

			//OnScroll();
			Invalidate();
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (!(isPressed && e.Button == MouseButtons.Left))
				return;

			Mouse2Value(e);

			OnScroll();
			Invalidate(); //更新畫面
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			isPressed = false;

			OnValueChanged();
			//OnScroll();
			Invalidate();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			isFocused = false;
			Invalidate();

			base.OnMouseLeave(e);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (!isFocused || selectOn == null) return;

			int delta = (int)(e.Delta / Math.Abs(e.Delta) * ScrollChange);
			switch (selectOn)
			{
				case ("Max"):
					RangeMax = MyMethods.Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
					break;
				case ("Min"):
					RangeMin = MyMethods.Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
					break;
				case ("range"):
					RangeMax = MyMethods.Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
					RangeMin = MyMethods.Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
					break;
			}

			OnValueChanged();
			OnScroll();
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
			return (int)Math.Round((pixel - barMinX) * ratioAxPixel + BarMin);
		}

		private int ax2pixel(int ax)
		{
			return (int)Math.Round((ax - BarMin) * ratioPixelAx + barMinX);
		}

		#endregion
	}
}