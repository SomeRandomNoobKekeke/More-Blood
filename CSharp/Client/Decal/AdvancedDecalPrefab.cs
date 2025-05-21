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
    public static AdvancedDecalPrefab RedBlood = new AdvancedDecalPrefab()
    {
      Colors = new List<ColorPoint>(){
        new ColorPoint(new Color(255, 0, 0, 255), 0.0),
        new ColorPoint(new Color(32, 0, 0, 255), 0.9),
        new ColorPoint(new Color(32, 0, 0, 0), 1.0),
      },
      Sprites = DecalManager.Prefabs["blood"].Sprites,
      LifeTime = 5,
    };

    public List<ColorPoint> Colors = new();
    public List<Sprite> Sprites;
    public double LifeTime;
  }
}