using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Math;

namespace MyLibrary
{
	public class MyMethods
	{
		/*	TimeIt Example:
		 *	1. 
		 *	Code code = () => { string s = "Your Codes"; };
		 *	TimeIt(code); 
		 *	2.
		 *	TimeIt( () => { string s = "Your Codes"; } );
		 */
		public delegate void Code(); 
		public static void TimeIt(Code code)
		{
			Stopwatch sw = new Stopwatch();//引用stopwatch物件
			sw.Reset();//碼表歸零
			sw.Start();//碼表開始計時
								 //-----目標程式-----//
			code.Invoke();
			//-----目標程式-----//
			sw.Stop();//碼錶停止
			string result = sw.Elapsed.TotalMilliseconds.ToString();
			Console.WriteLine(result);
		}

		/*	RecursiveGetControls Exmaple:	
		 *	List<Control> AllControls = RecursiveGetControls(Form); 
		 */
		public static List<Control> RecursiveGetControls(Form form)
		{
			return RecursiveGetControls(form.Controls);
		}
		public static List<Control> RecursiveGetControls(Control.ControlCollection controls)
		{
			List<Control> controlList = new List<Control>(); //初始化List
			foreach (Control control in controls)
				controlList.Add(control); // 將controls轉型成List				
			List<Control> allControls = RecursiveGetControls(controlList); //真正開始getAllControls
			return allControls;
		}
		public static List<Control> RecursiveGetControls(List<Control> controlList)
		{
			List<Control> opt = new List<Control>(); //不能opt = controlList!!! 會複製到參考型別!!!
			opt.AddRange(controlList);
			IEnumerable<Control> groupControls = from control in controlList
																					 where control is GroupBox | control is TabControl | control is Panel 
																					 select control; //選出controls中的groupControls
			foreach (Control groupControl in groupControls)
				opt.AddRange(RecursiveGetControls(groupControl.Controls)); //遞迴加入groupControls中的控制項
			return opt;
		}

		public static Bitmap ResizeImage(Image image, int width, int height)
		{
			var destRect = new Rectangle(0, 0, width, height);
			var destImage = new Bitmap(width, height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}

		public static Bitmap Crop(Image image, Rectangle rect)
		{
			Bitmap bitmap = new Bitmap(rect.Width, rect.Height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
				graphics.DrawImage(image, -rect.X, -rect.Y);
			return bitmap;
		}

		public static int LinConvert(int value1, int max1, int min1, int max2, int min2)
		{
			float r = (float)(max2 - min2) / (max1 - min1);
			return (int)(min2 + (value1 - min1) * r);
		}

		public static float LinConvert(float value1, float max1, float min1, float max2, float min2)
		{
			float r = (max2 - min2) / (max1 - min1);
			return (min2 + (value1 - min1) * r);
		}

		public static double LinConvert(double value1, double max1, double min1, double max2, double min2)
		{
			double r = (max2 - min2) / (max1 - min1);
			return (min2 + (value1 - min1) * r);
		}

		public static int Clamp(float value, int Max, int Min)
		{
			if (Min > Max)
				Swap(ref Max, ref Min);

			if (value > Max)
				return Max;
			else if (value < Min)
				return Min;
			else
				return (int)value;
		}

		public static void Swap<T>(ref T x, ref T y)
		{
			T tmp = x;
			x = y;
			y = tmp;
		}

		public static bool IsIn<T>(int value, int Max, int Min)
		{
			if (Min > Max)
				Swap(ref Max, ref Min);
			return (value <= Max && value >= Min) ? true : false;
		}

		public static double LogBase(double Base, double num)
		{
			return Log(num) / Log(Base);
		}
	}
}
