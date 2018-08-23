using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MyLibrary.Methods.Math;

namespace MyLibrary.Methods
{
  public static class TimeConverter
  {
    /// <summary>
    /// 將總時間轉換成MM:SS:MS / MM:SS:TIC格式的字串
    /// </summary>
    /// <param name="totalTime">以毫秒為單位的總時間</param>
    public static (string min, string sec, string ms, string tic) ToString(int totalTime)
    {
      totalTime /= 10; //單位 百分之一秒
      totalTime = Clamp(totalTime, 60 * 60 * 100 - 1, 0);
      var min = (totalTime / (60 * 100)).ToString().PadLeft(2, '0');
      totalTime %= 60 * 100;
      var sec = (totalTime / 100).ToString().PadLeft(2, '0');
      totalTime %= 100;
      var ms = (totalTime * 10).ToString().PadLeft(2, '0');
      var tic = ((int)(totalTime * 60d / 100)).ToString().PadLeft(2, '0');
      return (min, sec, ms, tic);
    }

    /// <summary>
    /// 將總時間轉換成MM:SS:MS / MM:SS:TIC格式的字串
    /// </summary>
    /// <param name="totalTime">以毫秒為單位的總時間</param>
    public static (string min, string sec, string ms, string tic) ToString(int? totalTime)
    {
      if (totalTime == null)
      {
        return ("--", "--", "--", "--");
      }
      else
      {
        return ToString((int)totalTime);
      }
    }

    /// <summary>
    /// 將總時間轉換成MM:SS:MS / MM:SS:TIC格式的字串
    /// </summary>
    /// <param name="timeSpan">總時間</param>
    /// <returns></returns>
    public static (string min, string sec, string ms, string tic) ToString(TimeSpan timeSpan)
    {
      var min = timeSpan.Minutes.ToString().PadLeft(2, '0');
      var sec = timeSpan.Seconds.ToString().PadLeft(2, '0');
      var ms = (timeSpan.Milliseconds / 10).ToString().PadLeft(2, '0');
      var tic = ((int)(timeSpan.Milliseconds * 60d / 1000)).ToString().PadLeft(2, '0');
      return (min, sec, ms, tic);
    }
  }
}
