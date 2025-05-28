using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.IO;

using Barotrauma;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;

namespace MoreBlood
{
  public partial class Mod
  {
    public static List<DebugConsole.Command> AddedCommands = new List<DebugConsole.Command>();
    public static void AddCommands()
    {
      AddedCommands.Add(new DebugConsole.Command("reloadblood", "", ReloadBlood_Command));
      AddedCommands.Add(new DebugConsole.Command("reloadbloodconfig", "", ReloadBloodConfig_Command));
      AddedCommands.Add(new DebugConsole.Command("resetbloodconfig", "", ResetBloodConfig_Command));
      AddedCommands.Add(new DebugConsole.Command("reloadbloodprefabs", "", ReloadBloodPrefabs_Command));
      AddedCommands.Add(new DebugConsole.Command("printbloodprefabs", "", PrintBloodPrefabs_Command));
      AddedCommands.Add(new DebugConsole.Command("printbloodconfig", "", PrintBloodConfig_Command));
      AddedCommands.Add(new DebugConsole.Command("bleed", "bleed [amount] [limbcount]", Bleed_Command));
      AddedCommands.Add(new DebugConsole.Command("clearblood", "", ClearBlood_Command));
      AddedCommands.Add(new DebugConsole.Command("spawnblood", "spawnblood [size]", SpawnBlood_Command));
      AddedCommands.Add(new DebugConsole.Command("spawnbloodspectrum", "spawnbloodspectrum [offset]", SpawnBloodSpectrum_Command));
      AddedCommands.Add(new DebugConsole.Command("spawnbloodpuddle", "spawnbloodpuddle [size]", SpawnBloodPuddle_Command));


      AddedCommands.Add(new DebugConsole.Command("blooddebug", "", BloodDecalDebug_Command, () => new string[][] { typeof(BloodDebug).GetProperties().Select(pi => pi.Name).ToArray() }));

      DebugConsole.Commands.InsertRange(0, AddedCommands);
    }
    public static void BloodDecalDebug_Command(string[] args)
    {
      if (args.Length == 0)
      {
        Debug.ConsoleDebug = !Debug.ConsoleDebug;
        Debug.VisualDebug = !Debug.VisualDebug;
        return;
      }

      if (string.Equals(args[0], "ConsoleDebug", StringComparison.OrdinalIgnoreCase))
      {
        Debug.ConsoleDebug = !Debug.ConsoleDebug;
      }

      if (string.Equals(args[0], "VisualDebug", StringComparison.OrdinalIgnoreCase))
      {
        Debug.VisualDebug = !Debug.VisualDebug;
      }

      if (string.Equals(args[0], "PluginDebug", StringComparison.OrdinalIgnoreCase))
      {
        Debug.PluginDebug = !Debug.PluginDebug;
      }
    }
    public static void SpawnBlood_Command(string[] args)
    {
      Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);

      Hull hull = Hull.GetCleanTarget(mousePos);

      if (hull is null)
      {
        Mod.Log($"You need a hull");
        return;
      }

      float size = 1.0f;
      if (args.Length > 0) float.TryParse(args[0], out size);

      hull.AddDecal(
        AdvancedDecal.Create(AdvancedDecalPrefab.DefaultBasePrefab, size),
        mousePos
      );
    }

    public static void SpawnBloodPuddle_Command(string[] args)
    {
      ClearBlood_Command(null);

      Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);

      Hull hull = Hull.GetCleanTarget(mousePos);

      if (hull is null)
      {
        Mod.Log($"You need a hull");
        return;
      }

      int size = 2000;
      if (args.Length > 0) int.TryParse(args[0], out size);

