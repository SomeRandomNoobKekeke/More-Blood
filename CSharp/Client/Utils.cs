using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;

namespace MoreBlood
{
  public static class Utils
  {
    public static Random Random = new Random();
    public static float Pi2 = (float)(Math.PI * 2.0);
    public static float RandomMult(float decrement, float increment)
      => 1 + (decrement + increment) * Random.NextSingle() - decrement;
  }
}