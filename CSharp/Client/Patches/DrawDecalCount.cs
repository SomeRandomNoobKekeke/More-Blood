using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework.Graphics;

namespace MoreBlood
{
  public class DrawDecalCount
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(GUI).GetMethod("Draw", AccessTools.all),
        postfix: new HarmonyMethod(typeof(DrawDecalCount).GetMethod("GUI_Draw_Postfix"))
      );
    }

    public static float TooMany = 1000.0f;


    public static void GUI_Draw_Postfix(Camera cam, SpriteBatch spriteBatch)
    {
      if (Mod.Debug.ConsoleDebug || Mod.Debug.VisualDebug)
      {
        Color cl = Mod.Config.GlobalBloodAmount == 1 && Mod.Config.GlobalDecalLifetime == 1 ?
          ToolBox.GradientLerp(AdvancedDecal.cachedCount / TooMany, Color.Lime, Color.Yellow, Color.Orange, Color.Red) :
          Color.LightSlateGray;
        GUI.DrawString(spriteBatch, new Vector2(GameMain.GraphicsWidth / 2.0f - 70.0f, 0), $"Blood decals count:{AdvancedDecal.cachedCount}", cl);
      }
    }
  }
}