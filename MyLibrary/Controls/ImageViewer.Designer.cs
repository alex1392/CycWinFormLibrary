using MyLibrary.Classes;

namespace MyLibrary.Controls
{
	partial class ImageViewer
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

		#region 元件設計工具產生的程式碼

		/// <summary> 
		/// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
		/// 這個方法的內容。
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox = new System.Windows.Forms.PictureBox();
			this.ScrollBarHorizontal = new MyLibrary.Controls.ScrollBar();
			this.ScrollBarVertical = new MyLibrary.Controls.ScrollBar();
			this.BackgroundWorker = new System.ComponentModel.BackgroundWorker();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox
			// 
			this.pictureBox.Location = new System.Drawing.Point(3, 3);
			this.pictureBox.Name = "pictureBox";
			this.pictureBox.Size = new System.Drawing.Size(306, 252);
			this.pictureBox.TabIndex = 2;
			this.pictureBox.TabStop = false;
			this.pictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseDown);
			this.pictureBox.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
			this.pictureBox.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
			this.pictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
			this.pictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseUp);
			// 
			// ScrollBarHorizontal
			// 
			this.ScrollBarHorizontal.BarLength = 306;
			this.ScrollBarHorizontal.Location = new System.Drawing.Point(3, 261);
			this.ScrollBarHorizontal.Maximum = 100;
			this.ScrollBarHorizontal.Minimum = 0;
			this.ScrollBarHorizontal.Name = "ScrollBarHorizontal";
			this.ScrollBarHorizontal.Orientation = HVOrientation.Horizontal;
			this.ScrollBarHorizontal.Size = new System.Drawing.Size(306, 18);
			this.ScrollBarHorizontal.TabIndex = 4;
			this.ScrollBarHorizontal.Text = "scrollBar1";
			this.ScrollBarHorizontal.ThumbLength = 28;
			this.ScrollBarHorizontal.UseBarColor = true;
			this.ScrollBarHorizontal.Value = 0;
			this.ScrollBarHorizontal.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollBarHorizontal_Scroll);
			// 
			// ScrollBarVertical
			// 
			this.ScrollBarVertical.BarLength = 260;
			this.ScrollBarVertical.Location = new System.Drawing.Point(315, 3);
			this.ScrollBarVertical.Maximum = 100;
			this.ScrollBarVertical.Minimum = 0;
			this.ScrollBarVertical.Name = "ScrollBarVertical";
			this.ScrollBarVertical.Orientation = HVOrientation.Vertical;
			this.ScrollBarVertical.Size = new System.Drawing.Size(16, 260);
			this.ScrollBarVertical.TabIndex = 3;
			this.ScrollBarVertical.Text = "scrollBar1";
			this.ScrollBarVertical.ThumbLength = 60;
			this.ScrollBarVertical.UseBarColor = true;
			this.ScrollBarVertical.Value = 0;
			this.ScrollBarVertical.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollBarVertical_Scroll);
			// 
			// BackgroundWorker
			// 
			this.BackgroundWorker.WorkerReportsProgress = true;
			this.BackgroundWorker.WorkerSupportsCancellation = true;
			this.BackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker_DoWork);
			this.BackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker_RunWorkerCompleted);
			// 
			// ImageViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ScrollBarHorizontal);
			this.Controls.Add(this.ScrollBarVertical);
			this.Controls.Add(this.pictureBox);
			this.Name = "ImageViewer";
			this.Size = new System.Drawing.Size(341, 288);
			this.Load += new System.EventHandler(this.ImageViewer_Load);
			this.Resize += new System.EventHandler(this.ImageViewer_Resize);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.PictureBox pictureBox;
		private ScrollBar ScrollBarVertical;
		private ScrollBar ScrollBarHorizontal;
		private System.ComponentModel.BackgroundWorker BackgroundWorker;
	}
}
