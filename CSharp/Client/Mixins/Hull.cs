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
  public class HullMixin
  {
    public Hull Hull;

    public void DrawAdvancedDecals(SpriteBatch spriteBatch)
    {
      // Rectangle hullDrawRect = rect;
      // if (Submarine != null) hullDrawRect.Location += Submarine.DrawPosition.ToPoint();

      // float depth = 1.0f;
      // foreach (Decal d in decals)
      // {
      //   d.Draw(spriteBatch, this, depth);
      //   depth -= 0.000001f;
      // }
    }

    public HullMixin(Hull hull)
    {
      this.Hull = hull;
    }
  }

}