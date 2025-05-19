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

  public class LimbMixin
  {
    public Vector2 PrevLinearVelocity;
    public Vector2 GetAcceleration(Vector2 LinearVelocity)
      => LinearVelocity - PrevLinearVelocity;
  }
}