using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;

namespace MoreBlood
{
  public class BloodDebug
  {
    public bool ConsoleDebug { get; set; } = false;
    public bool VisualDebug { get; set; } = false;
    public bool PluginDebug { get; set; } = false;
  }
}