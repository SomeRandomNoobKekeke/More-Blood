using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{
  public class Mixins
  {
    public static Dictionary<ushort, HullMixin> HullMixins = new();
    public static HullMixin GetHullMixin(Hull hull)
    {
      if (!HullMixins.ContainsKey(hull.ID))
      {
        HullMixins[hull.ID] = new HullMixin(hull);
      }
      return HullMixins[hull.ID];
    }

    public static void Clear()
    {
      HullMixins.Clear();
    }
  }

}