      for (int x = 0; x < size; x++)
      {
        hull.AddDecal(
          AdvancedDecal.Create(AdvancedDecalPrefab.DefaultBasePrefab, 1f),
          mousePos + new Vector2(Mod.Random.NextSingle(), Mod.Random.NextSingle()) * 10.0f
        );
      }
    }

    public static void SpawnBloodSpectrum_Command(string[] args)
    {
      ClearBlood_Command(null);

      Vector2 mousePos = Screen.Selected.Cam.ScreenToWorld(PlayerInput.MousePosition);

      Hull hull = Hull.GetCleanTarget(mousePos);

      if (hull is null)
      {
        Mod.Log($"You need a hull");
        return;
      }

      int offset = 0;
      if (args.Length > 0) int.TryParse(args[0], out offset);

      float dx = 0;
      for (int x = 0; x <= 20; x++)
      {
        float size = (offset + x) / 10.0f;

        hull.AddDecal(
          AdvancedDecal.Create(AdvancedDecalPrefab.DefaultBasePrefab, size),
          mousePos + new Vector2(dx, 0)
        );

        dx += size * 100;
      }
    }

    public static void ReloadBlood_Command(string[] args)
    {
      ClearBlood_Command(null);
      ReloadBloodConfig_Command(null);
      ReloadBloodPrefabs_Command(null);
    }
    public static void ReloadBloodConfig_Command(string[] args)
    {
      ConfigManager.Load();
    }

    public static void ResetBloodConfig_Command(string[] args)
    {
      Mod.Config = new Config();
      Mod.Config.Version = Mod.Package.ModVersion;
      ConfigManager.Save();
    }

    public static void ReloadBloodPrefabs_Command(string[] args)
    {
      AdvancedDecalPrefab.LoadPrefabs(PrefabsPath);
      AdvancedDecalPrefab.SavePrefabs(PrefabsPath);
    }
    public static void PrintBloodPrefabs_Command(string[] args)
    {
      AdvancedDecalPrefab.PrintPrefabs();
    }
    public static void PrintBloodConfig_Command(string[] args)
    {
      Mod.Config.Print();
    }

    public static void ClearBlood_Command(string[] args)
    {
      Mixins.Clear();
      AdvancedDecal.Decals.Clear();
    }

    public static float? MemorizedBleedingAmount;
    public static int? MemorizedLimbCount;
    public static void Bleed_Command(string[] args)
    {
      if (Character.Controlled is null) return;

      DebugConsole.ExecuteCommand("revive");

      float bleedingAmount = MemorizedBleedingAmount ?? 100.0f;
      if (args.Length > 0)
      {
        float.TryParse(args[0], out bleedingAmount);
        MemorizedBleedingAmount = bleedingAmount;
      }

      Dictionary<int, string> limbNames = new()
      {
        [0] = "righthand",
        [1] = "lefthand",
        [2] = "rightleg",
        [3] = "leftleg",
        [4] = "head",
        [5] = "torso",
      };

      int limbCount = MemorizedLimbCount ?? 1;
      if (args.Length > 1)
      {
        int.TryParse(args[1], out limbCount);
        MemorizedLimbCount = limbCount;
      }
      limbCount = Math.Clamp(limbCount, 0, 5);

      List<Limb> targetLimbs = new List<Limb>();

      for (int i = 0; i < limbCount; i++)
      {
        targetLimbs.Add(Character.Controlled.AnimController.Limbs.FirstOrDefault(l => l.type.ToString().Equals(limbNames[i], StringComparison.OrdinalIgnoreCase)));
      }

      AfflictionPrefab afflictionPrefab = AfflictionPrefab.Bleeding;

      foreach (Limb limb in targetLimbs)
      {
        Character.Controlled.CharacterHealth.ApplyAffliction(limb ?? Character.Controlled.AnimController.MainLimb, afflictionPrefab.Instantiate(bleedingAmount));
      }
    }

    public static void RemoveCommands()
    {
      AddedCommands.ForEach(c => DebugConsole.Commands.Remove(c));
      AddedCommands.Clear();
    }

    public static void PermitCommands(Identifier command, ref bool __result)
    {
      if (AddedCommands.Any(c => c.Names.Contains(command.Value))) __result = true;
    }
  }
}