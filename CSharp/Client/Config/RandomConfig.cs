using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace MoreBlood
{
  public class RandomConfig : ConfigBase
  {
    public float Min { get; set; } = 1.0f;
    public float Max { get; set; } = 1.0f;
    public float UnconciousPulseSpeed { get; set; } = 0.3f;
  }
}