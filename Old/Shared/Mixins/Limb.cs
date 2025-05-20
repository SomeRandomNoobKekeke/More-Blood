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

  /// <summary>
  /// Mixin for tracking limb acceleration.  
  /// Unused for now
  /// </summary>
  public class LimbMixin
  {
    public Vector2 PrevLinearVelocity;
    public Vector2 GetAcceleration(Vector2 LinearVelocity)
      => LinearVelocity - PrevLinearVelocity;
  }
}