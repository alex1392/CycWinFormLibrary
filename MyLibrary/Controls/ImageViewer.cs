using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Diagnostics;
using static MyLibrary.Methods.Math;
using static MyLibrary.Methods.Drawing;
using static MyLibrary.Methods.System;
using MyLibrary.Classes;

namespace MyLibrary.Controls
{
	[DefaultEvent("Click")]
	public partial class ImageViewer : UserControl
	{
		public ImageViewer()
		{
			InitializeComponent();
		}

		int changeTimes = 0;
		private Image _DisplayImage;
		private Image DisplayImage
		{
			get => _DisplayImage;
			set
			{
				_DisplayImage = value;
				changeTimes++;
				//Console.WriteLine("DisplayImage Changed {0}", changeTimes);
			}
		}

		//程式執行時更新影像設定給OriginImage，不會即時更新畫面
		private Image _OriginImage;
		public Image OriginImage
		{
			private get => _OriginImage;
			set
			{
				_OriginImage = value;
			}
		}
		//設計工具中設定影像給Image，及時更新畫面
		[Category("Appearance")]
		public Image Image
		{
			get => OriginImage;
			set
			{
				OriginImage = value;
				//Console.WriteLine("Set Image");
				DisplayImage = UpdateDisplayImage(OriginImage);
				UpdatePictureBox();
				UpdateScrollBar();
			}
		}

		private int EffectivePictureBoxWidth => (int)(pictureBox.Width / ZoomFactor);
		private int EffectivePictureBoxHeight => (int)(pictureBox.Height / ZoomFactor);
		private int EffectiveImageWidth => (int)(OriginImage.Width * ZoomFactor);
		private int EffectiveImageHeight => (int)(OriginImage.Height * ZoomFactor);
		private bool IsImageWidthExceed => (OriginImage.Width > EffectivePictureBoxWidth) ? true : false;
		private bool IsImageHeightExceed => (OriginImage.Height > EffectivePictureBoxHeight) ? true : false;

		[Category("Appearance")]
		public Point ImageBoxPos
		{
			get => _ImageBoxPos;
			private set
			{
				if (OriginImage == null)
					return;
				_ImageBoxPos.X = (IsImageWidthExceed) ? Clamp(value.X, OriginImage.Width - EffectivePictureBoxWidth, 0) : 0;
				_ImageBoxPos.Y = (IsImageHeightExceed) ? Clamp(value.Y, OriginImage.Height - EffectivePictureBoxHeight, 0) : 0;
			}
		}
		private Point _ImageBoxPos = new Point(0, 0);
		
		//background worker
		private Image UpdateDisplayImage(Image OriginImage)
		{
			Image DisplayImage = new Bitmap(pictureBox.Width, pictureBox.Height); //使用new，因此已經不是原來的DisplayImage
			if (OriginImage == null)
			{
				return DisplayImage;
			}

			Rectangle DestRect = pictureBox.ClientRectangle;
			Rectangle CropRectangle = new Rectangle
			{
				X = ImageBoxPos.X,
				Y = ImageBoxPos.Y,
				Width = EffectivePictureBoxWidth,
				Height = EffectivePictureBoxHeight,
			};
			Rectangle BoundaryRectangle = new Rectangle
			{
				X = 0,
				Y = 0,
				Width = EffectiveImageWidth,
				Height = EffectiveImageHeight,
			};
			using (Graphics graphics = Graphics.FromImage(DisplayImage))
			{
				graphics.DrawImage(OriginImage, DestRect, CropRectangle, GraphicsUnit.Pixel);
				if (OriginImage != null)
				{
					using (Pen pen = new Pen(Color.Black, 1))
					{
						graphics.DrawRectangle(pen, BoundaryRectangle);
					}
				}
			}
			return DisplayImage;
		}
		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs DoWork_e)
		{
			if (this.OriginImage == null)
			{
				return;
			}
			BackgroundArgs args = (BackgroundArgs)DoWork_e.Argument;
			Image OriginImage = (Image)args.parameters[0]; //使欄位值變成區域變數
			Image DisplayImage = (Image)args.parameters[1]; //讓擴充功能不會共搶變數
			if (args.e is MouseEventArgs)
			{
				MouseEventArgs e = args.e as MouseEventArgs;
				if (args.sender == pictureBox && e.Button == MouseButtons.Right && e.Delta == 0)
				{
					int dX, dY;
					dX = (int)((e.X - AnchorPoint.X) / ZoomFactor);
					dY = (int)((e.Y - AnchorPoint.Y) / ZoomFactor);
					ImageBoxPos = new Point(OldBoxPos.X - dX, OldBoxPos.Y - dY);
					//Console.WriteLine("drag");
					DisplayImage = UpdateDisplayImage(OriginImage);
				}//drag
				else if (args.sender == this && e.Delta != 0 && pictureBox.ClientRectangle.Contains(e.Location))
				{
					float factor = (e.Delta > 0) ? 1.1f : 0.9f;
					ZoomFactor *= factor;
					Point EffectiveMouseLocation = GetEffectiveMouseLocation(e.Location);
					ImageBoxPos = new Point((int)(ImageBoxPos.X + EffectiveMouseLocation.X * (1 - 1 / factor)), (int)(ImageBoxPos.Y + EffectiveMouseLocation.Y * (1 - 1 / factor)));
					DisplayImage = UpdateDisplayImage(OriginImage);
				}//zoom
			}
			else if (args.e is ScrollEventArgs)
			{
				ScrollEventArgs e = args.e as ScrollEventArgs;
				if (args.sender == ScrollBarVertical)
				{
					ImageBoxPos = new Point(ImageBoxPos.X, e.NewValue);
					DisplayImage = UpdateDisplayImage(OriginImage);
				}
				else if (args.sender == ScrollBarHorizontal)
				{
					ImageBoxPos = new Point(e.NewValue, ImageBoxPos.Y);
					DisplayImage = UpdateDisplayImage(OriginImage);
				}
			}
			DoWork_e.Result = DisplayImage; //輸出更改的值
		}

