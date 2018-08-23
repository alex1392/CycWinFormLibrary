using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Controls;

namespace MyLibrary.Extensions
{
  public static class ControlExtensions
  {
    /* 非同步委派更新UI Example:
     * control.InvokeIfRequired(new Action(() => { "Do Something" }));
     */
    public static void InvokeIfRequired(this System.Windows.Forms.Control control, Action action)
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
  }
}
