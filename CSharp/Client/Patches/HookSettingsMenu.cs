using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Barotrauma.Items.Components;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;

namespace MoreBlood
{
  public class HookSettingsMenu
  {
    public static void PatchAll(Harmony harmony)
    {
      harmony.Patch(
        original: typeof(SettingsMenu).GetConstructors(AccessTools.all)[0],
        postfix: new HarmonyMethod(typeof(MenuIntegration).GetMethod("AfterMenuCreation"))
      );
    }
  }
}