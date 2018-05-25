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
using MyLibrary;

namespace MyLibrary.Controls
{
	[DefaultEvent("Click")]
	public partial class ImageViewer : UserControl
	{
		public ImageViewer()
		{
			InitializeComponent();
		}
		private Image OriginImage;
		private Image DisplayImage;
		[Category("Metro Appearance")]
		public Image Image
		{
			get => OriginImage;
			set
			{
				if (value == null)
					return;
				else
				{
					OriginImage = new Bitmap(value);
					DisplayImage = new Bitmap(pictureBox.Width, pictureBox.Height);
					UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
				}
			}
		}

		//Paint
		private int EffectivePictureBoxWidth => (int)(pictureBox.Width / ZoomFactor);
		private int EffectivePictureBoxHeight => (int)(pictureBox.Height / ZoomFactor);
		private bool IsImageWidthExceed => (OriginImage.Width > EffectivePictureBoxWidth) ? true : false;
		private bool IsImageHeightExceed => (OriginImage.Height > EffectivePictureBoxHeight) ? true : false;
		public Point ImageBoxPos
		{
			get => _ImageBoxPos;
			private set
			{
				if (OriginImage == null)
					return;
				_ImageBoxPos.X = IsImageWidthExceed ?
					MyMethods.Clamp(value.X, OriginImage.Width - EffectivePictureBoxWidth, 0) : 0;
				_ImageBoxPos.Y = IsImageHeightExceed ?
					MyMethods.Clamp(value.Y, OriginImage.Height - EffectivePictureBoxHeight, 0) : 0;
			}
		}
		private Point _ImageBoxPos = new Point(0, 0);
		private void UpdateImage(Image OriginImage, Image DisplayImage, ScrollBar ScrollBarHorizontal, ScrollBar ScrollBarVertical)
		{
			UpdatePictureBox(OriginImage, DisplayImage);
			UpdateScrollBar(ScrollBarHorizontal, ScrollBarVertical);
		}
		private void UpdatePictureBox(Image OriginImage, Image DisplayImage)
		{
			Rectangle CropRectangle = new Rectangle
			{
				X = ImageBoxPos.X,
				Y = ImageBoxPos.Y,
				Width = EffectivePictureBoxWidth,
				Height = EffectivePictureBoxHeight,
			};

			Rectangle DestRect = pictureBox.ClientRectangle;
			using (var graphics = Graphics.FromImage(DisplayImage))
			{
				graphics.DrawImage(OriginImage, DestRect, CropRectangle, GraphicsUnit.Pixel);
			}
		}
		private void UpdateScrollBar(ScrollBar ScrollBarHorizontal, ScrollBar ScrollBarVertical)
		{
			ScrollBarHorizontal.Maximum = OriginImage.Width - EffectivePictureBoxWidth;
			ScrollBarHorizontal.Value = ImageBoxPos.X;
			ScrollBarHorizontal.ThumbLength = ScrollBarHorizontal.Width * EffectivePictureBoxWidth / OriginImage.Width;
			ScrollBarHorizontal.SmallChange = (int)(ScrollBarHorizontal.Maximum * ScrollBarHorizontal.ThumbLength / ScrollBarHorizontal.BarLength / 10f);
			ScrollBarHorizontal.LargeChange = (int)(ScrollBarHorizontal.Maximum * ScrollBarHorizontal.ThumbLength / ScrollBarHorizontal.BarLength / 5f);
			ScrollBarHorizontal.MouseWheelBarPartitions = (int)(10 * ScrollBarHorizontal.BarLength / (float)ScrollBarHorizontal.ThumbLength);

			ScrollBarVertical.Maximum = OriginImage.Height - EffectivePictureBoxHeight;
			ScrollBarVertical.Value = ImageBoxPos.Y;
			ScrollBarVertical.ThumbLength = ScrollBarVertical.Height * EffectivePictureBoxHeight / OriginImage.Height;
			ScrollBarVertical.SmallChange = (int)(ScrollBarVertical.Maximum * ScrollBarVertical.ThumbLength / ScrollBarVertical.BarLength / 10f);
			ScrollBarVertical.LargeChange = (int)(ScrollBarVertical.Maximum * ScrollBarVertical.ThumbLength / ScrollBarVertical.BarLength / 5f);
			ScrollBarVertical.MouseWheelBarPartitions = (int)(10 * ScrollBarVertical.BarLength / (float)ScrollBarVertical.ThumbLength);
		}
		

