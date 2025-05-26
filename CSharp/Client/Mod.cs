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
    public static string PrefabsPath => Path.Combine(ModDir, AdvancedDecalPrefab.DefaultPrefabsPath);
    public static Dictionary<Character, double> PulseOffsets = new();
    public static Harmony Harmony = new Harmony("more.blood");
    public static Random Random = new Random();
    public static VanillaDecalKiller VanillaDecalKiller = new();

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
      AddCommands();
      VanillaDecalKiller.HideVanillaBlood();

      Config.Load(ConfigPath);
      Config.Save(ConfigPath);
      AdvancedDecalPrefab.LoadPrefabs(PrefabsPath);
      AdvancedDecalPrefab.SavePrefabs(PrefabsPath);
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

      Harmony.Patch(
        original: typeof(LuaGame).GetMethod("IsCustomCommandPermitted"),
        postfix: new HarmonyMethod(typeof(Mod).GetMethod("PermitCommands"))
      );

      HullPatches.PatchAll(Harmony);
      FromBleeding.PatchAll(Harmony);
      FromImpact.PatchAll(Harmony);
      TrackDamageSource.PatchAll(Harmony);
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
      VanillaDecalKiller.RestoreVanillaBlood();
      RemoveCommands();
      Harmony.UnpatchSelf();
      DestroyStaticVars();
      Mixins.Clear();
    }
  }
}