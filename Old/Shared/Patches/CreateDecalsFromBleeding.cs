using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{
  public class CreateDecalsFromBleeding
  {
    public static void PatchAll()
    {
      Mod.Harmony.Patch(
        original: typeof(CharacterHealth).GetMethod("Update", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CreateDecalsFromBleeding).GetMethod("CharacterHealth_Update_Replace"))
      );
    }

    public static void AddBleedingDecal(CharacterHealth _, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {


      //if (affliction.Strength < 10.0f) return;

      //float bloodDecalSize = (affliction.Strength / affliction.Prefab.MaxStrength) * 1.0f + 0.2f;
      // float bloodDecalSize = targetLimb.LinearVelocity.Length() * 0.1f;

      Vector2 limbSpeed = targetLimb.LinearVelocity - _.Character.AnimController.Collider.LinearVelocity;

      float vitalityFactor = _.Character.Params.Health.Vitality / 100.0f;


      float pulseFactor = (float)Math.Pow(Math.Sin((Timing.TotalTime - Mod.PulseOffsets[_.Character]) * 7), 8);

      float sizeOffset = 0.2f;

      float severityFactor = (affliction.Strength / affliction.Prefab.MaxStrength) * 0.8f;



      float bloodDecalSize = sizeOffset + 0.5f * vitalityFactor * severityFactor * (0.5f + pulseFactor * 3.0f + limbSpeed.Length() * 0.5f);

      if (bloodDecalSize < sizeOffset + 0.1f) return;

      Vector2 decalPos = targetLimb.WorldPosition + limbSpeed * 10.0f;

      //bloodDecalSize = Math.Clamp(bloodDecalSize, 0.2f, 2.0f);

      if (_.Character.CurrentHull is not null)
      {
        if (Mod.Random.Next(2) == 0)
        {
          Decal decal = _.Character.CurrentHull.AddDecal(_.Character.BloodDecalName, decalPos, bloodDecalSize, isNetworkEvent: false);

          if (decal != null)
          {
            decal.FadeTimer = (int)MathHelper.Lerp(0, decal.LifeTime, bloodDecalSize / 4.0f);
            //decal.Color = Color.Lerp(decal.Color, Color.Black, bloodDecalSize / 4.0f);
          }
        }
        else
        {
          Decal decal = _.Character.CurrentHull.AddDecal("oldblood", decalPos, bloodDecalSize, isNetworkEvent: false);

          if (decal != null)
          {
            decal.FadeTimer = (int)MathHelper.Lerp(0, decal.LifeTime, bloodDecalSize / 4.0f);
            //decal.Color = Color.Lerp(decal.Color, Color.Black, bloodDecalSize / 4.0f);
          }
        }

      }
    }

    public static void CharacterHealth_Update_Replace(CharacterHealth __instance, ref bool __runOriginal, float deltaTime)
    {
      CharacterHealth _ = __instance;
      __runOriginal = false;

      //TODO move to character creation
      if (!Mod.PulseOffsets.ContainsKey(_.Character))
      {
        Mod.PulseOffsets[_.Character] = Rand.Range(0.0f, 1.0f);
      }

      _.WasInFullHealth = _.vitality >= _.MaxVitality;

      _.UpdateOxygen(deltaTime);

      _.StunTimer = _.Stun > 0 ? _.StunTimer + deltaTime : 0;

      if (!_.Character.GodMode)
      {
        CharacterHealth.afflictionsToRemove.Clear();
        CharacterHealth.afflictionsToUpdate.Clear();
        foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in _.afflictions)
        {
          var affliction = kvp.Key;
          if (affliction.Strength <= 0.0f)
          {
            AchievementManager.OnAfflictionRemoved(affliction, _.Character);
            if (!_.irremovableAfflictions.Contains(affliction)) { CharacterHealth.afflictionsToRemove.Add(affliction); }
            continue;
          }
          if (affliction.Prefab.Duration > 0.0f)
          {
            affliction.Duration -= deltaTime;
            if (affliction.Duration <= 0.0f)
            {
              //set strength to 0 in case the affliction needs to react to becoming inactive
              affliction.Strength = 0.0f;
              CharacterHealth.afflictionsToRemove.Add(affliction);
              continue;
            }
          }
          CharacterHealth.afflictionsToUpdate.Add(kvp);
        }
        foreach (KeyValuePair<Affliction, CharacterHealth.LimbHealth> kvp in CharacterHealth.afflictionsToUpdate)
        {
          var affliction = kvp.Key;
          Limb targetLimb = null;
          if (kvp.Value != null)
          {
            int healthIndex = _.limbHealths.IndexOf(kvp.Value);
            targetLimb =
                _.Character.AnimController.Limbs.LastOrDefault(l => !l.IsSevered && !l.Hidden && l.HealthIndex == healthIndex) ??
                _.Character.AnimController.MainLimb;
          }
          affliction.Update(_, targetLimb, deltaTime);
          affliction.DamagePerSecondTimer += deltaTime;
          if (affliction is AfflictionBleeding bleeding)
          {
            _.UpdateBleedingProjSpecific(bleeding, targetLimb, deltaTime);
            AddBleedingDecal(_, bleeding, targetLimb, deltaTime);
          }
          _.Character.StackSpeedMultiplier(affliction.GetSpeedMultiplier());
        }

        foreach (var affliction in CharacterHealth.afflictionsToRemove)
        {
          _.afflictions.Remove(affliction);
        }

        if (CharacterHealth.afflictionsToRemove.Count is not 0)
        {
          MedicalClinic.OnAfflictionCountChanged(_.Character);
        }
      }

      _.Character.StackSpeedMultiplier(1f + _.Character.GetStatValue(StatTypes.MovementSpeed));
      if (_.Character.InWater)
      {
        _.Character.StackSpeedMultiplier(1f + _.Character.GetStatValue(StatTypes.SwimmingSpeed));
      }
      else
      {
        _.Character.StackSpeedMultiplier(1f + _.Character.GetStatValue(StatTypes.WalkingSpeed));
      }

      _.UpdateDamageReductions(deltaTime);

      if (!_.Character.GodMode)
      {
#if CLIENT
      _.updateVisualsTimer -= deltaTime;
      if (_.Character.IsVisible && _.updateVisualsTimer <= 0.0f)
      {
          _.UpdateLimbAfflictionOverlays();
          _.UpdateSkinTint();
          _.updateVisualsTimer = CharacterHealth.UpdateVisualsInterval;
      }
#endif
        _.RecalculateVitality();
      }
    }



  }
}