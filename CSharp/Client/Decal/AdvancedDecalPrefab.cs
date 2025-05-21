using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoreBlood
{
  public class AdvancedDecalPrefab
  {
    public static AdvancedDecalPrefab Backup => Prefabs["blood"];
    public static Dictionary<string, AdvancedDecalPrefab> Prefabs = new()
    {
      ["blood"] = new AdvancedDecalPrefab()
      {
        Colors = new List<ColorPoint>(){
          new ColorPoint(new Color(102, 0, 0, 255), 0.0),
          new ColorPoint(new Color(64, 0, 0, 255), 0.4),
          new ColorPoint(new Color(32, 0, 0, 255), 0.8),
          new ColorPoint(new Color(32, 0, 0, 0), 1.0),
        },
        Sprites = DecalManager.Prefabs["blood"].Sprites,
        LifeTime = 60,

      },
      ["blackblood"] = new AdvancedDecalPrefab()
      {
        Colors = new List<ColorPoint>(){
          new ColorPoint(new Color(0, 0, 0, 255), 0.0),
          new ColorPoint(new Color(0, 0, 0, 255), 0.8),
          new ColorPoint(new Color(0, 0, 0, 0), 1.0),
        },
        Sprites = DecalManager.Prefabs["blackblood"].Sprites,
        LifeTime = 60,
      },
    };

    public static AdvancedDecalPrefab GetPrefab(string name)
    {
      if (Prefabs.ContainsKey(name)) return Prefabs[name];
      return Backup;
    }


    // public static AdvancedDecalPrefab RedBlood = new AdvancedDecalPrefab()
    // {
    //   Colors = new List<ColorPoint>(){
    //     new ColorPoint(new Color(102, 0, 0, 255), 0.0),
    //     new ColorPoint(new Color(32, 0, 0, 255), 0.8),
    //     new ColorPoint(new Color(32, 0, 0, 0), 1.0),
    //   },
    //   Sprites = DecalManager.Prefabs["blood"].Sprites,
    //   LifeTime = 60,
    // };

    // public static AdvancedDecalPrefab BlackBlood = new AdvancedDecalPrefab()
    // {
    //   Colors = new List<ColorPoint>(){
    //     new ColorPoint(new Color(0, 0, 0, 255), 0.0),
    //     new ColorPoint(new Color(0, 0, 0, 255), 0.8),
    //     new ColorPoint(new Color(0, 0, 0, 0), 1.0),
    //   },
    //   Sprites = DecalManager.Prefabs["blackblood"].Sprites,
    //   LifeTime = 60,
    // };

    public List<ColorPoint> Colors = new();
    public List<Sprite> Sprites;
    public double LifeTime;
    public float MaxScale = 1.8f;
    public float MinScale = 0.1f;
  }
}