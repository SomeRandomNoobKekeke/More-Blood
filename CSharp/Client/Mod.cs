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
    //TODO idk where to put these paths
    public static ContentPackage Package;
    public static string ConfigPath
      => Path.Combine(Package.Dir, Config.DefaultConfigPath);

    public static string OldConfigPath => Path.Combine(Package.Dir, Config.DefaultOldConfigPath);
    public static string PrefabsPath => Path.Combine(Package.Dir, AdvancedDecalPrefab.DefaultPrefabsPath);




    public static Dictionary<Character, double> PulseOffsets = new();
    public static Harmony Harmony = new Harmony("more.blood");

    public static VanillaDecalKiller VanillaDecalKiller = new();

    public static Config Config = new Config();
    public static BloodDebug Debug = new BloodDebug();

    public void Initialize()
    {
      if (!GameMain.LuaCs.PluginPackageManager.TryGetPackageForPlugin<Mod>(out ContentPackage package))
      {
        Log($"PluginPackageManager couldn't find ContentPackage for {this} (LoL)");
        return;
      }
      Package = package;

      if (Package.Dir.Contains("LocalMods"))
      {
        Debug.PluginDebug = true;
        Log($"Found [{Package.Name}] in LocalMods, debug: {Debug.PluginDebug}\n");
      }

      PatchAll();
      AddCommands();


      ConfigManager.Load();

      VanillaDecalKiller.HideVanillaBlood();
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
      HookSettingsMenu.PatchAll(Harmony);
      DrawDecalCount.PatchAll(Harmony);
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