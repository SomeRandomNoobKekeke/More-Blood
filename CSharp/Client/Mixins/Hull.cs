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

    public List<AdvancedDecal> AdvancedDecals = new();

    public void DrawAdvancedDecals(SpriteBatch spriteBatch)
    {
      float depth = 1.0f;
      foreach (AdvancedDecal decal in AdvancedDecals)
      {
        decal.Draw(spriteBatch, depth);
        depth -= 0.000001f;
      }
    }

    public HullMixin(Hull hull)
    {
      this.Hull = hull;
    }
  }

}