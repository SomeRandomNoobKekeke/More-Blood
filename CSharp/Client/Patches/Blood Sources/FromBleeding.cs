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

      harmony.Patch(
        original: typeof(Character).GetMethod("UpdateAll", AccessTools.all),
        postfix: new HarmonyMethod(typeof(FromBleeding).GetMethod("Character_UpdateAll_Postfix"))
      );
    }

    public static double LastDecalCreationTime;
    public static bool ShouldCreate;
    public static void Character_UpdateAll_Postfix(float deltaTime, Camera cam)
    {
      if (Timing.TotalTimeUnpaused - LastDecalCreationTime > Mod.Config.DecalCreationInterval)
      {
        LastDecalCreationTime = Timing.TotalTimeUnpaused;
        ShouldCreate = false;
      }
      else
      {
        ShouldCreate = true;
      }
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



    public static void AddBleedingDecal(CharacterHealth _, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {
      if (_.Character.CurrentHull is not null)
      {
        BleedingConfig config = Mod.Config.BleedingConfig;

        Vector2 limbSpeed = targetLimb.LinearVelocity - _.Character.AnimController.Collider.LinearVelocity;

        float vitalityFactor = _.Character.Params.Health.Vitality / 100.0f;

        float pulseSpeed = config.BasicPulseSpeed * (_.Character.IsUnconscious ? config.UnconciousPulseSpeed : 1.0f);

        float pulseFactor = (float)Math.Pow(Math.Sin((Timing.TotalTime - Mod.PulseOffsets[_.Character]) * pulseSpeed), config.PulseSteepness) * config.UnconciousBloodFlow;

        float severityFactor = (affliction.Strength / affliction.Prefab.MaxStrength);

        float bloodDecalSize = config.FlowOffset + Mod.Config.GlobalBloodAmount * vitalityFactor * severityFactor * (config.SeverityFlowFactor + config.PulseFlowFactor * pulseFactor + config.LimbSpeedFlowFactor * limbSpeed.Length());

        if (bloodDecalSize < config.MinFlow) return;

        Vector2 decalPos = targetLimb.WorldPosition + config.LimbSpeedPosFactor * limbSpeed + config.RandomPosFactor * new Vector2(
          Mod.Random.NextSingle(), Mod.Random.NextSingle()
        );


        AdvancedDecal decal = _.Character.CurrentHull?.AddDecal(
          new AdvancedDecal(AdvancedDecalPrefab.GetPrefab(_.Character.BloodDecalName))
          {
            Scale = bloodDecalSize,
          },
          decalPos
        );

        //TODO why LifeTime is calculated here? it should be the same for all blood decals of the same size
        decal.LifeTime = MathHelper.Lerp((float)decal.MaxLifeTime * config.MinDecalLifetime, (float)decal.MaxLifeTime, bloodDecalSize * config.SizeToLifetime);
      }
    }

    public static void CharacterHealth_UpdateBleedingProjSpecific_Postfix(CharacterHealth __instance, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {
      if (ShouldCreate) return;
      try
      {
        AddBleedingDecal(__instance, affliction, targetLimb, deltaTime);
      }
      catch (Exception e) { Mod.Warning($"AddBleedingDecal threw: {e.Message}"); }
    }
  }
}