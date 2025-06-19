using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;

namespace MoreBlood
{
  public static class MenuIntegration
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(SettingsMenu).GetConstructors(AccessTools.all)[0],
        postfix: new HarmonyMethod(typeof(MenuIntegration).GetMethod("AfterMenuCreation"))
      );
    }

    public static void PrintComponentsReq(GUIComponent component, string offset = "")
    {
      Mod.Log($"{offset}{component} {component.Rect}");
      foreach (RectTransform rt in component.RectTransform.Children)
      {
        PrintComponentsReq(rt.GUIComponent, offset + "|   ");
      }
    }

    public static void AfterMenuCreation(SettingsMenu __instance)
    {
      if (__instance.tabContents.TryGetValue(SettingsMenu.Tab.Graphics, out (GUIButton Button, GUIFrame Content) tabContent))
      {
        GUILayoutGroup right = (GUILayoutGroup)tabContent.Content.GetChild(0).GetChild(2);

        SettingsMenu.Label(right, "Amount of blood", GUIStyle.SubHeadingFont);
        SettingsMenu.Slider(
          right, (0, 2), 41,
          v =>
          {
            if (v == 0) return "No Blood";
            if (v == 1) return "Balance?";
            if (v == 2) return "5 FPS";
            return Math.Round(v, 2).ToString();
          },
          Mod.Config.GlobalBloodAmount,
          v => { Mod.Config.GlobalBloodAmount = (float)Math.Round(v, 2); ConfigManager.Save(); }
        );
        SettingsMenu.Label(right, "Blood decals lifetime", GUIStyle.SubHeadingFont);
        SettingsMenu.Slider(
          right, (0, 10), 101,
          v => Math.Round(v, 2).ToString(),
          Mod.Config.GlobalDecalLifetime,
          v => { Mod.Config.GlobalDecalLifetime = (float)Math.Round(v, 2); ConfigManager.Save(); }
        );
        //SettingsMenu.Spacer(right);
      }
    }
  }
}