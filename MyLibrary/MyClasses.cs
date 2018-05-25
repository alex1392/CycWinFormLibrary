using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyLibrary
{
	public enum ScrollBarOrientation
	{
		Horizontal,
		Vertical
	}

	public enum SliderOrientation
	{
		Horizontal,
		Vertical,
		Down,
		Right,
		Up,
		Left
	}

	public class BackgroundArgs
	{
		public object sender;
		public EventArgs e;
		public object[] parameters;

		public BackgroundArgs(object sender)
		{
			this.sender = sender;
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

	public class ControlAnchor
	{
		public int Top;
		public int Left;
		public int Width;
		public int Height;
	}

	public class PixelImage: ICloneable
	{
		private Bitmap _Bitmap;
		public Bitmap Bitmap
		{
			get => _Bitmap;
			set
			{
				_Bitmap = value;
				Bitmap2Pixel();
			}
		}
		private byte[] _Pixel;
		public byte[] Pixel
		{
			get => _Pixel;
			set
			{
				_Pixel = value;
				Pixel2Bitmap();
			}
		}
		public int Byte = 4; //根據Format32bppArgb
		public int Stride => Width * Byte;  //根據Format32bppArgb
		public int Width => _Bitmap.Width; 
		public int Height => _Bitmap.Height; 
		public Size Size => _Bitmap.Size; 
		public static PixelFormat PixelFormat = PixelFormat.Format32bppArgb;

		#region Constructors
		public PixelImage(Bitmap bitmap)
		{
			this.Bitmap = new Bitmap(bitmap); //更新pixel
		}

		public PixelImage(Size size)
		{
			_Bitmap = new Bitmap(size.Width, size.Height); //不更新pixel
		}

		public PixelImage(byte[] pixel, Size size)
		{
			_Bitmap = new Bitmap(size.Width, size.Height); //不更新pixel
			this.Pixel = pixel; //更新bitmap
		}

		public object Clone()
		{
			PixelImage obj = new PixelImage(this.Bitmap);
			return obj;
		}
		#endregion

		#region Private Methods
		private void Pixel2Bitmap()
		{
				//將image鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的imageData
				BitmapData bitmapData = _Bitmap.LockBits(
				new Rectangle(0, 0, _Bitmap.Width, _Bitmap.Height),
				ImageLockMode.ReadOnly,
				PixelFormat);

				//複製pixel到bitmapData中
				Marshal.Copy(_Pixel, 0, bitmapData.Scan0, _Pixel.Length);

				//解鎖
				_Bitmap.UnlockBits(bitmapData);
			
		}

		private void Bitmap2Pixel()
		{
				//將image鎖定到系統內的記憶體的某個區塊中，並將這個結果交給BitmapData類別的imageData
				BitmapData bitmapData = _Bitmap.LockBits(
					new Rectangle(0, 0, _Bitmap.Width, _Bitmap.Height),
					ImageLockMode.ReadOnly,
					PixelFormat);

				//初始化pixel陣列，用來儲存所有像素的訊息
				_Pixel = new byte[bitmapData.Stride * _Bitmap.Height];

				//複製imageData的RGB信息到pixel陣列中
				Marshal.Copy(bitmapData.Scan0, _Pixel, 0, _Pixel.Length);

				//解鎖
				_Bitmap.UnlockBits(bitmapData);

		}
		#endregion
	}
}
