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
using System.IO;
namespace MoreBlood
{
  public class Config : ConfigBase
  {
    public static string DefaultConfigPath = "Config.xml";
    public BleedingConfig BleedingConfig { get; set; } = new BleedingConfig();
    public float DecalDrawDepth { get; set; } = 0.6f;
    public float DecalCreationInterval { get; set; } = 0.03f;
    public float GlobalBloodAmount { get; set; } = 1.0f;
  }

  public class ConfigBase
  {
    private IEnumerable<PropertyInfo> Props
    {
      get
      {
        foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
          yield return pi;
        }
        yield break;
      }
    }

    private XElement PackProps(XElement element)
    {
      foreach (PropertyInfo pi in Props)
      {
        if (pi.PropertyType.IsSubclassOf(typeof(ConfigBase)))
        {
          XElement prop = new XElement(pi.Name);
          (pi.GetValue(this) as ConfigBase).PackProps(prop);
          element.Add(prop);
        }
        else
        {
          element.Add(new XElement(pi.Name, pi.GetValue(this).ToString()));
        }
      }

      return element;
    }

    public XElement ToXML()
    {
      return PackProps(new XElement(this.GetType().Name));
    }

    public void Save(string path)
    {
      XDocument xdoc = new XDocument();
      xdoc.Add(this.ToXML());
      xdoc.Save(path);
    }

    public void FromXML(XElement element)
    {
      foreach (XElement child in element.Elements())
      {
        PropertyInfo pi = this.GetType().GetProperty(child.Name.ToString());
        if (pi is null) continue;

        if (pi.PropertyType.IsSubclassOf(typeof(ConfigBase)))
        {
          ConfigBase subConfig = (ConfigBase)pi.GetValue(this);
          if (subConfig is null)
          {
            subConfig = (ConfigBase)Activator.CreateInstance(pi.PropertyType);
            pi.SetValue(this, subConfig);
          }

          subConfig.FromXML(child);
        }
        else
        {
          pi.SetValue(this, Parser.Parse(child.Value, pi.PropertyType));
        }
      }
    }

    public void Load(string path)
    {
      if (!File.Exists(path))
      {
        Mod.Warning($"Couldn't load config from {path}");
        return;
      }
      XDocument xdoc = XDocument.Load(path);
      this.FromXML(xdoc.Root);
    }

    public void Print()
    {
      Mod.Log(this.ToXML());
    }

  }
}