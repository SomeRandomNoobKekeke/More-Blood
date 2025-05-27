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
  public static class MenuIntegration
  {
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
            if (v == 1) return "Normal";
            if (v == 2) return "Bloodbath";
            return Math.Round(v, 2).ToString();
          },
          Mod.Config.GlobalBloodAmount,
          v => { Mod.Config.GlobalBloodAmount = v; ConfigManager.Save(); }
        );
        SettingsMenu.Spacer(right);
      }
    }
  }
}