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
  public partial class Mod : IAssemblyPlugin
  {
    public static float Pi2 = (float)(Math.PI * 2.0);
    public static string ModDir;
    public static string ConfigPath => Path.Combine(ModDir, Config.DefaultConfigPath);
    public static Dictionary<Character, double> PulseOffsets = new();
    public static Harmony Harmony = new Harmony("more.blood");
    public static Random Random = new Random();

    public static Config Config = new Config();

    public static bool Debug;

    public void Initialize()
    {
      if (!GameMain.LuaCs.PluginPackageManager.TryGetPackageForPlugin<Mod>(out ContentPackage package))
      {
        Log($"PluginPackageManager couldn't find ContentPackage for {this} (LoL)");
        return;
      }

      ModDir = Path.GetFullPath(package.Dir);
      if (ModDir.Contains("LocalMods"))
      {
        Debug = true;
        Log($"Found [{package.Name}] in LocalMods, debug: {Debug}\n");
      }

      PatchAll();

      Config.Load(ConfigPath);
      Config.Save(ConfigPath);
    }

    public void PatchAll()
    {
      GameMain.LuaCs.Hook.Add("think", "MoreBlood.UpdateDecals", (object[] args) =>
      {
        AdvancedDecal.UpdateAll();
        return null;
      });

      GameMain.LuaCs.Hook.Add("roundStart", "MoreBlood.clearmixins", (object[] args) =>
      {
        Mixins.Clear();
        return null;
      });

      HullPatches.PatchAll(Harmony);
      FromBleeding.PatchAll(Harmony);
      //       RemoveDecalLimit.PatchAll();
      //       NetworkingPatch.PatchAll();
      //       CreateDecalsFromBleeding.PatchAll();
      //       MeasureDecalDrawTime.PatchAll();

      // #if CLIENT
      //       DontCreateDecalsFromBleedingOnClient.PatchAll();
      // #endif
    }



    public void DestroyStaticVars()
    {
      foreach (FieldInfo fi in typeof(Mod).GetFields(AccessTools.all))
      {
        if (!fi.FieldType.IsValueType) fi.SetValue(null, null);
      }
    }

    public void OnLoadCompleted() { }
    public void PreInitPatching() { }
    public void Dispose()
    {
      Harmony.UnpatchSelf();
      DestroyStaticVars();
      Mixins.Clear();
    }
  }
}