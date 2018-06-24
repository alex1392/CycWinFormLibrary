using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Example
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
      openFileDialog1.Filter = "Image files (.jpg, .jpeg, .jpe, .jfif, .png) | *.PNG; *.jpg; *.jpeg; *.jpe; *.jfif; *.png |All files (*.*)| *.*";
    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        imageViewer1.Image = Image.FromFile(openFileDialog1.FileName);
      }
    }
  }
}
