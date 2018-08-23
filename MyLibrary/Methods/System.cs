using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLibrary.Methods
{
  public static class System
  {
    /* 非同步委派更新UI Example:
     * control.InvokeIfRequired(new Action(() => { "Do Something" }));
     */
    public static void InvokeIfRequired(this Control control, Action action)
    {
      if (control.InvokeRequired)//在非當前執行緒內 使用委派
      {
        control.Invoke(action);
      }
      else
      {
        action();
      }
    }

    /*	TimeIt Example:
    *	TimeIt( () => { string s = "Your Codes"; } );
    */
    public static void TimeIt(Action action)
    {
      Stopwatch sw = new Stopwatch();//引用stopwatch物件
      sw.Restart();
      //-----目標程式-----//
      action.Invoke();
      //-----目標程式-----//
      sw.Stop();//碼錶停止
      string result = sw.Elapsed.TotalMilliseconds.ToString();
      Console.WriteLine(result);
    }

    /*	GetAllControls Exmaple:	
     *	List<Control> AllControls = GetAllControls(Form); 
     */
    public static List<Control> GetAllControls(Form form)
    {
      return GetAllControls(ToList(form.Controls));
    }
    public static List<Control> ToList(Control.ControlCollection controls)
    {
      List<Control> controlList = new List<Control>();
      foreach (Control control in controls)
        controlList.Add(control);
      return controlList;
    }
    public static List<Control> GetAllControls(List<Control> inputList)
    {
      //複製inputList到outputList
      List<Control> outputList = new List<Control>(inputList);

      //取出inputList中的容器
      IEnumerable<Control> containers = from control in inputList
                                        where
              control is GroupBox |
              control is TabControl |
              control is Panel |
              control is FlowLayoutPanel |
              control is TableLayoutPanel |
              control is ContainerControl
                                        select control;


      foreach (Control container in containers)
      {
        //遞迴加入容器內的容器與控制項
        outputList.AddRange(GetAllControls(ToList(container.Controls)));
      }
      return outputList;
    }

    public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign)
    {
      TextFormatFlags controlFlags = TextFormatFlags.EndEllipsis;

      switch (textAlign)
      {
        case ContentAlignment.TopLeft:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.Left;
          break;
        case ContentAlignment.TopCenter:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.TopRight:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.Right;
          break;
        case ContentAlignment.MiddleLeft:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
          break;
        case ContentAlignment.MiddleCenter:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.MiddleRight:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
          break;
        case ContentAlignment.BottomLeft:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
          break;
        case ContentAlignment.BottomCenter:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.BottomRight:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
          break;
      }

      return controlFlags;
    }
  }
}
