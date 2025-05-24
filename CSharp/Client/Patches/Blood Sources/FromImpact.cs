using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace MoreBlood
{

  public class FromImpact
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(Limb).GetMethod("AddDamage", AccessTools.all, new Type[]{
          typeof(Vector2),
          typeof(IEnumerable<Affliction>),
          typeof(bool),
          typeof(float),
          typeof(float),
          typeof(Character),
        }),
        postfix: new HarmonyMethod(typeof(FromImpact).GetMethod("Limb_AddDamage_Postfix"))
      );
    }


    public static void Limb_AddDamage_Postfix(Limb __instance, ref AttackResult __result, Vector2 simPosition, IEnumerable<Affliction> afflictions, bool playSound, float damageMultiplier = 1, float penetration = 0f, Character attacker = null)
    {
      if (__result.Afflictions is null) return;
      Limb _ = __instance;



      float bleedingDamage = 0;
      try
      {
        if (_.character.CharacterHealth.DoesBleed)
        {
          foreach (var affliction in __result.Afflictions)
          {
            if (affliction is AfflictionBleeding)
            {
              bleedingDamage += affliction.GetVitalityDecrease(_.character.CharacterHealth);
            }
          }

          if (bleedingDamage > 0)
          {
            float bloodDecalSize = MathHelper.Clamp(bleedingDamage / 5, 0.1f, 1.0f);

            if (_.character.LastDamageSource is Item item)
            {
              Vector2 offset = item.body.LinearVelocity * 200;
              Mod.Log($"{item} {item.body.LinearVelocity}");

              AdvancedDecal decal = _.character.CurrentHull?.AddDecal(
                new AdvancedDecal(AdvancedDecalPrefab.GetPrefab(_.character.BloodDecalName))
                {
                  Scale = bloodDecalSize * 10,
                },
                _.WorldPosition + offset
              );
            }
            else
            {
              AdvancedDecal decal = _.character.CurrentHull?.AddDecal(
                new AdvancedDecal(AdvancedDecalPrefab.GetPrefab(_.character.BloodDecalName))
                {
                  Scale = bloodDecalSize * 10,
                },
                _.WorldPosition
              );
            }
          }
        }
      }
      catch (Exception e)
      {
        Mod.Warning(e.Message);
      }

    }
  }
}