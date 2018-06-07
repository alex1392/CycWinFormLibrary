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


  public sealed class Colors
  {
    public static Color Blue = Color.FromArgb(67, 133, 246);
    public static Color Green = Color.FromArgb(52, 168, 85);
    public static Color Red = Color.FromArgb(234, 66, 53);
    public static Color Black = Color.FromArgb(0, 0, 0);
    public static Color White = Color.FromArgb(255, 255, 255);
    public static Color Silver = Color.FromArgb(85, 85, 85);
    public static Color Lime = Color.FromArgb(142, 188, 0);
    public static Color Teal = Color.FromArgb(0, 170, 173);
    public static Color Orange = Color.FromArgb(243, 119, 53);
    public static Color Brown = Color.FromArgb(165, 81, 0);
    public static Color Pink = Color.FromArgb(231, 113, 189);
    public static Color Magenta = Color.FromArgb(255, 0, 148);
    public static Color Purple = Color.FromArgb(124, 65, 153);
    public static Color Yellow = Color.FromArgb(255, 196, 37);
  }

  public enum ScrollBarOrientation
  {
    Horizontal,
    Vertical
  }

  public enum RangeSliderOrientation
  {
    Horizontal,
    Vertical,
  }

  public enum SliderOrientation
  { 
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
    public object[] results;

    public BackgroundArgs(object sender)
    {
      this.sender = sender;
    }
    public BackgroundArgs(EventArgs e, object[] parameters)
    {
      this.e = e;
      this.parameters = parameters;
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

  public class PixelImage : ICloneable
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
    public PixelImage()
    {

    }

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
      return new PixelImage()
      {
        _Bitmap = (Bitmap)this._Bitmap.Clone(),
        _Pixel = (byte[])this._Pixel.Clone(),
        Byte = this.Byte
      };
    } //比new PixelIamge(bitmap)快
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
