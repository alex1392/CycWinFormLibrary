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

    /*	GetAllControls Exmaple:	
		 *	List<Control> AllControls = GetAllControls(Form); 
		 */
    public static List<Control> GetAllControls(Form form)
    {
      return GetAllControls(ToList(form.Controls));
    }
    public static List<Control> ToList(Control.ControlCollection controls)
    {
      List<Control> controlList = new List<Control>();
      foreach (Control control in controls)
        controlList.Add(control);
      return controlList;
    }
    public static List<Control> GetAllControls(List<Control> inputList)
    {
      //複製inputList到outputList
      List<Control> outputList = new List<Control>(inputList);

      //取出inputList中的容器
      IEnumerable<Control> containers = from control in inputList
                                        where
              control is GroupBox |
              control is TabControl |
              control is Panel |
              control is FlowLayoutPanel |
              control is TableLayoutPanel |
              control is ContainerControl
                                        select control;


      foreach (Control container in containers)
      {
        //遞迴加入容器內的容器與控制項
        outputList.AddRange(GetAllControls(ToList(container.Controls)));
      }
      return outputList;
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

		public static float Interpolate(int StartValue, int EndValue, float Ratio)
		{
			return StartValue + Ratio * (EndValue - StartValue);
		}

		public static Color Interpolate(Color StartColor, Color EndColor, float Ratio)
		{
      int A = (int)Interpolate(StartColor.A, EndColor.A, Ratio);
      int R = (int)Interpolate(StartColor.R, EndColor.R, Ratio);
      int G = (int)Interpolate(StartColor.G, EndColor.G, Ratio);
      int B = (int)Interpolate(StartColor.B, EndColor.B, Ratio);
			return Color.FromArgb(A, R, G, B);
		}

    public static Color WriteOut(Color color, int value)
    {
      int R = Clamp(color.R + value, 255, 0);
      int G = Clamp(color.G + value, 255, 0);
      int B = Clamp(color.B + value, 255, 0);
      return Color.FromArgb(color.A, R, G, B);
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

		public static bool IsIn(int value, int Max, int Min)
		{
			if (Min > Max)
				Swap(ref Max, ref Min);
			return (value <= Max && value >= Min) ? true : false;
		}

		public static bool IsIn(float value, int Max, int Min)
		{
			if (Min > Max)
				Swap(ref Max, ref Min);
			return (value <= Max && value >= Min) ? true : false;
		}

    public static bool IsIn(float value, int Max, int Min, bool excludeBoundary)
    {
      if (!excludeBoundary)
        return IsIn(value, Max, Min);
      else
      {
        if (Min > Max)
          Swap(ref Max, ref Min);
        return (value < Max && value > Min) ? true : false;
      }
    }

		public static double LogBase(double Base, double num)
		{
			return Log(num) / Log(Base);
		}

    public static TextFormatFlags GetTextFormatFlags(ContentAlignment textAlign)
    {
      TextFormatFlags controlFlags = TextFormatFlags.EndEllipsis;

      switch (textAlign)
      {
        case ContentAlignment.TopLeft:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.Left;
          break;
        case ContentAlignment.TopCenter:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.TopRight:
          controlFlags |= TextFormatFlags.Top | TextFormatFlags.Right;
          break;
        case ContentAlignment.MiddleLeft:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
          break;
        case ContentAlignment.MiddleCenter:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.MiddleRight:
          controlFlags |= TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
          break;
        case ContentAlignment.BottomLeft:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
          break;
        case ContentAlignment.BottomCenter:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
          break;
        case ContentAlignment.BottomRight:
          controlFlags |= TextFormatFlags.Bottom | TextFormatFlags.Right;
          break;
      }

      return controlFlags;
    }

    public static void DrawRoundRectangle(Graphics g, Pen pen, Rectangle rect, int cornerRadius)
    {
      using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
      {
        g.DrawPath(pen, path);
      }
    }

    public static void FillRoundRectangle(Graphics g, Brush brush, Rectangle rect, int cornerRadius)
    {
      using (GraphicsPath path = CreateRoundedRectanglePath(rect, cornerRadius))
      {
        g.FillPath(brush, path);
      }
    }

    public static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
    {
      GraphicsPath roundedRect = new GraphicsPath();
      roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
      roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
      roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
      roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
      roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
      roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
      roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
      roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
      roundedRect.CloseFigure();
      return roundedRect;
    }

    public static void DrawRoundShadow(Graphics g, Rectangle rect, int width)
    {
      g.SmoothingMode = SmoothingMode.AntiAlias;
      Color color = Color.FromArgb(0, 0, 0, 0);
      int penWidth = 3;
      using (Pen pen = new Pen(color, penWidth))
      {
        for (int i = -penWidth; i < width; i++)
        {
          pen.Color = Color.FromArgb((50 / width) * (width-i), color);
          g.DrawEllipse(pen, new Rectangle(rect.X - i, rect.Y - i, rect.Width + 2 * i, rect.Height + 2 * i));
        }
      }
    }
  }
}
