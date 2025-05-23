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
  public class BleedingConfig : ConfigBase
  {
    public float UnconciousPulseSpeed { get; set; } = 0.6f;
    public float UnconciousBloodFlow { get; set; } = 0.6f;
    public float BasicPulseSpeed { get; set; } = 7.0f;
    public float PulseSteepness { get; set; } = 8.0f;
    public float FlowOffset { get; set; } = 0.3f;
    public float MinFlow { get; set; } = 0.4f;
    public float SeverityFlowFactor { get; set; } = 0.2f;
    public float PulseFlowFactor { get; set; } = 1.4f;
    public float LimbSpeedFlowFactor { get; set; } = 0.1f;
    public float LimbSpeedPosFactor { get; set; } = 10.0f;
    public float RandomPosFactor { get; set; } = 10.0f;
    public float MinDecalLifetime { get; set; } = 0.1f;
    public float SizeToLifetime { get; set; } = 0.1f;
  }
}