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
      AddedCommands.Add(new DebugConsole.Command("bleed", "", Bleed_Command));
      AddedCommands.Add(new DebugConsole.Command("clearblood", "", ClearBlood_Command));
      AddedCommands.Add(new DebugConsole.Command("spawnblood", "spawnblood [size]", SpawnBlood_Command));
      AddedCommands.Add(new DebugConsole.Command("spawnbloodspectrum", "", SpawnBloodSpectrum_Command));
      AddedCommands.Add(new DebugConsole.Command("blooddecaldebug", "", BloodDecalDebug_Command));

      DebugConsole.Commands.InsertRange(0, AddedCommands);
    }
    public static void BloodDecalDebug_Command(string[] args)
    {
      AdvancedDecal.DecalDebug = !AdvancedDecal.DecalDebug;
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
      for (int x = 0; x < 20; x++)
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
    }

    public static void Bleed_Command(string[] args)
    {
      if (Character.Controlled is null) return;

      AfflictionPrefab afflictionPrefab = AfflictionPrefab.Bleeding;

      Limb targetLimb = Character.Controlled.AnimController.Limbs.FirstOrDefault(l => l.type.ToString().Equals("righthand", StringComparison.OrdinalIgnoreCase));

      Character.Controlled.CharacterHealth.ApplyAffliction(targetLimb ?? Character.Controlled.AnimController.MainLimb, afflictionPrefab.Instantiate(100));
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