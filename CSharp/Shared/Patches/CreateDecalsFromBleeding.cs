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

      // Vector2 bloodAccel = new Vector2(
      //   targetLimb.LinearVelocity.X - _.Character.AnimController.Collider.LinearVelocity.X,
      //   targetLimb.LinearVelocity.Y - _.Character.AnimController.Collider.LinearVelocity.Y
      // ) * 20;

      double rotation = -targetLimb.Rotation;

      Vector2 bloodAccel = new Vector2(
        (float)(Math.Cos(rotation)),
        (float)(-Math.Sin(rotation))
      ) * Math.Abs(targetLimb.body.AngularVelocity) * 10;

      Mod.Log($"targetLimb:{targetLimb} rotation:{rotation} targetLimb.body.AngularVelocity:{targetLimb.body.AngularVelocity}");


      // bloodAccel += new Vector2(
      //   (float)(Math.Cos(targetLimb.Rotation) * targetLimb.body.AngularVelocity * mult),
      //   (float)(Math.Sin(targetLimb.Rotation) * targetLimb.body.AngularVelocity * mult)
      // );

      float pulseFactor = (float)Math.Pow(
        Math.Sin((Timing.TotalTime - Mod.PulseOffsets[_.Character]) * 7),
        8
      ) * 0.8f;

      float limbSpeedFactor = targetLimb.LinearVelocity.Length() * 0.0f;
      float severityFactor = (affliction.Strength / affliction.Prefab.MaxStrength) * 0.8f;



      float bloodDecalSize = severityFactor * (0.6f + pulseFactor + limbSpeedFactor);

      if (bloodDecalSize < 0.1f) return;

      Vector2 decalPos = targetLimb.WorldPosition + bloodAccel;

      //bloodDecalSize = Math.Clamp(bloodDecalSize, 0.2f, 2.0f);

      float time = (float)(Timing.TotalTime / 10.0);

      float perlin = (float)PerlinNoise.GetPerlin(time, time);
      //if (perlin < 0.5 || Timing.TotalTime % 2.0 < 0.5) return;

      if (_.Character.CurrentHull is not null)
      {
        var decal = _.Character.CurrentHull.AddDecal(_.Character.BloodDecalName, decalPos, bloodDecalSize, isNetworkEvent: false);

        // if (decal != null)
        // {
        //   decal.FadeTimer = decal.LifeTime - 3;
        // }
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