		//background worker
		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs DoWork_e)
		{
			BackgroundArgs args = (BackgroundArgs)DoWork_e.Argument;
			if (args.e is MouseEventArgs)
			{
				MouseEventArgs e = args.e as MouseEventArgs;
				if (args.sender == pictureBox && e.Button == MouseButtons.Right && e.Delta == 0)
				{
					Image OriginImage = (Image)args.parameters[0];
					Image DisplayImage = (Image)args.parameters[1];
					ScrollBar ScrollBarHorizontal = (ScrollBar)args.parameters[2];
					ScrollBar ScrollBarVertical = (ScrollBar)args.parameters[3];

					int dX, dY;
					dX = (int)((e.X - AnchorPoint.X) / ZoomFactor);
					dY = (int)((e.Y - AnchorPoint.Y) / ZoomFactor);
					ImageBoxPos = new Point(OldBoxPos.X - dX, OldBoxPos.Y - dY);
					UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
				}//drag
				else if (args.sender == this && e.Delta != 0 && pictureBox.ClientRectangle.Contains(e.Location))
				{
					Image OriginImage = (Image)args.parameters[0];
					Image DisplayImage = (Image)args.parameters[1];
					ScrollBar ScrollBarHorizontal = (ScrollBar)args.parameters[2];
					ScrollBar ScrollBarVertical = (ScrollBar)args.parameters[3];

					float factor = (e.Delta > 0) ? 1.1f : 0.9f;
					ZoomFactor *= factor;
					Point EffectiveMouseLocation = GetEffectiveMouseLocation(e.Location);
					ImageBoxPos = new Point((int)(ImageBoxPos.X + EffectiveMouseLocation.X * (1 - 1 / factor)), (int)(ImageBoxPos.Y + EffectiveMouseLocation.Y * (1 - 1 / factor)));
					UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
				}//zoom
			}
			else if (args.e is ScrollEventArgs)
			{
				ScrollEventArgs e = args.e as ScrollEventArgs;
				if (args.sender == ScrollBarVertical)
				{
					Image OriginImage = (Image)args.parameters[0];
					Image DisplayImage = (Image)args.parameters[1];
					ScrollBar ScrollBarHorizontal = (ScrollBar)args.parameters[2];
					ScrollBar ScrollBarVertical = (ScrollBar)args.parameters[3];

					ImageBoxPos = new Point(ImageBoxPos.X, e.NewValue);
					UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
				}
				else if (args.sender == ScrollBarHorizontal)
				{
					Image OriginImage = (Image)args.parameters[0];
					Image DisplayImage = (Image)args.parameters[1];
					ScrollBar ScrollBarHorizontal = (ScrollBar)args.parameters[2];
					ScrollBar ScrollBarVertical = (ScrollBar)args.parameters[3];

					ImageBoxPos = new Point(e.NewValue, ImageBoxPos.Y);
					UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
				}
			}
		}
		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			pictureBox.Image = DisplayImage;
			ScrollBarVertical.Enabled = IsImageHeightExceed ? true : false;
			ScrollBarHorizontal.Enabled = IsImageWidthExceed ? true : false;
		}

		//Zoom
		private float ZoomFactor = 1;
		public Point GetEffectiveMouseLocation(Point MouseLocation) => new Point((int)(MouseLocation.X / ZoomFactor), (int)(MouseLocation.Y / ZoomFactor));
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (BackgroundWorker.IsBusy)
				return;
			object[] parameters = { OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(this, e));

		}

		//drag
		private Point AnchorPoint = new Point();
		private Point OldBoxPos = new Point();
		private void pictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			this.OnMouseMove(e);
			if (BackgroundWorker.IsBusy)
				return;
			object[] parameters = { OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(sender, e, parameters));
		}
		private void pictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			this.OnMouseDown(e);
			if (e.Button == MouseButtons.Right)
			{
				AnchorPoint = e.Location;
				OldBoxPos = ImageBoxPos;
				Cursor = new Cursor(new Bitmap(Properties.Resources.grab_cursor50).GetHicon());
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
			object[] parameters = { OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical };
			BackgroundWorker.RunWorkerAsync(new BackgroundArgs(sender, e, parameters));
		}
		private void ScrollBarHorizontal_Scroll(object sender, ScrollEventArgs e)
		{
			if (BackgroundWorker.IsBusy)
				return;
			object[] parameters = { OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical };
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

			DisplayImage = new Bitmap(pictureBox.Width, pictureBox.Height);
		}
		private void ImageViewer_Resize(object sender, EventArgs e)
		{
			UpdateLayout();
		}
		private void ImageViewer_Load(object sender, EventArgs e)
		{
			if (Image == null)
			{
				Image = new Bitmap(pictureBox.Width, pictureBox.Height);
			}
			UpdateLayout();
			UpdateImage(OriginImage, DisplayImage, ScrollBarHorizontal, ScrollBarVertical);
			BackgroundWorker_RunWorkerCompleted(null, null);
		}

		
	}
}