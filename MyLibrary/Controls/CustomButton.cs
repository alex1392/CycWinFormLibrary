using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLibrary.Controls
{
  public class CustomButton : Button
  {
    public CustomButton()
    {

    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
      TextBox tb = new TextBox();
      tb.Width = 100;
      tb.Height = 20;
      tb.Top = this.Top - 20;
      tb.Left = this.Left;
      this.Parent.Controls.Add(tb);
      base.OnPaint(pevent);
    }
  }
}
