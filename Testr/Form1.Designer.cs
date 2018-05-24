namespace Testr
{
	partial class Form1
	{
		/// <summary>
		/// 設計工具所需的變數。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清除任何使用中的資源。
		/// </summary>
		/// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 設計工具產生的程式碼

		/// <summary>
		/// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
		/// 這個方法的內容。
		/// </summary>
		private void InitializeComponent()
		{
			this.slider1 = new MyLibrary.Controls.Slider();
			this.rangeSlider1 = new MyLibrary.Controls.RangeSlider();
			this.imageViewer1 = new MyLibrary.Controls.ImageViewer();
			this.scrollBar1 = new MyLibrary.Controls.ScrollBar();
			this.SuspendLayout();
			// 
			// slider1
			// 
			this.slider1.ArrowChange = ((uint)(1u));
			this.slider1.BackColor = System.Drawing.Color.Transparent;
			this.slider1.BarMax = 100;
			this.slider1.BarMin = 0;
			this.slider1.CustomBackground = false;
			this.slider1.Location = new System.Drawing.Point(277, 325);
			this.slider1.Name = "slider1";
			this.slider1.Orientation = MyLibrary.SliderOrientation.Down;
			this.slider1.PageChange = ((uint)(5u));
			this.slider1.Reverse = false;
			this.slider1.ScrollChange = ((uint)(10u));
			this.slider1.Size = new System.Drawing.Size(75, 23);
			this.slider1.Style = MetroFramework.MetroColorStyle.Blue;
			this.slider1.StyleManager = null;
			this.slider1.TabIndex = 3;
			this.slider1.Text = "slider1";
			this.slider1.Theme = MetroFramework.MetroThemeStyle.Light;
			this.slider1.Value = 25;
			// 
			// rangeSlider1
			// 
			this.rangeSlider1.ArrowChange = ((uint)(1u));
			this.rangeSlider1.BackColor = System.Drawing.Color.Transparent;
			this.rangeSlider1.BarMax = 255;
			this.rangeSlider1.BarMin = 0;
			this.rangeSlider1.CustomBackground = false;
			this.rangeSlider1.Location = new System.Drawing.Point(67, 326);
			this.rangeSlider1.Name = "rangeSlider1";
			this.rangeSlider1.Orientation = MyLibrary.SliderOrientation.Horizontal;
			this.rangeSlider1.PageChange = ((uint)(5u));
			this.rangeSlider1.RangeMax = 200;
			this.rangeSlider1.RangeMin = 50;
			this.rangeSlider1.Reverse = false;
			this.rangeSlider1.ScrollChange = ((uint)(10u));
			this.rangeSlider1.Size = new System.Drawing.Size(75, 23);
			this.rangeSlider1.Style = MetroFramework.MetroColorStyle.Blue;
			this.rangeSlider1.StyleManager = null;
			this.rangeSlider1.TabIndex = 1;
			this.rangeSlider1.Text = "rangeSlider1";
			this.rangeSlider1.Theme = MetroFramework.MetroThemeStyle.Light;
			// 
			// imageViewer1
			// 
			this.imageViewer1.Image = null;
			this.imageViewer1.Location = new System.Drawing.Point(47, 13);
			this.imageViewer1.Name = "imageViewer1";
			this.imageViewer1.Size = new System.Drawing.Size(341, 288);
			this.imageViewer1.TabIndex = 0;
			// 
			// scrollBar1
			// 
			this.scrollBar1.BarLength = 200;
			this.scrollBar1.LargeChange = 20;
			this.scrollBar1.Location = new System.Drawing.Point(237, 151);
			this.scrollBar1.Maximum = 100;
			this.scrollBar1.Minimum = 0;
			this.scrollBar1.MouseWheelBarPartitions = 20;
			this.scrollBar1.Name = "scrollBar1";
			this.scrollBar1.Orientation = MyLibrary.ScrollBarOrientation.Horizontal;
			this.scrollBar1.Size = new System.Drawing.Size(200, 10);
			this.scrollBar1.SmallChange = 10;
			this.scrollBar1.Style = MetroFramework.MetroColorStyle.Blue;
			this.scrollBar1.StyleManager = null;
			this.scrollBar1.TabIndex = 4;
			this.scrollBar1.Text = "scrollBar1";
			this.scrollBar1.Theme = MetroFramework.MetroThemeStyle.Light;
			this.scrollBar1.ThumbLength = 11;
			this.scrollBar1.UseBarColor = true;
			this.scrollBar1.Value = 0;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(479, 421);
			this.Controls.Add(this.scrollBar1);
			this.Controls.Add(this.slider1);
			this.Controls.Add(this.rangeSlider1);
			this.Controls.Add(this.imageViewer1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);

		}

		#endregion

		private MyLibrary.Controls.ImageViewer imageViewer1;
		private MyLibrary.Controls.RangeSlider rangeSlider1;
		private MyLibrary.Controls.Slider slider1;
		private MyLibrary.Controls.ScrollBar scrollBar1;
	}
}

