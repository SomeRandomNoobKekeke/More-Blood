using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

namespace NoDecalLimit
{
  public class CharacterHealthPatch
  {
    public static void PatchAll()
    {
      Mod.Harmony.Patch(
        original: typeof(CharacterHealth).GetMethod("UpdateBleedingProjSpecific", AccessTools.all),
        prefix: new HarmonyMethod(typeof(CharacterHealthPatch).GetMethod("CharacterHealth_UpdateBleedingProjSpecific_Replace"))
      );
    }

    public static bool CharacterHealth_UpdateBleedingProjSpecific_Replace(CharacterHealth __instance, AfflictionBleeding affliction, Limb targetLimb, float deltaTime)
    {
      CharacterHealth _ = __instance;

      if (_.Character.InvisibleTimer > 0.0f) { return false; }

      _.bloodParticleTimer -= deltaTime * (affliction.Strength / 10.0f);
      if (_.bloodParticleTimer <= 0.0f)
      {
        Limb limb = targetLimb ?? _.Character.AnimController.MainLimb;

        bool inWater = _.Character.AnimController.InWater;
        var drawTarget = inWater ? Barotrauma.Particles.ParticlePrefab.DrawTargetType.Water : Barotrauma.Particles.ParticlePrefab.DrawTargetType.Air;
        var emitter = _.Character.BloodEmitters.FirstOrDefault(e => e.Prefab.ParticlePrefab?.DrawTarget == drawTarget || e.Prefab.ParticlePrefab?.DrawTarget == Barotrauma.Particles.ParticlePrefab.DrawTargetType.Both);
        float particleMinScale = emitter?.Prefab.Properties.ScaleMin ?? 0.5f;
        float particleMaxScale = emitter?.Prefab.Properties.ScaleMax ?? 1;
        float severity = Math.Min(affliction.Strength / affliction.Prefab.MaxStrength * _.Character.Params.BleedParticleMultiplier, 1);
        float bloodParticleSize = MathHelper.Lerp(particleMinScale, particleMaxScale, severity);

        Vector2 velocity = Rand.Vector(affliction.Strength * 0.1f);
        if (!inWater)
        {
          bloodParticleSize *= 2.0f;
          velocity = limb.LinearVelocity * 100.0f;
        }

        // TODO: use the blood emitter?
        var blood = GameMain.ParticleManager.CreateParticle(
            inWater ? _.Character.Params.BleedParticleWater : _.Character.Params.BleedParticleAir,
            limb.WorldPosition, velocity, 0.0f, _.Character.AnimController.CurrentHull);

        if (blood != null)
        {
          blood.Size *= bloodParticleSize;
          if (!inWater && !string.IsNullOrEmpty(_.Character.BloodDecalName) && Rand.Range(0.0f, 1.0f) < 0.05f)
          {
            blood.OnCollision += (Vector2 pos, Hull hull) =>
            {
              var decal = hull?.AddDecal(_.Character.BloodDecalName, pos, Rand.Range(1.0f, 2.0f), isNetworkEvent: true);
              if (decal != null)
              {
                decal.FadeTimer = decal.LifeTime - decal.FadeOutTime * 2;
              }
            };
          }
        }
        _.bloodParticleTimer = MathHelper.Lerp(2, 0.5f, severity);
      }

      return false;
    }
  }
}