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
    public Harmony Harmony = new Harmony("no.decal.limit");

    public void Initialize()
    {
      PatchAll();
    }

    public void PatchAll()
    {
      Harmony.Patch(
        original: typeof(Hull).GetMethod("AddDecal", AccessTools.all, new Type[]{
          typeof(string),
          typeof(Vector2),
          typeof(float),
          typeof(bool),
          typeof(int),
        }),
        prefix: new HarmonyMethod(typeof(HullPatch).GetMethod("Hull_AddDecal_Replace"))
      );
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