using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLibrary.Forms
{
  public partial class AutoResizeControlsForm : Form
  {
    public AutoResizeControlsForm()
    {
      InitializeComponent();
    }

    public class ControlAnchor
    {
      public int Top;
      public int Left;
      public int Width;
      public int Height;
    }

    private List<Control> AllControls;
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

    private void Form1_Load(object sender, EventArgs e)
    {
      AllControls = GetAllControls(this);
      this.Tag = new ControlAnchor()
      {
        Height = this.Height,
        Width = this.Width
      };
      foreach (Control control in AllControls)
      {
        control.Tag = new ControlAnchor()
        {
          Top = control.Top,
          Left = control.Left,
          Height = control.Height,
          Width = control.Width,
        };
      }
    }
    private void Form1_Resize(object sender, EventArgs e)
    {
      ControlAnchor formAnchor = (ControlAnchor)this.Tag;
      float WidthRatio = (float)this.Width / formAnchor.Width;
      float HeightRatio = (float)this.Height / formAnchor.Height;
      foreach (Control control in AllControls)
      {
        ControlAnchor controlAnchor = (ControlAnchor)control.Tag;
        control.Width = (int)(controlAnchor.Width * WidthRatio);
        control.Height = (int)(controlAnchor.Height * HeightRatio);
        control.Left = (int)(controlAnchor.Left * WidthRatio);
        control.Top = (int)(controlAnchor.Top * HeightRatio);
      }
    }
  }
}
