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
  public class VanillaDecalKiller
  {
    public static string[] BloodDecalNames = new string[] { "blood", "blackblood" };
    public Dictionary<string, Color> OriginalColors = new();

    public void HideVanillaBlood()
    {
      foreach (string name in BloodDecalNames)
      {
        OriginalColors[name] = DecalManager.Prefabs[name].Color;
        typeof(DecalPrefab).GetField("Color", AccessTools.all).SetValue(DecalManager.Prefabs[name], Color.Transparent);
      }
    }

    public void RestoreVanillaBlood()
    {
      foreach (string name in BloodDecalNames)
      {
        typeof(DecalPrefab).GetField("Color", AccessTools.all).SetValue(DecalManager.Prefabs[name], OriginalColors[name]);
      }
    }
  }
}