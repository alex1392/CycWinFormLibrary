using System;
using System.Windows.Forms;

namespace MyLibrary
{
	public partial class AutoResizeControlsForm : Form
	{
		public AutoResizeControlsForm()
		{
			InitializeComponent();
		}

		// AutoResizeControls
		private void Form1_Load(object sender, EventArgs e)
		{
			this.Tag = this.Height + "|" + this.Width;
			foreach (Control control in this.Controls)
			{
				control.Tag = control.Top + "|" + control.Left + "|" + control.Height + "|" + control.Width;
			}
		}
		private void Form1_Resize(object sender, EventArgs e)
		{
			if (this.Tag == null)
				return;
			foreach (Control control in this.Controls)
			{
				control.Width = (int)(double.Parse(control.Tag.ToString().Split('|')[3]) * (this.Width / double.Parse(this.Tag.ToString().Split('|')[1])));
				control.Height = (int)(double.Parse(control.Tag.ToString().Split('|')[2]) * (this.Height / double.Parse(this.Tag.ToString().Split('|')[0])));
				control.Left = (int)(double.Parse(control.Tag.ToString().Split('|')[1]) * (this.Width / double.Parse(this.Tag.ToString().Split('|')[1])));
				control.Top = (int)(double.Parse(control.Tag.ToString().Split('|')[0]) * (this.Height / double.Parse(this.Tag.ToString().Split('|')[0])));
			}
		}
	}
}
