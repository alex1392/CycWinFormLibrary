using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyLibrary.Forms
{
  /// <summary>
  /// 浮動層基類
  /// </summary>
  public class FloatLayerBase : Form
  {
    /// <summary>
    /// 鼠標消息篩選器
    /// </summary>
    //由於本窗體為WS_CHILD，所以不會收到在窗體以外點擊鼠標的消息
    //該消息篩選器的作用就是讓本窗體獲知鼠標點擊情況，進而根據鼠標是否在本窗體以外的區域點擊，做出相應處理
    readonly AppMouseMessageHandler _mouseMsgFilter;

    /// <summary>
    /// 指示本窗體是否已ShowDialog過
    /// </summary>
    //由於多次ShowDialog會使OnLoad/OnShown重入，故需設置此標記以供重入時判斷
    bool _isShowDialogAgain;

    //邊框相關字段
    BorderStyle _borderType;
    Border3DStyle _border3DStyle;
    ButtonBorderStyle _borderSingleStyle;
    Color _borderColor;

    [Description("獲取或設置邊框類型。")]
    [DefaultValue(BorderStyle.Fixed3D)]
    public BorderStyle BorderType
    {
      get { return _borderType; }
      set
      {
        if (_borderType == value) { return; }
        _borderType = value;
        Invalidate();
      }
    }

    [Description("獲取或設置三維邊框樣式。")]
    [DefaultValue(Border3DStyle.RaisedInner)]
    public Border3DStyle Border3DStyle
    {
      get { return _border3DStyle; }
      set
      {
        if (_border3DStyle == value) { return; }
        _border3DStyle = value;
        Invalidate();
      }
    }

    [Description("獲取或設置線型邊框樣式。")]
    [DefaultValue(ButtonBorderStyle.Solid)]
    public ButtonBorderStyle BorderSingleStyle
    {
      get { return _borderSingleStyle; }
      set
      {
        if (_borderSingleStyle == value) { return; }
        _borderSingleStyle = value;
        Invalidate();
      }
    }

    [Description("獲取或設置邊框顏色（僅當邊框類型為線型時有效）。")]
    [DefaultValue(typeof(Color), "DarkGray")]
    public Color BorderColor
    {
      get { return _borderColor; }
      set
      {
        if (_borderColor == value) { return; }
        _borderColor = value;
        Invalidate();
      }
    }

    protected override sealed CreateParams CreateParams
    {
      get
      {
        CreateParams prms = base.CreateParams;

        //prms.Style = 0;
        //prms.Style |= -2147483648;   //WS_POPUP
        prms.Style |= 0x40000000;      //WS_CHILD  重要，只有CHILD窗體才不會搶父窗體焦點
        prms.Style |= 0x4000000;       //WS_CLIPSIBLINGS
        prms.Style |= 0x10000;         //WS_TABSTOP
        prms.Style &= ~0x40000;        //WS_SIZEBOX       去除
        prms.Style &= ~0x800000;       //WS_BORDER        去除
        prms.Style &= ~0x400000;       //WS_DLGFRAME      去除
                                       //prms.Style &= ~0x20000;      //WS_MINIMIZEBOX   去除
                                       //prms.Style &= ~0x10000;      //WS_MAXIMIZEBOX   去除

        prms.ExStyle = 0;
        //prms.ExStyle |= 0x1;         //WS_EX_DLGMODALFRAME 立體邊框
        //prms.ExStyle |= 0x8;         //WS_EX_TOPMOST
        prms.ExStyle |= 0x10000;       //WS_EX_CONTROLPARENT
                                       //prms.ExStyle |= 0x80;        //WS_EX_TOOLWINDOW
                                       //prms.ExStyle |= 0x100;       //WS_EX_WINDOWEDGE
                                       //prms.ExStyle |= 0x8000000;   //WS_EX_NOACTIVATE
                                       //prms.ExStyle |= 0x4;         //WS_EX_NOPARENTNOTIFY

        return prms;
      }
    }

    //構造函數
    public FloatLayerBase()
    {
      //初始化消息篩選器。添加和移除在顯示/隱藏時負責
      _mouseMsgFilter = new AppMouseMessageHandler(this);

      //初始化基類屬性
      InitBaseProperties();

      //初始化邊框相關
      _borderType = BorderStyle.Fixed3D;
      _border3DStyle = System.Windows.Forms.Border3DStyle.RaisedInner;
      _borderSingleStyle = ButtonBorderStyle.Solid;
      _borderColor = Color.DarkGray;
    }

    protected override void OnLoad(EventArgs e)
    {
      //防止重入
      if (_isShowDialogAgain) { return; }

      //需得減掉兩層邊框寬度，運行時尺寸才與設計時完全相符，原因不明
      //確定與ControlBox、FormBorderStyle有關，但具體聯繫不明
      if (!DesignMode)
      {
        Size size = SystemInformation.FrameBorderSize;
        this.Size -= size + size;//不可以用ClientSize，後者會根據窗口風格重新調整Size
      }
      base.OnLoad(e);
    }

    protected override void OnShown(EventArgs e)
    {
      //防止重入
      if (_isShowDialogAgain) { return; }

      //在OnShown中為首次ShowDialog設標記
      if (Modal) { _isShowDialogAgain = true; }

      if (!DesignMode)
      {
        //激活首控件
        Control firstControl;
        if ((firstControl = GetNextControl(this, true)) != null)
        {
          firstControl.Focus();
        }
      }
      base.OnShown(e);
    }

    protected override void WndProc(ref Message m)
    {
      //當本窗體作為ShowDialog彈出時，在收到WM_SHOWWINDOW前，Owner會被Disable
      //故需在收到該消息後立即Enable它，不然Owner窗體和本窗體都將處於無響應狀態
      if (m.Msg == 0x18 && m.WParam != IntPtr.Zero && m.LParam == IntPtr.Zero
          && Modal && Owner != null && !Owner.IsDisposed)
      {
        if (Owner.IsMdiChild)
        {
          //當Owner是MDI子窗體時，被Disable的是MDI主窗體
          //並且Parent也會指向MDI主窗體，故需改回為Owner，這樣彈出窗體的Location才會相對於Owner而非MDIParent
          NativeMethods.EnableWindow(Owner.MdiParent.Handle, true);
          NativeMethods.SetParent(this.Handle, Owner.Handle);//只能用API設置Parent，因為模式窗體是TopLevel，.Net拒絕為頂級窗體設置Parent
        }
        else
        {
          NativeMethods.EnableWindow(Owner.Handle, true);
        }
      }
      base.WndProc(ref m);
    }

    //畫邊框
    protected override void OnPaintBackground(PaintEventArgs e)
    {
      base.OnPaintBackground(e);

      if (_borderType == BorderStyle.Fixed3D)//繪製3D邊框
      {
        ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle);
      }
      else if (_borderType == BorderStyle.FixedSingle)//繪製線型邊框
      {
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle, BorderColor, BorderSingleStyle);
      }
    }

    //顯示後添加鼠標消息篩選器以開始捕捉，隱藏時則移除篩選器。之所以不放Dispose中是想盡早移除篩選器
    protected override void OnVisibleChanged(EventArgs e)
    {
      if (!DesignMode)
      {
        if (Visible) { Application.AddMessageFilter(_mouseMsgFilter); }
        else { Application.RemoveMessageFilter(_mouseMsgFilter); }
      }
      base.OnVisibleChanged(e);
    }

    //實現窗體客户區拖動
    //在WndProc中實現這個較麻煩，所以放到這裏做
    protected override void OnMouseDown(MouseEventArgs e)
    {
      //讓鼠標點擊客户區時達到與點擊標題欄一樣的效果，以此實現客户區拖動
      NativeMethods.ReleaseCapture();
      NativeMethods.SendMessage(Handle, 0xA1/*WM_NCLBUTTONDOWN*/, (IntPtr)2/*CAPTION*/, IntPtr.Zero);

      base.OnMouseDown(e);
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="control">顯示在該控件下方</param>
    public DialogResult ShowDialog(Control control)
    {
      return ShowDialog(control, 0, control.Height);
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="control">觸發彈出窗體的控件</param>
    /// <param name="offsetX">相對control水平偏移</param>
    /// <param name="offsetY">相對control垂直偏移</param>
    public DialogResult ShowDialog(Control control, int offsetX, int offsetY)
    {
      return ShowDialog(control, new Point(offsetX, offsetY));
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="control">觸發彈出窗體的控件</param>
    /// <param name="offset">相對control偏移</param>
    public DialogResult ShowDialog(Control control, Point offset)
    {
      return this.ShowDialogInternal(control, offset);
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="item">顯示在該工具欄項的下方</param>
    public DialogResult ShowDialog(ToolStripItem item)
    {
      return ShowDialog(item, 0, item.Height);
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="item">觸發彈出窗體的工具欄項</param>
    /// <param name="offsetX">相對item水平偏移</param>
    /// <param name="offsetY">相對item垂直偏移</param>
    public DialogResult ShowDialog(ToolStripItem item, int offsetX, int offsetY)
    {
      return ShowDialog(item, new Point(offsetX, offsetY));
    }

    /// <summary>
    /// 顯示為模式窗體
    /// </summary>
    /// <param name="item">觸發彈出窗體的工具欄項</param>
    /// <param name="offset">相對item偏移</param>
    public DialogResult ShowDialog(ToolStripItem item, Point offset)
    {
      return this.ShowDialogInternal(item, offset);
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="control">顯示在該控件下方</param>
    public void Show(Control control)
    {
      Show(control, 0, control.Height);
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="control">觸發彈出窗體的控件</param>
    /// <param name="offsetX">相對control水平偏移</param>
    /// <param name="offsetY">相對control垂直偏移</param>
    public void Show(Control control, int offsetX, int offsetY)
    {
      Show(control, new Point(offsetX, offsetY));
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="control">觸發彈出窗體的控件</param>
    /// <param name="offset">相對control偏移</param>
    public void Show(Control control, Point offset)
    {
      this.ShowInternal(control, offset);
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="item">顯示在該工具欄下方</param>
    public void Show(ToolStripItem item)
    {
      Show(item, 0, item.Height);
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="item">觸發彈出窗體的工具欄項</param>
    /// <param name="offsetX">相對item水平偏移</param>
    /// <param name="offsetY">相對item垂直偏移</param>
    public void Show(ToolStripItem item, int offsetX, int offsetY)
    {
      Show(item, new Point(offsetX, offsetY));
    }

    /// <summary>
    /// 顯示窗體
    /// </summary>
    /// <param name="item">觸發彈出窗體的工具欄項</param>
    /// <param name="offset">相對item偏移</param>
    public void Show(ToolStripItem item, Point offset)
    {
      this.ShowInternal(item, offset);
    }

    /// <summary>
    /// ShowDialog內部方法
    /// </summary>
    private DialogResult ShowDialogInternal(Component controlOrItem, Point offset)
    {
      //快速連續彈出本窗體將有可能遇到尚未Hide的情況下再次彈出，這會引發異常，故需做處理
      if (this.Visible) { return System.Windows.Forms.DialogResult.None; }

      this.SetLocationAndOwner(controlOrItem, offset);
      return base.ShowDialog();
    }

    /// <summary>
    /// Show內部方法
    /// </summary>
    private void ShowInternal(Component controlOrItem, Point offset)
    {
      if (this.Visible) { return; }//原因見ShowDialogInternal

      this.SetLocationAndOwner(controlOrItem, offset);
      base.Show();
    }

    /// <summary>
    /// 設置座標及所有者
    /// </summary>
    /// <param name="controlOrItem">控件或工具欄項</param>
    /// <param name="offset">相對偏移</param>
    private void SetLocationAndOwner(Component controlOrItem, Point offset)
    {
      Point pt = Point.Empty;

      if (controlOrItem is ToolStripItem)
      {
        ToolStripItem item = (ToolStripItem)controlOrItem;
        pt.Offset(item.Bounds.Location);
        controlOrItem = item.Owner;
      }

      Control c = (Control)controlOrItem;
      pt.Offset(GetControlLocationInForm(c));
      pt.Offset(offset);
      this.Location = pt;

      //設置Owner屬性與Show[Dialog](Owner)有不同，當Owner是MDIChild時，後者會改Owner為MDIParent
      this.Owner = c.FindForm();
    }

    /// <summary>
    /// 獲取控件在窗體中的座標
    /// </summary>
    private static Point GetControlLocationInForm(Control c)
    {
      Point pt = c.Location;
      while (!((c = c.Parent) is Form))
      {
        pt.Offset(c.Location);
      }
      return pt;
    }

    #region 屏蔽對本類影響重大的基類方法和屬性

    /// <summary>
    /// 初始化部分基類屬性
    /// </summary>
    private void InitBaseProperties()
    {
      base.ControlBox = false;                           //重要
                                                         //必須得是SizableToolWindow才能支持調整大小的同時，不受SystemInformation.MinWindowTrackSize的限制
      base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
      base.Text = string.Empty;                          //重要
      base.HelpButton = false;
      base.Icon = null;
      base.IsMdiContainer = false;
      base.MaximizeBox = false;
      base.MinimizeBox = false;
      base.ShowIcon = false;
      base.ShowInTaskbar = false;
      base.StartPosition = FormStartPosition.Manual;     //重要
      base.TopMost = false;
      base.WindowState = FormWindowState.Normal;
    }

    //屏蔽原方法
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("請使用別的重載！", true)]
    public new DialogResult ShowDialog() { throw new NotImplementedException(); }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("請使用別的重載！", true)]
    public new DialogResult ShowDialog(IWin32Window owner) { throw new NotImplementedException(); }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("請使用別的重載！", true)]
    public new void Show() { throw new NotImplementedException(); }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("請使用別的重載！", true)]
    public new void Show(IWin32Window owner) { throw new NotImplementedException(); }

    //屏蔽原屬性
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool ControlBox { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("設置邊框請使用Border相關屬性！", true)]
    public new FormBorderStyle FormBorderStyle { get { return System.Windows.Forms.FormBorderStyle.SizableToolWindow; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public override sealed string Text { get { return string.Empty; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool HelpButton { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new Image Icon { get { return null; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool IsMdiContainer { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool MaximizeBox { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool MinimizeBox { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool ShowIcon { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool ShowInTaskbar { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new FormStartPosition StartPosition { get { return FormStartPosition.Manual; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new bool TopMost { get { return false; } set { } }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("禁用該屬性！", true)]
    public new FormWindowState WindowState { get { return FormWindowState.Normal; } set { } }

    #endregion

    /// <summary>
    /// 進程鼠標消息篩選器
    /// </summary>
    private class AppMouseMessageHandler : IMessageFilter
    {
      readonly FloatLayerBase _layerForm;

      public AppMouseMessageHandler(FloatLayerBase layerForm)
      {
        _layerForm = layerForm;
      }

      public bool PreFilterMessage(ref Message m)
      {
        //如果在本窗體以外點擊鼠標，隱藏本窗體
        //若想在點擊標題欄、滾動條等非客户區也要讓本窗體消失，取消0xA1的註釋即可
        //本例是根據座標判斷，亦可以改為根據句柄，但要考慮子孫控件
        //之所以用API而不用Form.DesktopBounds是因為後者不可靠
        if ((m.Msg == 0x201/*|| m.Msg==0xA1*/)
            && _layerForm.Visible && !NativeMethods.GetWindowRect(_layerForm.Handle).Contains(MousePosition))
        {
          _layerForm.Hide();//之所以不Close是考慮應該由調用者負責銷燬
        }

        return false;
      }
    }

    /// <summary>
    /// API封裝類
    /// </summary>
    private static class NativeMethods
    {
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

      [DllImport("user32.dll")]
      public static extern bool ReleaseCapture();

      [DllImport("user32.dll", SetLastError = true)]
      public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

      [DllImport("user32.dll", SetLastError = true)]
      private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

      [StructLayout(LayoutKind.Sequential)]
      private struct RECT
      {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public static explicit operator Rectangle(RECT rect)
        {
          return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }
      }

      public static Rectangle GetWindowRect(IntPtr hwnd)
      {
        RECT rect;
        GetWindowRect(hwnd, out rect);
        return (Rectangle)rect;
      }
    }
  }
}