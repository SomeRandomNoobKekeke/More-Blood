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
  public class DrawBloodInASeparateLayer
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(GameScreen).GetMethod("DrawMap", AccessTools.all),
        prefix: new HarmonyMethod(typeof(DrawBloodInASeparateLayer).GetMethod("GameScreen_DrawMap_Prefix"))
      );

      harmony.Patch(
        original: typeof(Submarine).GetMethod("DrawBack", AccessTools.all),
        prefix: new HarmonyMethod(typeof(DrawBloodInASeparateLayer).GetMethod("Submarine_DrawBack_Prefix"))
      );
    }


    public static int SubDrawCall;
    public static void GameScreen_DrawMap_Prefix(GameScreen __instance, GraphicsDevice graphics, SpriteBatch spriteBatch, double deltaTime)
    {
      SubDrawCall = 0;
    }

    public static bool Submarine_DrawBack_Prefix(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
    {
      SubDrawCall++;

      if (SubDrawCall == 3)
      {
        Submarine_DrawBack_Fake(spriteBatch, editing, e => e is not Structure || (0.6f < e.SpriteDepth && e.SpriteDepth < 0.9f));
        Submarine_DrawBack_Fake(spriteBatch, editing, e => e is not Structure || e.SpriteDepth < 0.6f);
        return false;
      }
      return true;
    }

    public static void Submarine_DrawBack_Fake(SpriteBatch spriteBatch, bool editing = false, Predicate<MapEntity> predicate = null)
    {
      var entitiesToRender = !editing && Submarine.visibleEntities != null ? Submarine.visibleEntities : MapEntity.MapEntityList;

      foreach (MapEntity e in entitiesToRender)
      {
        if (!e.DrawBelowWater) continue;

        if (predicate != null)
        {
          if (!predicate(e)) continue;
        }

        e.Draw(spriteBatch, editing, true);
      }
    }


  }
}