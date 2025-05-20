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
  public class MeasureDecalDrawTime
  {
    public static Stopwatch sw = new Stopwatch();
    public static void PatchAll()
    {
      Mod.Harmony.Patch(
        original: typeof(Hull).GetMethod("DrawDecals", AccessTools.all),
        prefix: new HarmonyMethod(typeof(MeasureDecalDrawTime).GetMethod("Hull_DrawDecals_Prefix"))
      );

      Mod.Harmony.Patch(
        original: typeof(Hull).GetMethod("DrawDecals", AccessTools.all),
        postfix: new HarmonyMethod(typeof(MeasureDecalDrawTime).GetMethod("Hull_DrawDecals_Postfix"))
      );
    }

    public static void Hull_DrawDecals_Prefix(SpriteBatch spriteBatch)
    {
      sw.Restart();
    }

    public static void Hull_DrawDecals_Postfix(SpriteBatch spriteBatch)
    {
      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Decals", sw.ElapsedTicks);
    }
  }
}