using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using static MyLibrary.Methods.Math;

namespace MyLibrary.Methods
{
  public class Drawing
  {
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

    public static Color Interpolate(Color StartColor, Color EndColor, float Ratio)
    {
      int A = (int)Math.Interpolate(StartColor.A, EndColor.A, Ratio);
      int R = (int)Math.Interpolate(StartColor.R, EndColor.R, Ratio);
      int G = (int)Math.Interpolate(StartColor.G, EndColor.G, Ratio);
      int B = (int)Math.Interpolate(StartColor.B, EndColor.B, Ratio);
      return Color.FromArgb(A, R, G, B);
    }

    public static Color WriteOut(Color color, int value)
    {
      int R = Clamp(color.R + value, 255, 0);
      int G = Clamp(color.G + value, 255, 0);
      int B = Clamp(color.B + value, 255, 0);
      return Color.FromArgb(color.A, R, G, B);
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
          pen.Color = Color.FromArgb((50 / width) * (width - i), color);
          g.DrawEllipse(pen, new Rectangle(rect.X - i, rect.Y - i, rect.Width + 2 * i, rect.Height + 2 * i));
        }
      }
    }

    public static PointF GetCentroid(Point[] points)
    {
      PointF centroid = new PointF(0, 0);
      foreach (PointF point in points)
      {
        centroid.X += point.X;
        centroid.Y += point.Y;
      }
      centroid.X /= points.Length;
      centroid.Y /= points.Length;
      return centroid;
    }

    public static double GetNormal(PointF vector)
    {
      return Sqrt(Pow(vector.X, 2) + Pow(vector.Y, 2));
    }

    public static Point[] ZoomPolygon(Point[] inputPoints, float ratio)
    {
      PointF centroid = GetCentroid(inputPoints);
      Point[] OutputPoints = new Point[inputPoints.Length];
      PointF vector;
      for (int i = 0; i < inputPoints.Length; i++)
      {
        vector = new PointF(inputPoints[i].X - centroid.X,
                            inputPoints[i].Y - centroid.Y);

        OutputPoints[i].X = (int)(centroid.X + vector.X * ratio);
        OutputPoints[i].Y = (int)(centroid.Y + vector.Y * ratio);
      }
      return OutputPoints;
    }

  }
}
