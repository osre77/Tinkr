using System;
using Microsoft.SPOT;
using Skewworks.NETMF;
using Skewworks.NETMF.Controls;
using Skewworks.NETMF.Resources;

namespace TestSandbox
{
   public static class MenuHelper
   {
      public static MenuStrip AddMenuStrip(this Form form, params MenuItem[] items)
      {
         var strip = new MenuStrip("MainMenu", Fonts.Droid11, items, 0, 0, form.Width, 28);
         form.AddChild(strip);
         return strip;
      }

      public static MenuItem CreateMenuItem(string text, params MenuItem[] items)
      {
         var item = new MenuItem(text, text);
         item.AddMenuItems(items);
         return item;
      }

      public static MenuItem CreateMenuItem(string text, OnTap tap)
      {
         var item = new MenuItem(text, text);
         item.Tap += tap;
         return item;
      }
   }
}
