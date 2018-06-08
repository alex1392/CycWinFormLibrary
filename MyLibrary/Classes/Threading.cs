using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.Classes
{
  public class BackgroundArgs
  {
    public object sender;
    public EventArgs e;
    public object[] parameters;
    public object[] results;

    public BackgroundArgs(object sender)
    {
      this.sender = sender;
    }
    public BackgroundArgs(EventArgs e, object[] parameters)
    {
      this.e = e;
      this.parameters = parameters;
    }
    public BackgroundArgs(object sender, EventArgs e)
    {
      this.sender = sender;
      this.e = e;
    }
    public BackgroundArgs(object sender, EventArgs e, object[] parameters)
    {
      this.sender = sender;
      this.e = e;
      this.parameters = parameters;
    }
  }
}
