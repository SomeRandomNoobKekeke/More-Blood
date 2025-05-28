using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MoreBlood
{
  public class AdvancedDecal
  {
    public static bool DecalDebug { get; set; } = false;
    public static HashSet<AdvancedDecal> Decals = new();
    private static double lastNotifyTiming;
    public static void UpdateAll()
    {
      foreach (AdvancedDecal decal in Decals) decal.Update();

      if (DecalDebug && Timing.TotalTimeUnpaused - lastNotifyTiming > 0.1)
      {
        lastNotifyTiming = Timing.TotalTimeUnpaused;
        Mod.Log($"Blood decals count:{Decals.Count}", Color.Pink);
      }
    }
    public AdvancedDecalPrefab Prefab;
    public Sprite Sprite;
    public Hull Hull;
    public double LifeTime;

    public double CreationTime;
    public Color CurrentColor;
    public float MaxSize;
    public float MinSize;
    private float size; public float Size
    {
      get => size;
      set
      {
        size = Math.Clamp(value, MinSize, MaxSize);

        double l = (
          value * SizeToLifetime * (
            1
            + RandomLifetimeIncrement * Mod.Random.NextSingle()
            - RandomLifetimeDecrement * Mod.Random.NextSingle()
          )
        ) / MaxLifeTime;

        l = Math.Pow(Math.Max(0, l), LifetimeExponent);

        LifeTime = (float)Math.Clamp(l * MaxLifeTime, MinLifetime, MaxLifeTime);

        if (DecalDebug)
        {
          Mod.Log($"new decal| input size:{Math.Round(value, 2)} -> SizeToLifetime:{Math.Round(l * MaxLifeTime, 2)} -> lifetime:{Math.Round(LifeTime, 2)}");
        }
      }
    }


    public float MaxLifeTime = 10.0f;
    public float MinLifetime = 0.0f;
    public float SizeToLifetime = 0.1f;
    public float LifetimeExponent = 1.0f;
    public float RandomLifetimeIncrement = 0.0f;
    public float RandomLifetimeDecrement = 0.0f;

    public float Rotation;
    public Vector2 HullPosition;
    public Vector2 WorldPosition
      => Hull?.Submarine is null ?
         HullPosition + Hull.Rect.Location.ToVector2() :
         HullPosition + Hull.Rect.Location.ToVector2() + Hull.Submarine.DrawPosition;
    private Vector2 HalfSpriteSize;

    public void ConnectToHull(Vector2 worldPosition, Hull hull)
    {
      Hull = hull;

      Mixins.GetHullMixin(hull).AdvancedDecals.Add(this);
      HullPosition = worldPosition - hull.WorldRect.Location.ToVector2();
    }

    private Vector2 drawPos;
    public void Draw(SpriteBatch spriteBatch, float depth)
    {
      try
      {
        spriteBatch.Draw(Sprite.Texture, drawPos, Sprite.SourceRect, CurrentColor, Rotation, HalfSpriteSize, Size, SpriteEffects.None, depth);

        if (DecalDebug)
        {
          GUI.DrawRectangle(spriteBatch, drawPos - new Vector2(64, 64) * Size, new Vector2(128, 128) * Size, Color.Yellow * 0.3f);

          //GUI.DrawRectangle(spriteBatch, drawPos - new Vector2(2, 2), new Vector2(4, 4), Color.Yellow);
          GUI.DrawString(spriteBatch, drawPos, $"{Math.Round(Size, 1)}|{Math.Round((Timing.TotalTimeUnpaused - CreationTime))}/{Math.Round(LifeTime).ToString()}", Color.Cyan);
        }
      }
      catch (Exception e) { Mod.Warning($"AdvancedDecal.Draw threw: {e.Message}"); }
    }

    public void Update()
    {
      drawPos = HullPosition + Hull.Rect.Location.ToVector2();
      if (Hull.Submarine != null) { drawPos += Hull.Submarine.DrawPosition; }
      drawPos.Y = -drawPos.Y;

      double lambda = (Timing.TotalTimeUnpaused - CreationTime) / LifeTime;
      if (lambda > 1) Remove();
      CurrentColor = ColorPoint.Lerp(Prefab.Colors, lambda);
    }

    public AdvancedDecal(AdvancedDecalPrefab prefab)
    {
      Decals.Add(this);
      CreationTime = Timing.TotalTimeUnpaused;
      Rotation = Mod.Random.NextSingle() * Mod.Pi2;

      Prefab = prefab;
      Sprite = prefab.Sprites[Rand.Range(0, prefab.Sprites.Count)];
      CurrentColor = prefab.Colors[0].Color;
      LifeTime = prefab.MaxLifeTime;
      MaxLifeTime = prefab.MaxLifeTime;
      MinLifetime = Math.Min(prefab.MinLifetime, prefab.MaxLifeTime);
      MinSize = prefab.MinSize;
      MaxSize = prefab.MaxSize;
      SizeToLifetime = prefab.SizeToLifetime;
      LifetimeExponent = prefab.LifetimeExponent;
      RandomLifetimeIncrement = prefab.RandomLifetimeIncrement;
      RandomLifetimeDecrement = prefab.RandomLifetimeDecrement;


      HalfSpriteSize = new Vector2(
        Sprite.SourceRect.Width / 2.0f,
        Sprite.SourceRect.Height / 2.0f
      );
    }

    public AdvancedDecal(AdvancedDecalPrefab prefab, float size) : this(prefab) => this.Size = size;
    public AdvancedDecal(AdvancedDecalPrefab prefab, Vector2 worldPosition, Hull hull) : this(prefab)
    {
      ConnectToHull(worldPosition, hull);
    }

    public static AdvancedDecal Create(string name, float size)
      => new AdvancedDecal(AdvancedDecalPrefab.GetPrefab(name), size);

    public void Remove()
    {
      Decals.Remove(this);
      if (Hull != null)
      {
        Mixins.GetHullMixin(Hull).AdvancedDecals.Remove(this);
      }
    }

  }
}