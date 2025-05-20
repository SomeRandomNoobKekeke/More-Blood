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
  public class HullPatches
  {
    public static Stopwatch sw = new Stopwatch();
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(Hull).GetMethod("DrawDecals", AccessTools.all),
        prefix: new HarmonyMethod(typeof(HullPatches).GetMethod("Hull_DrawDecals_Prefix"))
      );

      harmony.Patch(
        original: typeof(Hull).GetMethod("DrawDecals", AccessTools.all),
        postfix: new HarmonyMethod(typeof(HullPatches).GetMethod("Hull_DrawDecals_Postfix"))
      );
    }

    public static void Hull_DrawDecals_Prefix(Hull __instance, SpriteBatch spriteBatch)
    {
      sw.Restart();
    }

    public static void Hull_DrawDecals_Postfix(Hull __instance, SpriteBatch spriteBatch)
    {
      sw.Restart();
      __instance.DrawAdvancedDecals();
      sw.Stop();
      GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:Decals", sw.ElapsedTicks);
      //GameMain.PerformanceCounter.AddElapsedTicks("Draw:Map:BackCharacterItems", -sw.ElapsedTicks);
    }
  }
}