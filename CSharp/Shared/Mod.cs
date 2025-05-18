using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace NoDecalLimit
{
  public partial class Mod : IAssemblyPlugin
  {
    public static Harmony Harmony = new Harmony("no.decal.limit");

    public void Initialize()
    {
      PatchAll();
    }

    public void PatchAll()
    {
      RemoveDecalLimit.PatchAll();
      NetworkingPatch.PatchAll();

#if CLIENT
      CreateDecalsFromBleeding.PatchAll();
#endif
    }

    public static void Log(object msg, Color? cl = null)
    {
      cl ??= Color.Cyan;
      LuaCsLogger.LogMessage($"{msg ?? "null"}", cl * 0.8f, cl);
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {
      Harmony.UnpatchSelf();
    }
  }
}