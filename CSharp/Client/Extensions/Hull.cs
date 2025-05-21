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
  public static class HullExtensions
  {
    public static void AddDecal(this Hull hull, AdvancedDecal decal, Vector2 worldPosition)
    {
      decal.ConnectToHull(worldPosition, hull);
    }
  }
}