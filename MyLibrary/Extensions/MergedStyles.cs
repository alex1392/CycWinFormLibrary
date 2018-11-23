using System;
using System.Windows;
using System.Windows.Markup;

namespace MyLibrary.Extensions
{
  [MarkupExtensionReturnType(typeof(Style))]
  public class MergedStyles : MarkupExtension
  {
    public Style BasedOn { get; set; }
    public Style MergeStyle { get; set; }

    public override object ProvideValue(IServiceProvider
                                        serviceProvider)
    {
      if (null == MergeStyle)
        return BasedOn;

      Style newStyle = new Style(BasedOn.TargetType,
                                 BasedOn);

      MergeWithStyle(newStyle, MergeStyle);

      return newStyle;
    }

    private static void MergeWithStyle(Style style,
                                       Style mergeStyle)
    {
      // Recursively merge with any Styles this Style
      // might be BasedOn.
      if (mergeStyle.BasedOn != null)
      {
        MergeWithStyle(style, mergeStyle.BasedOn);
      }

      // Merge the Setters...
      foreach (var setter in mergeStyle.Setters)
        style.Setters.Add(setter);

      // Merge the Triggers...
      foreach (var trigger in mergeStyle.Triggers)
        style.Triggers.Add(trigger);
    }
  }
}
