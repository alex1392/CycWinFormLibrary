using System;
using System.Windows.Forms;

namespace example
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    public void SetTextBox(string text) //實作一個公開方法，使其他Form可以傳遞資料進來
    {
      textBox1.Text = text;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      this.Hide(); //隱藏父視窗

      Form2 form2 = new Form2(); //創建子視窗
      form2.SetTextBox(textBox1.Text);    //將Form1中textBox的資料透過公開方法傳遞給Form2
      switch (form2.ShowDialog(this))
      {
        case DialogResult.Yes: //Form2中按下ToForm1按鈕
          this.Show(); //顯示父視窗
          break;
        case DialogResult.No: //Form2中按下關閉鈕
          this.Close();  //關閉父視窗 (同時結束應用程式)
          break;
        default:
          break;
      }
    }
  }
}
