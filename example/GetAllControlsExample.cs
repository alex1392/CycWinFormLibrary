using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace example
{
  public partial class GetAllControlsExample : Form
  {
    public GetAllControlsExample()
    {
      InitializeComponent();

      Console.WriteLine("===Form.Controls===");
      foreach (Control control in this.Controls)
      {
        Console.WriteLine(control.Name);
      }

      Console.WriteLine("===GetAllControls===");
      List<Control> AllControls = GetAllControls(this);
      foreach (Control control in AllControls)
      {
        Console.WriteLine(control.Name);
      }
    }
    
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
      IEnumerable<Control> containers = from control in inputList where
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

  }
}
