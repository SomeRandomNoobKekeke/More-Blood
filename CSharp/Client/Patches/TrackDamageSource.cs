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

  /// <summary>
  /// Character.LastDamageSource isn't enough and not reliable
  /// It doesn't get cleared automatically and idk if it's safe to clear
  /// </summary>
  public class TrackDamageSource
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(Projectile).GetMethod("HandleProjectileCollision", AccessTools.all),
        prefix: new HarmonyMethod(typeof(TrackDamageSource).GetMethod("Projectile_HandleProjectileCollision_Prefix"))
      );

      harmony.Patch(
        original: typeof(Projectile).GetMethod("HandleProjectileCollision", AccessTools.all),
        postfix: new HarmonyMethod(typeof(TrackDamageSource).GetMethod("ClearContext"))
      );


      harmony.Patch(
        original: typeof(MeleeWeapon).GetMethod("HandleImpact", AccessTools.all),
        prefix: new HarmonyMethod(typeof(TrackDamageSource).GetMethod("MeleeWeapon_HandleImpact_Prefix"))
      );

      harmony.Patch(
        original: typeof(MeleeWeapon).GetMethod("HandleImpact", AccessTools.all),
        postfix: new HarmonyMethod(typeof(TrackDamageSource).GetMethod("ClearContext"))
      );
    }

    public static void ClearContext() => DamageContext.Current = DamageContext.Unknown;

    public static void Projectile_HandleProjectileCollision_Prefix(Projectile __instance, Fixture target, Vector2 collisionNormal, Vector2 velocity)
    {
      DamageContext.Current = new ProjectileDamageContext(__instance.Item);
    }

    public static void MeleeWeapon_HandleImpact_Prefix(MeleeWeapon __instance, Fixture targetFixture)
    {
      DamageContext.Current = new MeleeDamageContext(__instance.Item);
    }


  }
}