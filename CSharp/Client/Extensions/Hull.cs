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
    public static void DrawAdvancedDecals(this Hull hull)
    {
      HullMixin mixin = Mixins.GetHullMixin(hull);
    }
  }
}