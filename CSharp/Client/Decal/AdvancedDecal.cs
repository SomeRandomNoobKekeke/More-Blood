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
    public static HashSet<AdvancedDecal> Decals = new();
    private static double lastNotifyTiming;
    public static int cachedCount;
    public static void UpdateAll()
    {
      foreach (AdvancedDecal decal in Decals) decal.Update();

      if (Mod.Debug.ConsoleDebug && Timing.TotalTimeUnpaused - lastNotifyTiming > 0.1)
      {
        lastNotifyTiming = Timing.TotalTimeUnpaused;
        cachedCount = Decals.Count;
        Mod.Log($"Blood decals count:{Decals.Count}", Color.Pink);
      }
    }


    private float size; public float Size
    {
      get => size;
      set
      {
        float s = value * Prefab.SizeFluctuation.Next();

        size = Math.Clamp(s, Prefab.MinSpriteSize, Prefab.MaxSpriteSize);

        LifeTime =
          Math.Clamp(
            Utils.ExpSegment(Prefab.SLStart, Prefab.SLEnd, Prefab.LifetimeExponent, s) *
            Prefab.LifetimeFluctuation.Next() * Mod.Config.GlobalDecalLifetime,
            Prefab.MinLifetime, Prefab.MaxLifetime
          );

        InitialLifeTime = LifeTime;

        if (Mod.Debug.ConsoleDebug)
        {
          Mod.Log($"new decal| input:{Math.Round(value, 2)} -> size:{Math.Round(size, 2)} -> lambda:{Math.Round(Utils.Lambda(Prefab.SLStart.X, Prefab.SLEnd.X, s), 2)} -> lifetime:{Math.Round(LifeTime, 2)}");
        }
      }
    }
    public AdvancedDecalPrefab Prefab;
    public Sprite Sprite;
    public Hull Hull;
    private double lifeTime; public double LifeTime
    {
      get => lifeTime;
      set => lifeTime = Math.Max(0, value);
    }
    public double TimeLeft => Math.Max(0, LifeTime - (Timing.TotalTimeUnpaused - CreationTime));
    public double InitialLifeTime;
    public double LifeTimeLambda;
    public double CreationTime;
    public Color CurrentColor;
    public float Rotation;
    public Vector2 HullPosition;
    public Rectangle HullRectangle;
    public Rectangle WorldRectangle;
    public Vector2 WorldPosition
      => Hull?.Submarine is null ?
         HullPosition + Hull.Rect.Location.ToVector2() :
         HullPosition + Hull.Rect.Location.ToVector2() + Hull.Submarine.DrawPosition;
    public Vector2 SpriteSize;
    public Vector2 HalfSpriteSize;
    public void ConnectToHull(Vector2 worldPosition, Hull hull)
    {
      Hull = hull;

      Mixins.GetHullMixin(hull).AdvancedDecals.Add(this);
      HullPosition = worldPosition - hull.WorldRect.Location.ToVector2();
      HullRectangle = new Rectangle(
        (int)(HullPosition.X - HalfSpriteSize.X * Size),
        (int)(HullPosition.Y - HalfSpriteSize.Y * Size),
        (int)(SpriteSize.X * Size),
        (int)(SpriteSize.Y * Size)
      );
    }

    private Vector2 drawPos;
    public void Draw(SpriteBatch spriteBatch, float depth)
    {
      try
      {
        spriteBatch.Draw(Sprite.Texture, drawPos, Sprite.SourceRect, CurrentColor, Rotation, HalfSpriteSize, Size, SpriteEffects.None, depth);

        if (Mod.Debug.VisualDebug)
        {
          GUI.DrawRectangle(spriteBatch, drawPos - HalfSpriteSize * Size, SpriteSize * Size, Color.Yellow * 0.3f);

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

      LifeTimeLambda = (Timing.TotalTimeUnpaused - CreationTime) / LifeTime;
      if (LifeTimeLambda > 1) Remove();
      CurrentColor = ColorPoint.Lerp(Prefab.Colors, LifeTimeLambda);
    }

    public AdvancedDecal(AdvancedDecalPrefab prefab)
    {
      Decals.Add(this);
      CreationTime = Timing.TotalTimeUnpaused;
      Rotation = Utils.Random.NextSingle() * Utils.Pi2;

      Prefab = prefab;
      Sprite = prefab.Sprites[Rand.Range(0, prefab.Sprites.Count)];
      CurrentColor = prefab.Colors[0].Color;
      LifeTime = 1.0f;

      SpriteSize = new Vector2(
        Sprite.SourceRect.Width,
        Sprite.SourceRect.Height
      );

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