using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Windows.Forms;
using static MyLibrary.MyMethods;

namespace MyLibrary.Controls
{
  [DefaultEvent("Scroll")]
  public class RangeSlider : Control
  {
    #region Events
    [Category("Events")]
    [Description("滑桿數值變更時觸發")]
    public event EventHandler ValueChanged;
    private void OnValueChanged()
    {
      ValueChanged?.Invoke(this, EventArgs.Empty);
    }
    [Category("Events")]
    [Description("滑桿移動時觸發")]
    public event EventHandler Scroll;
    private void OnScroll()
    {
      Scroll?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Private Properties
    private int orientWidth => (Orientation == RangeSliderOrientation.Horizontal) ? Width : Height;
    private int orientHeight => (Orientation == RangeSliderOrientation.Horizontal) ? Height : Width;

    private int thumbRadius => Clamp(orientHeight * (6.5f / 20f), int.MaxValue, 3);
    private int thumbEdgeWidth => Clamp(orientHeight * (2.5f / 20f), int.MaxValue, 1);
    private int barY => orientHeight / 2;
    private int barHeightY => Clamp(orientHeight * (8f / 20f), int.MaxValue, 4);
    private float fontSize => thumbRadius * 0.7f;

    private int offsetMaxMinX => (!Reverse) ? thumbRadius * 2 : -thumbRadius * 2;
    private int offsetMaxMin => (int)(offsetMaxMinX * ratioAxPixel);
    private int offsetBoundaryX => thumbRadius + thumbEdgeWidth;

    private int barMaxX => (!Reverse) ? orientWidth - offsetBoundaryX : offsetBoundaryX;
    private int barMinX => (!Reverse) ? offsetBoundaryX : orientWidth - offsetBoundaryX;
    private int barWidth => BarMax - BarMin;
    private int barWidthX => barMaxX - barMinX;

    private float ratioAxPixel => (float)barWidth / barWidthX;
    private float ratioPixelAx => (float)barWidthX / barWidth;

    private int rangeMaxX => ax2pixel(RangeMax);
    private int rangeMinX => ax2pixel(RangeMin);
    private int rangeWidth => RangeMax - RangeMin;
    private int rangeWidthX => rangeMaxX - rangeMinX;

    private bool IsPressed = false;
    private bool IsHover = false;

    private string selectOn;
    private int pressX;
    private int mouseX;
    private int disMinMin;
    private int disMaxMax;
    private int disMinPress;
    private int disMaxPress;

    #endregion

    #region Fields

    private RangeSliderOrientation orientation = RangeSliderOrientation.Horizontal;
    [Category("Appearance")]
    [Description("滑桿軸的方向")]
    public RangeSliderOrientation Orientation
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
    [Category("Appearance")]
    [Description("是否使用自訂背景")]
    public bool CustomBackground { get; set; } = false;
    [Category("Appearance")]
    [Description("是否使滑桿數線方向相反(預設是由上至下或由左至右)")]
    public bool Reverse { get; set; } = false;

    [Category("Data")]
    [Description("滑桿範圍之最小值")]
    public int RangeMin { get; set; } = 50;
    [Category("Data")]
    [Description("滑桿範圍之最大值")]
    public int RangeMax { get; set; } = 200;
    [Category("Data")]
    [Description("滑桿軸之最小值")]
    public int BarMin { get; set; } = 0;
    [Category("Data")]
    [Description("滑桿軸之最大值")]
    public int BarMax { get; set; } = 255;
    [Category("Data")]
    [Description("按一下方向鍵使滑桿移動之數值")]
    public uint ArrowChange { get; set; } = 1;
    [Category("Data")]
    [Description("按一下PageUp/PageDown使滑桿移動之數值")]
    public uint PageChange { get; set; } = 5;
    [Category("Data")]
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
    [Category("Appearance")]
    public Color Color { get; set; } = Color.FromKnownColor(KnownColor.MenuHighlight);

    private Color backColor => (CustomBackground) ? BackColor : Parent.BackColor;
    private Color thumbColor;
    private Color barColor;
    private Color textColor;
    private sealed class Colors
    {
      private static int ThumbGrayNormal = 120;
      private static int ThumbGrayHover = ThumbGrayNormal - 40;
      private static int ThumbGrayPressed = ThumbGrayHover - 60;
      private static int BarGrayNormal = ThumbGrayNormal + 50;
      private static int BarGrayHover = ThumbGrayHover + 50;
      private static int BarGrayPressed = BarGrayHover;
      public sealed class Thumb
      {
        public static Color Normal = Color.FromArgb(ThumbGrayNormal, ThumbGrayNormal, ThumbGrayNormal);
        public static Color Hover = Color.FromArgb(ThumbGrayHover, ThumbGrayHover, ThumbGrayHover);
        public static Color Pressed = Color.FromArgb(ThumbGrayPressed, ThumbGrayPressed, ThumbGrayPressed);
        public static Color Disabled = Color.FromArgb(221, 221, 221);
      }
      public sealed class Bar
      {
        public static Color Normal = Color.FromArgb(BarGrayNormal, BarGrayNormal, BarGrayNormal);
        public static Color Hover = Color.FromArgb(BarGrayHover, BarGrayHover, BarGrayHover);
        public static Color Pressed = Color.FromArgb(BarGrayPressed, BarGrayPressed, BarGrayPressed);
        public static Color Disabled = Color.FromArgb(BarGrayNormal, BarGrayNormal, BarGrayNormal);
      }
      public sealed class Text
      {
        public static Color Normal = Color.FromArgb(0, 0, 0);
        public static Color Hover = Color.FromArgb(255, 255, 255);
        public static Color Pressed = Color.FromArgb(255, 255, 255);
        public static Color Disabled = Color.FromArgb(136, 136, 136);
      }
    }
    protected override void OnPaint(PaintEventArgs e)
    {
      if (!Enabled)
      {
        thumbColor = Colors.Thumb.Disabled;
        barColor = Colors.Bar.Disabled;
        textColor = Colors.Text.Disabled;
      }
      else if (IsPressed)
      {
        thumbColor = Colors.Thumb.Pressed;
        barColor = Colors.Bar.Pressed;
        textColor = Colors.Text.Pressed;
      }
      else if (IsHover)
      {
        thumbColor = Colors.Thumb.Hover;
        barColor = Colors.Bar.Hover;
        textColor = Colors.Text.Hover;
      }
      else
      {
        thumbColor = Colors.Thumb.Normal;
        barColor = Colors.Bar.Normal;
        textColor = Colors.Text.Normal;
      }

      if (HoverTimer.Enabled)
      {
        barColor = Interpolate(Colors.Bar.Normal, Colors.Bar.Hover, HoverRatio);
        thumbColor = Interpolate(Colors.Thumb.Normal, Colors.Thumb.Hover, HoverRatio);
        textColor = Interpolate(Colors.Text.Normal, Colors.Text.Hover, HoverRatio);
      }

      e.Graphics.Clear(backColor);
      DrawRangeSlider(e.Graphics, barColor, thumbColor, textColor);

      if (false && IsHover)
        ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
    }

    private void DrawRangeSlider(Graphics g, Color barColor, Color thumbColor, Color textColor)
    {
      SolidBrush thumbBrushInner = new SolidBrush(Color);
      Pen thumbPen = new Pen(thumbColor) { Width = thumbEdgeWidth };
      Pen barPen = new Pen(barColor) { StartCap = LineCap.Round, EndCap = LineCap.Round, Width = barHeightY };
      Pen rangePen = new Pen(Color) { Width = barHeightY };
      Font font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel);

      Point barLPtL, barLPtR, barRPtL, barRPtR, rangePtL, rangePtR;
      Rectangle rangeMinRect, rangeMaxRect;
      if (Orientation == RangeSliderOrientation.Horizontal)
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
      TextRenderer.DrawText(g, " " + RangeMin.ToString(), font, rangeMinRect, textColor, Color.Transparent, MyMethods.GetTextFormatFlags(ContentAlignment.MiddleCenter));
      TextRenderer.DrawText(g, " " + RangeMax.ToString(), font, rangeMaxRect, textColor, Color.Transparent, MyMethods.GetTextFormatFlags(ContentAlignment.MiddleCenter));
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
      if (!IsHover || selectOn == null) return;

      int delta = 0;
      switch (e.KeyCode)
      {
        case Keys.Up:
          delta = (Orientation == RangeSliderOrientation.Horizontal) ? 0 : -(int)ArrowChange;
          delta = (!Reverse) ? delta : -delta;
          break;
        case Keys.Down:
          delta = (Orientation == RangeSliderOrientation.Horizontal) ? 0 : +(int)ArrowChange;
          delta = (!Reverse) ? delta : -delta;
          break;
        case Keys.Left:
          delta = (Orientation == RangeSliderOrientation.Horizontal) ? -(int)ArrowChange : 0;
          delta = (!Reverse) ? delta : -delta;
          break;
        case Keys.Right:
          delta = (Orientation == RangeSliderOrientation.Horizontal) ? +(int)ArrowChange : 0;
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
          RangeMax = Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
          break;
        case ("Min"):
          RangeMin = Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
          break;
        case ("range"):
          RangeMax = Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
          RangeMin = Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
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
      mouseX = (Orientation == RangeSliderOrientation.Horizontal) ? e.Location.X : e.Location.Y;
      switch (selectOn)
      {
        case ("Max"):
          mouseX = Clamp(mouseX, barMaxX, rangeMinX + offsetMaxMinX);
          RangeMax = pixel2ax(mouseX);
          break;
        case ("Min"):
          mouseX = Clamp(mouseX, rangeMaxX - offsetMaxMinX, barMinX);
          RangeMin = pixel2ax(mouseX);
          break;
        case ("range"):
          mouseX = Clamp(mouseX, pressX + disMaxMax, pressX - disMinMin);
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
      if (e.Button != MouseButtons.Left) return;

      IsPressed = true;

      pressX = (Orientation == RangeSliderOrientation.Horizontal) ? e.Location.X : e.Location.Y;
      disMinMin = rangeMinX - barMinX;
      disMaxMax = barMaxX - rangeMaxX;
      disMinPress = pressX - rangeMinX;
      disMaxPress = rangeMaxX - pressX;

      if (Math.Abs(disMaxPress) < thumbRadius)
        selectOn = "Max";
      else if (Math.Abs(disMinPress) < thumbRadius)
        selectOn = "Min";
      else if (IsIn(pressX, rangeMaxX, rangeMinX))
        selectOn = "range";
      else
        selectOn = null;

      Mouse2Value(e);

      //OnScroll();
      HoverTimer.Stop();
      Refresh();
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
      base.OnMouseUp(e);
      IsPressed = false;

      OnValueChanged();
      //OnScroll();
      Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      if (!(IsPressed && e.Button == MouseButtons.Left))
        return;

      Mouse2Value(e);

      OnScroll();
      Invalidate(); //更新畫面
    }
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      base.OnMouseWheel(e);
      if (!IsHover || selectOn == null) return;

      int delta = (int)(e.Delta / Math.Abs(e.Delta) * ScrollChange);
      switch (selectOn)
      {
        case ("Max"):
          RangeMax = Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
          break;
        case ("Min"):
          RangeMin = Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
          break;
        case ("range"):
          RangeMax = Clamp(RangeMax + delta, BarMax, RangeMin + offsetMaxMin);
          RangeMin = Clamp(RangeMin + delta, RangeMax - offsetMaxMin, BarMin);
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
