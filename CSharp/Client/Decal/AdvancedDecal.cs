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
    public static void UpdateAll()
    {
      //Mod.Log($"Decal count:{Decals.Count}");
      foreach (AdvancedDecal decal in Decals) decal.Update();
    }
    public AdvancedDecalPrefab Prefab;
    public Sprite Sprite;
    public Hull Hull;
    public double LifeTime;
    public double CreationTime;
    public Color CurrentColor;
    public float Scale = 1;
    public Vector2 HullPosition;
    public Vector2 WorldPosition
      => Hull?.Submarine is null ?
         HullPosition + Hull.Rect.Location.ToVector2() :
         HullPosition + Hull.Rect.Location.ToVector2() + Hull.Submarine.DrawPosition;


    public void ConnectToHull(Vector2 worldPosition, Hull hull)
    {
      Hull = hull;
      HullPosition = worldPosition - hull.WorldRect.Location.ToVector2();
      Mixins.GetHullMixin(hull).AdvancedDecals.Add(this);
    }

    public void Draw(SpriteBatch spriteBatch, float depth)
    {
      try
      {
        if (Sprite.Texture is null) return;

        Vector2 drawPos = HullPosition + Hull.Rect.Location.ToVector2();
        if (Hull.Submarine != null) { drawPos += Hull.Submarine.DrawPosition; }
        drawPos.Y = -drawPos.Y;

        spriteBatch.Draw(Sprite.Texture, drawPos, Sprite.SourceRect, CurrentColor, 0, new Vector2(-0.5f, -0.5f), Scale, SpriteEffects.None, depth);
      }
      catch (Exception e)
      {
        Mod.Log(e);
      }
    }

    public void Update()
    {
      double lambda = (Timing.TotalTimeUnpaused - CreationTime) / LifeTime;
      if (lambda > 1) Remove();
      CurrentColor = ColorPoint.Lerp(Prefab.Colors, lambda);
    }

    public AdvancedDecal(AdvancedDecalPrefab prefab)
    {
      Prefab = prefab;
      Sprite = prefab.Sprites[Rand.Range(0, prefab.Sprites.Count)];
      CurrentColor = Prefab.Colors[0].Color;
      LifeTime = prefab.LifeTime;
      CreationTime = Timing.TotalTimeUnpaused;
      Decals.Add(this);
    }

    public AdvancedDecal(AdvancedDecalPrefab prefab, Vector2 worldPosition, Hull hull) : this(prefab)
    {
      ConnectToHull(worldPosition, hull);
    }

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