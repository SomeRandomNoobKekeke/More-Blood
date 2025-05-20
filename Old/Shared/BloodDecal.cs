using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;


namespace MoreBlood
{
  public partial class BloodDecal : Decal
  {
    public readonly DecalPrefab Prefab;
    private Vector2 position;

    private float fadeTimer;

    public readonly Sprite Sprite;

    public float FadeTimer
    {
      get { return fadeTimer; }
      set { fadeTimer = MathHelper.Clamp(value, 0.0f, LifeTime); }
    }

    public float FadeInTime
    {
      get { return Prefab.FadeInTime; }
    }

    public float FadeOutTime
    {
      get { return Prefab.FadeOutTime; }
    }

    public float LifeTime
    {
      get { return Prefab.LifeTime; }
    }

    public float BaseAlpha
    {
      get;
      set;
    } = 1.0f;

    public Color Color
    {
      get;
      set;
    }

    public Vector2 WorldPosition
    {
      get
      {
        Vector2 worldPos = position
            + clippedSourceRect.Size.ToVector2() / 2 * Scale
            + hull.Rect.Location.ToVector2();
        if (hull.Submarine != null) { worldPos += hull.Submarine.DrawPosition; }
        return worldPos;
      }
    }

    public Vector2 CenterPosition
    {
      get;
      private set;
    }

    public Vector2 NonClampedPosition
    {
      get;
      private set;
    }

    public int SpriteIndex
    {
      get;
      private set;
    }

    private readonly HashSet<BackgroundSection> affectedSections;

    private readonly Hull hull;

    public readonly float Scale;

    private Rectangle clippedSourceRect;

    private bool cleaned = false;



    public void Update(float deltaTime)
    {
      fadeTimer += deltaTime;
    }

    public void ForceRefreshFadeTimer(float val)
    {
      cleaned = false;
      fadeTimer = val;
    }

    public void StopFadeIn()
    {
      Color *= GetAlpha();
      fadeTimer = Prefab.FadeInTime;
    }

    public bool AffectsSection(BackgroundSection section)
    {
      return affectedSections != null && affectedSections.Contains(section);
    }

    public void Clean(float val)
    {
      cleaned = true;
      float sizeModifier = MathHelper.Clamp(Sprite.size.X * Sprite.size.Y * Scale / 10000, 1.0f, 25.0f);
      BaseAlpha -= val * -1 / sizeModifier;
    }

    private float GetAlpha()
    {
      if (fadeTimer < Prefab.FadeInTime && !cleaned)
      {
        return BaseAlpha * fadeTimer / Prefab.FadeInTime;
      }
      else if (cleaned || fadeTimer > Prefab.LifeTime - Prefab.FadeOutTime)
      {
        return BaseAlpha * Math.Min((Prefab.LifeTime - fadeTimer) / Prefab.FadeOutTime, 1.0f);
      }
      return BaseAlpha;
    }

    public static BloodDecal Create(string decalName, float scale, Vector2 worldPosition, Hull hull, int? spriteIndex = null)
    {
      string lowerCaseDecalName = decalName.ToLowerInvariant();
      if (!DecalManager.Prefabs.ContainsKey(lowerCaseDecalName))
      {
        DebugConsole.ThrowError("Decal prefab " + decalName + " not found!");
        return null;
      }

      DecalPrefab prefab = DecalManager.Prefabs[lowerCaseDecalName];

      return new BloodDecal(prefab, scale, worldPosition, hull, spriteIndex);
    }

    public BloodDecal(DecalPrefab prefab, float scale, Vector2 worldPosition, Hull hull, int? spriteIndex = null) : base(prefab, scale, worldPosition, hull, spriteIndex)
    {
      Color = Color.Yellow;
      //Mod.Log(Color);
    }
  }
}


