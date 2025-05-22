using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Xml.Linq;

namespace MoreBlood
{




  public class Config
  {
    public BleedingConfig BleedingConfig { get; set; } = new BleedingConfig();


    public IEnumerable<PropertyInfo> Props
    {
      get
      {
        foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
          yield return pi;
        }
      }
    }

    private XElement PackProps(XElement element)
    {
      foreach (PropertyInfo pi in Props)
      {
        if (pi.PropertyType.IsSubclassOf(typeof(Config)))
        {
          XElement prop = new XElement(pi.Name);
          (pi.GetValue(this) as Config).PackProps(prop);
          element.Add(prop);
        }
        else
        {
          element.Add(new XAttribute(pi.Name, pi.GetValue(this).ToString()));
        }
      }

      return element;
    }

    public XElement ToXml()
    {
      return PackProps(new XElement(this.GetType().Name));
    }
  }


}