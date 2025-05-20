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
  public partial class Mod : IAssemblyPlugin
  {
    public static Dictionary<Character, double> PulseOffsets = new();
    public static Harmony Harmony = new Harmony("more.blood");
    public static Random Random = new Random();

    public static bool Debug { get; set; } = true;

    public void Initialize()
    {
      PatchAll();

      // GameMain.PerformanceCounter.AddElapsedTicks("Draw:HUD", sw.ElapsedTicks);
      Info("MoreBlood Compiled!");
    }

    public void PatchAll()
    {
      HullPatches.PatchAll(Harmony);
      //       RemoveDecalLimit.PatchAll();
      //       NetworkingPatch.PatchAll();
      //       CreateDecalsFromBleeding.PatchAll();
      //       MeasureDecalDrawTime.PatchAll();

      // #if CLIENT
      //       DontCreateDecalsFromBleedingOnClient.PatchAll();
      // #endif
    }



    public void DestroyStaticVars()
    {
      foreach (FieldInfo fi in typeof(Mod).GetFields(AccessTools.all))
      {
        if (!fi.FieldType.IsValueType) fi.SetValue(null, null);
      }
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {
      Harmony.UnpatchSelf();
      DestroyStaticVars();
      Mixins.Clear();
    }
  }
}