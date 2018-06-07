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
    private HVOrientation orientation = HVOrientation.Horizontal;
    [Category("Appearance")]
    public HVOrientation Orientation
    {
      get { return orientation; }
      set
      {
        if (value == orientation)
          return;

        orientation = value;

        if (value == HVOrientation.Vertical)
          scrollOrientation = ScrollOrientation.VerticalScroll;
        else
          scrollOrientation = ScrollOrientation.HorizontalScroll;

        this.Size = new Size(Height, Width);
        Refresh();
      }
    }

    [Category("Appearance")]
    public bool UseBarColor { get; set; } = true;

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
    public int Thickness => Orientation == HVOrientation.Vertical ? Width : Height;
    [Category("Appearance")]
    public int BarLength
    {
      get => orientation == HVOrientation.Vertical ? Height : Width;
      set
      {
        if (orientation == HVOrientation.Vertical)
          Height = value;
        else
          Width = value;
      }
    }

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
    private int ValueLength => _Maximum - _Minimum;

    private int _ThumbFrontPosition;
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
    private int ThumbEndPosition => ThumbFrontPosition + ThumbLength;
    private int ThumbEndPositionMax => orientation == HVOrientation.Vertical ?
        ClientRectangle.Bottom : ClientRectangle.Right;
    private int ThumbFrontPositionMax => ThumbEndPositionMax - ThumbLength;
    private int ThumbFrontPositionMin = 0;
    private int ThumbPositionLength => ThumbFrontPositionMax - ThumbFrontPositionMin;
    private Rectangle ThumbRectangle => orientation == HVOrientation.Vertical ?
        new Rectangle(0, ThumbFrontPosition, Thickness, ThumbLength) :
        new Rectangle(ThumbFrontPosition, 0, ThumbLength, Thickness);


    private int Value2Position(int value)
    {
      return LinConvert(value, _Maximum, _Minimum, ThumbFrontPositionMax, ThumbFrontPositionMin);
    }
    private int Position2Value(int position)
    {
      if (position == 0) // 避免除以0
        return _Minimum;
      else
        return LinConvert(position, ThumbFrontPositionMax, ThumbFrontPositionMin, _Maximum, _Minimum);
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
    public ScrollBar(HVOrientation orientation)
        : this()
    {
      Orientation = orientation;
    }
    public ScrollBar(HVOrientation orientation, int width)
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

      MoveTimer = new Timer();
      MoveTimer.Tick += MoveTimer_Tick;
      MoveTimer.Interval = 5;
    }
    #endregion

    #region Paint Methods
    private Color backColor => Parent.BackColor;
    private Color thumbColor;
    private Color barColor;
    protected override void OnPaint(PaintEventArgs e)
    {
      if (IsFocus && !IsPressed && Enabled)
      {
        thumbColor = ShadeColors.Thumb.Focus;
        barColor = ShadeColors.Bar.Focus;
      }
      else if (IsFocus && IsPressed && Enabled)
      {
        thumbColor = ShadeColors.Thumb.Pressed;
        barColor = ShadeColors.Bar.Pressed;
      }
      else if (!Enabled)
      {
        thumbColor = ShadeColors.Thumb.Disabled;
        barColor = ShadeColors.Bar.Disabled;
      }
      else
      {
        thumbColor = ShadeColors.Thumb.Normal;
        barColor = ShadeColors.Bar.Normal;
      }

      if (HoverTimer.Enabled)
      {
        barColor = Interpolate(ShadeColors.Bar.Normal, ShadeColors.Bar.Focus, HoverRatio);
        thumbColor = Interpolate(ShadeColors.Thumb.Normal, ShadeColors.Thumb.Focus, HoverRatio);
      }

      e.Graphics.Clear(backColor);
      DrawScrollBar(e.Graphics, backColor, thumbColor, barColor);
    }

    private Point BarPoint1 => new Point(CapRadius, CapRadius);
    private Point BarPoint2 => orientation == HVOrientation.Vertical ?
      new Point(CapRadius, ClientRectangle.Bottom - CapRadius) :
      new Point(ClientRectangle.Right - CapRadius, CapRadius);
    private Point ThumbPoint1 => orientation == HVOrientation.Vertical ?
      new Point(CapRadius, ThumbFrontPosition + CapRadius) :
      new Point(ThumbFrontPosition + CapRadius, CapRadius);
    private Point ThumbPoint2 => orientation == HVOrientation.Vertical ?
      new Point(CapRadius, ThumbEndPosition - CapRadius) :
      new Point(ThumbEndPosition - CapRadius, CapRadius);
    private void DrawScrollBar(Graphics g, Color backColor, Color thumbColor, Color barColor)
    {
      if (UseBarColor)
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
      if (!IsIn(HoverRatio, 1, 0, true))
      {
        HoverTimer.Stop();
      }
      Refresh();
    }

    private Timer MoveTimer;
    private float MoveRatio; // 0~1
    private int StartValue;
    private int EndValue;
    private void MoveTimer_Tick(object sender, EventArgs e)
    {
      MoveRatio += 0.2f;
      if (!IsIn(MoveRatio, 1, 0, true))
      {
        MoveTimer.Stop();
      }
      Value = (int)Interpolate(StartValue, EndValue, MoveRatio);
      Refresh();
    }
    #endregion

    #region Focus Methods
    private bool IsPressed = false;
    private bool IsFocus = false;
    protected override void OnGotFocus(EventArgs e)
    {
      IsFocus = true;
      IsIncreasing = true;
      HoverRatio = 0;
      HoverTimer.Start();
      base.OnGotFocus(e);
    }
    protected override void OnLostFocus(EventArgs e)
    {
      IsFocus = false;
      IsIncreasing = false;
      HoverRatio = 1;
      HoverTimer.Start();
      base.OnLostFocus(e);
    }
    protected override void OnMouseEnter(EventArgs e)
    {
      if (!IsFocus)
      {
        Focus();
        IsFocus = true;
        IsIncreasing = true;
        HoverRatio = 0;
        HoverTimer.Start();
      }
      base.OnMouseEnter(e);
    }
    #endregion

    #region Mouse Methods
    private int MouseWheelBarPartitions = 20;
    protected override void OnMouseWheel(MouseEventArgs e)
    {
      base.OnMouseWheel(e);
      if (!IsFocus) return;

      int ValueOld = _Value;
      int v = (int)(e.Delta / 60f * ValueLength / MouseWheelBarPartitions);
      Value = orientation == HVOrientation.Vertical ? Value - v : Value + v; //Position Changed
      OnScroll(ScrollEventType.ThumbPosition, ValueOld, _Value, scrollOrientation);
    }

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
        ClickPosition = orientation == HVOrientation.Vertical ? e.Location.Y : e.Location.X;
        ThumbFrontPositionOld = ThumbFrontPosition;
        MousePositionMin = ClickPosition - (ThumbFrontPosition - ThumbFrontPositionMin);
        MousePositionMax = ClickPosition + (ThumbEndPositionMax - ThumbEndPosition);
        if (ThumbRectangle.Contains(ClickLocation))
        {
          IsThumbClicked = true;
        }
        else
        {
          MoveRatio = 0;
          StartValue = _Value;
          if (ClickPosition < ThumbFrontPosition)
            EndValue = Value - SmallChange; //Position Changed, Refresh
          else
            EndValue = Value + SmallChange;
          MoveTimer.Start();
        }
      }
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
        int MousePosition = orientation == HVOrientation.Vertical ? e.Location.Y : e.Location.X;
        MousePosition = Clamp(MousePosition, MousePositionMax, MousePositionMin);
        int DeltaPosition = MousePosition - ClickPosition;
        ThumbFrontPosition = ThumbFrontPositionOld + DeltaPosition; //Value Changed
        OnScroll(ScrollEventType.ThumbTrack, ValueOld, _Value, scrollOrientation);
        Invalidate();
      }
    }
    #endregion

    #region Keyboard Methods
    public int SmallChange => Clamp(ValueLength / 20f, int.MaxValue, 1);
    public int LargeChange => Clamp(ValueLength / 10f, int.MaxValue, 1);
    protected override void OnKeyDown(KeyEventArgs e)
    {
      base.OnKeyDown(e);

      bool isScroll = true;
      int ValueOld = _Value, delta;
      MoveRatio = 0;
      StartValue = _Value;
      switch (e.KeyCode)
      {
        case Keys.Up:
          delta = (orientation == HVOrientation.Vertical) ? -SmallChange : 0; //Position Changed, Refresh
          EndValue = Value + delta;
          break;
        case Keys.Down:
          delta = (orientation == HVOrientation.Vertical) ? +SmallChange : 0;
          EndValue = Value + delta;
          break;
        case Keys.Right:
          delta = (orientation == HVOrientation.Horizontal) ? +SmallChange : 0;
          EndValue = Value + delta;
          break;
        case Keys.Left:
          delta = (orientation == HVOrientation.Horizontal) ? -SmallChange : 0;
          EndValue = Value + delta;
          break;
        case Keys.PageUp:
          EndValue = Value - LargeChange;
          break;
        case Keys.PageDown:
          EndValue = Value + LargeChange;
          break;
        case Keys.End:
          EndValue = _Maximum;
          break;
        case Keys.Home:
          EndValue = _Minimum;
          break;
        default:
          isScroll = false;
          break;
      }
      if (isScroll)
      {
        MoveTimer.Start();
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
    protected override void OnSizeChanged(EventArgs e)
    {
      base.OnSizeChanged(e);

      BarLength = Clamp(BarLength, 10000, Thickness + 1);
      ThumbLength = Clamp(ThumbLength, BarLength, Thickness + 1);
      ThumbFrontPosition = Clamp(ThumbFrontPosition, ThumbFrontPositionMax, ThumbFrontPositionMin);

      ThumbFrontPosition = ThumbFrontPosition; // 更新Value
                                               //Refresh(); ResizeRedraw = true控制項會自動重繪
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
      base.OnEnabledChanged(e);
      Invalidate();
    }
    #endregion
  }
}
