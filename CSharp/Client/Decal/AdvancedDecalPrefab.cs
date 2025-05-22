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
  public class AdvancedDecalPrefab
  {
    public static string DefaultPrefabsPath = "Prefabs";
    public static string DefaultBasePrefab = "blood";
    public static AdvancedDecalPrefab Backup = new AdvancedDecalPrefab()
    {
      BasedOn = "blood",
      Colors = new List<ColorPoint>(){
          new ColorPoint(new Color(102, 0, 0, 255), 0.0),
          new ColorPoint(new Color(64, 0, 0, 255), 0.4),
          new ColorPoint(new Color(32, 0, 0, 255), 0.8),
          new ColorPoint(new Color(32, 0, 0, 0), 1.0),
        },
      LifeTime = 60,
    };
    public static Dictionary<string, AdvancedDecalPrefab> Prefabs = new();

    public static AdvancedDecalPrefab GetPrefab(string name)
    {
      if (Prefabs.ContainsKey(name)) return Prefabs[name];
      return Backup;
    }

    public static void SavePrefabs(string path)
    {
      if (!Directory.Exists(path))
      {
        Mod.Warning($"Guh? {path}");
        return;
      }

      foreach (string key in Prefabs.Keys)
      {
        Prefabs[key].Save(Path.Combine(path, key + ".xml"));
      }
    }

    public static void LoadPrefabs(string path)
    {
      if (!Directory.Exists(path))
      {
        Mod.Warning($"Guh? {path}");
        return;
      }

      Prefabs.Clear();
      foreach (string file in Directory.GetFiles(path, "*.xml"))
      {
        //HACK 
        Prefabs[Path.GetFileNameWithoutExtension(file)] = AdvancedDecalPrefab.Load(file);
      }
    }

    public void Save(string path)
    {
      XDocument xdoc = new XDocument();
      xdoc.Add(this.ToXML());
      xdoc.Save(path);
    }

    public static AdvancedDecalPrefab Load(string path)
    {
      if (!File.Exists(path))
      {
        Mod.Warning($"Couldn't load AdvancedDecalPrefab from {path}");
        return Backup;
      }
      XDocument xdoc = XDocument.Load(path);
      return FromXML(xdoc.Root);
    }

    private string basedOn; public string BasedOn
    {
      get => basedOn;
      set
      {
        basedOn = value;
        if (DecalManager.Prefabs.ContainsKey(value))
        {
          Sprites = DecalManager.Prefabs[value].Sprites;
        }
        else
        {
          Mod.Warning($"Couldn't steal sprites from vanilla decal prefab [{value}], using default blood sprites");
          Sprites = DecalManager.Prefabs[DefaultBasePrefab].Sprites;
        }
      }
    }
    public List<ColorPoint> Colors = new();
    public List<Sprite> Sprites;
    public double LifeTime;
    public float MaxScale = 1.8f;
    public float MinScale = 0.1f;

    public XElement ToXML()
    {
      XElement element = new XElement("AdvancedDecalPrefab",
        new XAttribute("BasedOn", BasedOn),
        new XAttribute("LifeTime", LifeTime),
        new XAttribute("MinScale", MinScale),
        new XAttribute("MaxScale", MaxScale)
      );

      foreach (ColorPoint cp in Colors)
      {
        element.Add(cp.ToXML());
      }

      return element;
    }

    public static AdvancedDecalPrefab FromXML(XElement element)
    {
      AdvancedDecalPrefab prefab = new AdvancedDecalPrefab();
      if (element.Attribute("BasedOn") != null)
      {
        prefab.BasedOn = element.Attribute("BasedOn").Value;
      }
      else
      {
        prefab.BasedOn = DefaultBasePrefab;
      }

      if (element.Attribute("LifeTime") != null)
      {
        prefab.LifeTime = Parser.Parse<double>(element.Attribute("LifeTime").Value);
      }

      if (element.Attribute("MinScale") != null)
      {
        prefab.MinScale = Parser.Parse<float>(element.Attribute("MinScale").Value);
      }
      if (element.Attribute("MaxScale") != null)
      {
        prefab.MaxScale = Parser.Parse<float>(element.Attribute("MaxScale").Value);
      }

      foreach (XElement cp in element.Elements("ColorPoint"))
      {
        prefab.Colors.Add(ColorPoint.FromXML(cp));
      }

      return prefab;
    }

    public static void PrintPrefabs()
    {
      foreach (string key in Prefabs.Keys)
      {
        Mod.Log($"Prefabs[{key}] = {Prefabs[key].ToXML()}");
      }
    }
  }
}