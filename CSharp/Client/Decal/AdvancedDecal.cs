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
    public double MaxLifeTime;
    public double CreationTime;
    public Color CurrentColor;
    public float MaxScale;
    public float MinScale;
    private float scale; public float Scale
    {
      get => scale;
      //set => scale = value;
      set => scale = Math.Clamp(value, MinScale, MaxScale);
    }

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

      //HullPosition -= new Vector2(Sprite.size.X / 2 * Scale, -Sprite.size.Y / 2 * Scale);
    }

    public void Draw(SpriteBatch spriteBatch, float depth)
    {
      try
      {
        //if (Sprite.Texture is null) return;

        Vector2 drawPos = HullPosition + Hull.Rect.Location.ToVector2();
        if (Hull.Submarine != null) { drawPos += Hull.Submarine.DrawPosition; }
        drawPos.Y = -drawPos.Y;

        // GUI.DrawRectangle(spriteBatch, drawPos - new Vector2(64, 64) * Scale, new Vector2(128, 128) * Scale, Color.Yellow * 0.3f);


        spriteBatch.Draw(Sprite.Texture, drawPos, Sprite.SourceRect, CurrentColor, Rotation, HalfSpriteSize, Scale, SpriteEffects.None, depth);

        //GUI.DrawRectangle(spriteBatch, drawPos - new Vector2(2, 2), new Vector2(4, 4), Color.Yellow);
      }
      catch (Exception e) { Mod.Warning($"AdvancedDecal.Draw threw: {e.Message}"); }
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
      MaxLifeTime = prefab.LifeTime;
      CreationTime = Timing.TotalTimeUnpaused;
      Decals.Add(this);
      Rotation = Mod.Random.NextSingle() * Mod.Pi2;
      MinScale = prefab.MinScale;
      MaxScale = prefab.MaxScale;

      HalfSpriteSize = new Vector2(
        Sprite.SourceRect.Width / 2.0f,
        Sprite.SourceRect.Height / 2.0f
      );
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