using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace NoDecalLimit
{
  public class RemoveDecalLimit
  {
    public static void PatchAll()
    {
      Mod.Harmony.Patch(
        original: typeof(Hull).GetMethod("AddDecal", AccessTools.all, new Type[]{
          typeof(string),
          typeof(Vector2),
          typeof(float),
          typeof(bool),
          typeof(int),
        }),
        prefix: new HarmonyMethod(typeof(RemoveDecalLimit).GetMethod("Hull_AddDecal_Replace"))
      );
    }

    public static bool Hull_AddDecal_Replace(Hull __instance, ref Decal __result, string decalName, Vector2 worldPosition, float scale, bool isNetworkEvent, int? spriteIndex = null)
    {
      Hull _ = __instance;

      //clients are only allowed to create decals when the server says so
      if (!isNetworkEvent && GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
      {
        __result = null; return false;
      }

      //if (_.decals.Count >= Hull.MaxDecalsPerHull) { __result = null; return false; }

      var decal = DecalManager.CreateDecal(decalName, scale, worldPosition, _, spriteIndex);
      if (decal != null)
      {
        if (GameMain.NetworkMember is { IsServer: true })
        {
          GameMain.NetworkMember.CreateEntityEvent(_, new Hull.DecalEventData());
        }
        _.decals.Add(decal);
      }

      __result = decal; return false;
    }
  }
}