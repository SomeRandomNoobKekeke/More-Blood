using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{


  public class FromBleeding
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(CharacterHealth).GetMethod("Update", AccessTools.all),
        prefix: new HarmonyMethod(typeof(FromBleeding).GetMethod("CharacterHealth_Update_Replace"))
      );

      harmony.Patch(
        original: typeof(CharacterHealth).GetMethod("UpdateBleedingProjSpecific", AccessTools.all),
        postfix: new HarmonyMethod(typeof(FromBleeding).GetMethod("CharacterHealth_UpdateBleedingProjSpecific_Postfix"))
      );
    }

    public static void CharacterHealth_Update_Replace(CharacterHealth __instance, float deltaTime)
    {
      CharacterHealth _ = __instance;

      //TODO move to character creation
      if (!Mod.PulseOffsets.ContainsKey(_.Character))
      {
        Mod.PulseOffsets[_.Character] = Rand.Range(0.0f, 1.0f);
      }
    }

    public class BleedingConfig : Config
    {

    }

    public static void AddBleedingDecal(CharacterHealth _, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {


      //if (affliction.Strength < 10.0f) return;

      //float bloodDecalSize = (affliction.Strength / affliction.Prefab.MaxStrength) * 1.0f + 0.2f;
      // float bloodDecalSize = targetLimb.LinearVelocity.Length() * 0.1f;

      float conciousnessFactor = _.Character.IsUnconscious ? 0.6f : 1.0f;
      Vector2 limbSpeed = targetLimb.LinearVelocity - _.Character.AnimController.Collider.LinearVelocity;

      float vitalityFactor = _.Character.Params.Health.Vitality / 100.0f;

      float pulseSpeed = 7 * conciousnessFactor;
      float pulseFactor = (float)Math.Pow(Math.Sin((Timing.TotalTime - Mod.PulseOffsets[_.Character]) * pulseSpeed), 8) * conciousnessFactor;

      float sizeOffset = 0.2f;

      float severityFactor = (affliction.Strength / affliction.Prefab.MaxStrength) * 0.8f;

      float bloodDecalSize = sizeOffset + 0.4f * vitalityFactor * severityFactor * (0.3f + 3.0f * pulseFactor + limbSpeed.Length() * 0.5f);

      if (bloodDecalSize < sizeOffset + 0.1f) return;

      Vector2 decalPos = targetLimb.WorldPosition + limbSpeed * 10.0f + new Vector2(
        Mod.Random.NextSingle(), Mod.Random.NextSingle()
      ) * 10.0f;

      //bloodDecalSize = Math.Clamp(bloodDecalSize, 0.2f, 2.0f);

      if (_.Character.CurrentHull is not null)
      {
        AdvancedDecal decal = _.Character.CurrentHull?.AddDecal(
          new AdvancedDecal(AdvancedDecalPrefab.GetPrefab(_.Character.BloodDecalName))
          {
            Scale = bloodDecalSize,
          },
          decalPos
        );

        //TODO why LifeTime is calculated here? it should be the same for all blood decals of the same size
        decal.LifeTime = MathHelper.Lerp((float)decal.MaxLifeTime * 0.1f, (float)decal.MaxLifeTime, bloodDecalSize / 4.0f);
      }
    }

    public static void CharacterHealth_UpdateBleedingProjSpecific_Postfix(CharacterHealth __instance, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {
      try
      {
        AddBleedingDecal(__instance, affliction, targetLimb, deltaTime);
      }
      catch (Exception e) { Mod.Warning($"AddBleedingDecal threw: {e.Message}"); }
    }
  }
}