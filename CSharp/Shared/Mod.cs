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

    public void Initialize()
    {
      PatchAll();

      // GameMain.PerformanceCounter.AddElapsedTicks("Draw:HUD", sw.ElapsedTicks);
    }

    public void PatchAll()
    {
      RemoveDecalLimit.PatchAll();
      NetworkingPatch.PatchAll();
      CreateDecalsFromBleeding.PatchAll();
      MeasureDecalDrawTime.PatchAll();

#if CLIENT
      DontCreateDecalsFromBleedingOnClient.PatchAll();
#endif
    }

    public static void Log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

    public void DestroyStaticVars()
    {
      foreach (FieldInfo fi in typeof(Mod).GetFields(AccessTools.all))
      {
        fi.SetValue(null, null);
      }
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {
      Harmony.UnpatchSelf();
      DestroyStaticVars();
    }
  }
}