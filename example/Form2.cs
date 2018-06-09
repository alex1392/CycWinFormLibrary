using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace example
{
  public partial class Form2 : Form
  {
    public Form2()
    {
      InitializeComponent();
    }

    public void SetTextBox(string text) //實作一個公開方法，使其他Form可以傳遞資料進來
    {
      textBox2.Text = text;
    }

    bool IsToForm1 = false; //紀錄是否要回到Form1
    private void button2_Click(object sender, EventArgs e)
    {
      IsToForm1 = true;
      this.Close(); //強制關閉Form2
    }

    protected override void OnClosing(CancelEventArgs e) //在視窗關閉時觸發
    {
      base.OnClosing(e);
      if (IsToForm1) //判斷是否要回到Form1
      {
        this.DialogResult = DialogResult.Yes; //利用DialogResult傳遞訊息
        Form1 form1 = (Form1)this.Owner; //取得父視窗的參考
        form1.SetTextBox(textBox2.Text); //將Form2中textBox的資料透過公開方法傳遞給Form1
      }
      else
      {
        this.DialogResult = DialogResult.No;
      }
    }

    
  }
}
