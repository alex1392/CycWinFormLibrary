using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MyLibrary.Methods
{
  /// <summary>
  /// 提供序列化與反序列化的泛型方法
  /// </summary>
  public static class XmlGenericSerializer
  {
    /// <summary>
    /// 將物件序列化為XML字串
    /// </summary>
    /// <typeparam name="T">物件類別</typeparam>
    /// <param name="obj">物件</param>
    /// <returns>XML字串</returns>
    public static string Serialize<T>(T obj) where T : class
    {
      XmlSerializer serializer = new XmlSerializer(typeof(T));
      var stringWriter = new StringWriter();
      using (var xmlWriter = XmlWriter.Create(stringWriter))
      {
        serializer.Serialize(xmlWriter, obj);
        return stringWriter.ToString();
      }
    }

    /// <summary>
    /// 將XML字串反序列化為物件，若反序列化失敗，回傳null
    /// </summary>
    /// <typeparam name="T">物件類別</typeparam>
    /// <param name="xmlString">XML字串</param>
    /// <returns>物件</returns>
    public static T Deserialize<T>(string xmlString) where T : class
    {
      XmlSerializer deserializer = new XmlSerializer(typeof(T));
      using (TextReader reader = new StringReader(xmlString))
      {
        try
        {
          return deserializer.Deserialize(reader) as T;

        }
        catch (InvalidOperationException)
        {
          return null;
          
        }
      };
    }
  }
}
