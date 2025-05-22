using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{

  public class BleedingConfig : ConfigBase
  {
    public float UnconciousPulseSpeed { get; set; } = 0.6f;
    public float UnconciousBloodFlow { get; set; } = 0.6f;
    public float BasicPulseSpeed { get; set; } = 7.0f;
    public float PulseSteepness { get; set; } = 8.0f;
    public float FlowOffset { get; set; } = 0.2f;
    public float MinFlow { get; set; } = 0.3f;
    public float GlobalFlowFactor { get; set; } = 0.4f;
    public float SeverityFlowFactor { get; set; } = 0.8f;
    public float PulseFlowFactor { get; set; } = 0.8f;
    public float LimbSpeedFlowFactor { get; set; } = 0.8f;
    public float LimbSpeedPosFactor { get; set; } = 10.0f;
    public float RandomPosFactor { get; set; } = 10.0f;
    public float MinDecalLifetime { get; set; } = 0.1f;
    public float SizeToLifetime { get; set; } = 0.25f;
  }


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

        float bloodDecalSize = config.FlowOffset + config.GlobalFlowFactor * vitalityFactor * severityFactor * (config.SeverityFlowFactor + config.PulseFlowFactor * pulseFactor + config.LimbSpeedFlowFactor * limbSpeed.Length());

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
      try
      {
        AddBleedingDecal(__instance, affliction, targetLimb, deltaTime);
      }
      catch (Exception e) { Mod.Warning($"AddBleedingDecal threw: {e.Message}"); }
    }
  }
}