		private void UpdateScrollBar()
		{
			if (OriginImage == null)
			{
				ScrollBarVertical.Enabled = false;
				ScrollBarHorizontal.Enabled = false;
			}
			else
			{
				ScrollBarHorizontal.Maximum = OriginImage.Width - EffectivePictureBoxWidth;
				ScrollBarHorizontal.ThumbLength = ScrollBarHorizontal.Width * EffectivePictureBoxWidth / OriginImage.Width; //一定要比Value先改，否則邊界值會出錯
				ScrollBarHorizontal.Value = ImageBoxPos.X;

				ScrollBarVertical.Maximum = OriginImage.Height - EffectivePictureBoxHeight;
				ScrollBarVertical.ThumbLength = ScrollBarVertical.Height * EffectivePictureBoxHeight / OriginImage.Height;
				ScrollBarVertical.Value = ImageBoxPos.Y;

				ScrollBarVertical.Enabled = IsImageHeightExceed ? true : false;
				ScrollBarHorizontal.Enabled = IsImageWidthExceed ? true : false;
			}
		}
		private void UpdatePictureBox()
		{
			pictureBox.Image = (Image)DisplayImage.Clone();
		}
		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (OriginImage == null)
			{
				return;
			}
			DisplayImage = (Image)e.Result;
			UpdateScrollBar();
			UpdatePictureBox();
			//Console.WriteLine("complete {0}", changeTimes);
		}

		//Zoom
		private float ZoomFactor = 1;
		public Point GetEffectiveMouseLocation(Point MouseLocation) => new Point((int)(MouseLocation.X / ZoomFactor), (int)(MouseLocation.Y / ZoomFactor));
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (BackgroundWorker.IsBusy)
				return;

			object[] parameters = { OriginImage, DisplayImage };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(this, e, parameters));

		}

		//drag
		private Point AnchorPoint = new Point();
		private Point OldBoxPos = new Point();
		private void pictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			this.OnMouseMove(e);
			if (BackgroundWorker.IsBusy)
				return;
			//Console.WriteLine("movenow {0}", changeTimes);
			object[] parameters = { OriginImage, DisplayImage };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(sender, e, parameters));
		}
		private void pictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			this.OnMouseDown(e);
			if (e.Button == MouseButtons.Right)
			{
				AnchorPoint = e.Location;
				OldBoxPos = ImageBoxPos;
				Cursor = new Cursor(((Bitmap)Properties.Resources.grab_cursor50.Clone()).GetHicon());
			}
		}
		private void pictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			this.OnMouseUp(e);
			if (e.Button == MouseButtons.Right)
			{
				Cursor = DefaultCursor;
			}
		}
		private void pictureBox_MouseEnter(object sender, EventArgs e)
		{
			this.OnMouseEnter(e);
		}
		private void pictureBox_MouseLeave(object sender, EventArgs e)
		{
			this.OnMouseLeave(e);
		}

		//scroll 
		private void ScrollBarVertical_Scroll(object sender, ScrollEventArgs e)
		{
			if (BackgroundWorker.IsBusy)
				return;
			object[] parameters = { OriginImage, DisplayImage };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(sender, e, parameters));
		}
		private void ScrollBarHorizontal_Scroll(object sender, ScrollEventArgs e)
		{
			if (BackgroundWorker.IsBusy)
				return;

			object[] parameters = { OriginImage, DisplayImage };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(sender, e, parameters));
		}

		//for developer
		private void UpdateLayout()
		{
			ScrollBarHorizontal.Height = 15;
			ScrollBarVertical.Width = 15;
			ScrollBarHorizontal.Width = this.Width - ScrollBarVertical.Width;
			ScrollBarVertical.Height = this.Height - ScrollBarHorizontal.Height;
			ScrollBarHorizontal.Location = new Point(0, this.Height - ScrollBarHorizontal.Height);
			ScrollBarVertical.Location = new Point(this.Width - ScrollBarVertical.Width, 0);
			pictureBox.Width = this.Width - ScrollBarVertical.Width;
			pictureBox.Height = this.Height - ScrollBarHorizontal.Height;
			pictureBox.Location = new Point(0, 0);
		}
		private void ImageViewer_Resize(object sender, EventArgs e)
		{
      Size = new Size(Clamp(Size.Width, int.MaxValue, 20), Clamp(Size.Height, int.MaxValue, 20));

			UpdateLayout();
			DisplayImage = UpdateDisplayImage(OriginImage);
			UpdateScrollBar();
			UpdatePictureBox();
		}
		private void ImageViewer_Load(object sender, EventArgs e)
		{
			UpdateLayout();
			DisplayImage = UpdateDisplayImage(OriginImage);
			UpdateScrollBar();
			UpdatePictureBox();
		}
	}
}