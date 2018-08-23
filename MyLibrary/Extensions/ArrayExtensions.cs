using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLibrary.Extensions
{
  public static class ArrayExtensions
  {
    public static (int, int) IndexOf(this int[,] array, int value)
    {
      var L1 = array.GetLength(0);
      var L2 = array.GetLength(1);
      for (int i = 0; i < L1; i++)
      {
        for (int j = 0; j < L2; j++)
        {
          var item = array[i, j];
          if (item == value)
          {
            return (i, j);
          }
        }
      }
      //didn't find...
      return (-1, -1);
    }
  }
}
