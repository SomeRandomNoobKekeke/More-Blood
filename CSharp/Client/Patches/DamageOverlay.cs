using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{
  public class DamageOverlay
  {
    public static void PatchAll(Harmony harmony)
    {
      Mod.Log(typeof(Limb).GetMethod("set_DamageOverlayStrength", AccessTools.all));

      harmony.Patch(
        original: typeof(Limb).GetMethod("set_DamageOverlayStrength", AccessTools.all),
        prefix: new HarmonyMethod(typeof(DamageOverlay).GetMethod("Limb_set_DamageOverlayStrength_Replace"))
      );
    }

    public static bool Limb_set_DamageOverlayStrength_Replace(Limb __instance, float value)
    {
      __instance.damageOverlayStrength = MathHelper.Clamp(value, 0.0f, Mod.Config.DamageOverlayThreshold) / Mod.Config.DamageOverlayThreshold;
      return false;
    }
  }
}