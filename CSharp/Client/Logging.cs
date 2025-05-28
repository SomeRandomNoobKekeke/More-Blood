using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Diagnostics;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;

namespace MoreBlood
{
  public partial class Mod
  {
    /// <summary>
    /// $"‖color:{color}‖{msg}‖end‖"
    /// </summary>
    public static string WrapInColor(object msg, string color)
    {
      return $"‖color:{color}‖{msg}‖end‖";
    }

    /// <summary>
    /// Prints a message to console
    /// </summary>
    public static void Log(object msg, Color? color = null, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      color ??= Color.Cyan;
      // var fi = new FileInfo(source);
      // LuaCsLogger.LogMessage($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", color * 0.6f, color * 0.6f);
      LuaCsLogger.LogMessage($"{msg ?? "null"}", color * 0.8f, color);
    }

    public static void Warning(object msg, Color? color = null, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      color ??= Color.Yellow;
      // var fi = new FileInfo(source);
      // LuaCsLogger.LogMessage($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", color * 0.6f, color * 0.6f);
      LuaCsLogger.LogMessage($"{msg ?? "null"}", color * 0.8f, color);
    }


    private static Dictionary<string, int> Traced = new();
    public static void AddTracer(string key, int i = 1) => Traced[key] = i;
    public static bool Trace(string key)
    {
      if (Traced.ContainsKey(key))
      {
        Traced[key]--;
        if (Traced[key] <= 0) Traced.Remove(key);
        return true;
      }
      return false;
    }

    public static void Point([CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      var fi = new FileInfo(source);
      Log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Magenta);
    }

    public static void PrintStackTrace()
    {
      StackTrace st = new StackTrace(true);
      for (int i = 0; i < st.FrameCount; i++)
      {
        StackFrame sf = st.GetFrame(i);
        if (sf.GetMethod().DeclaringType is null)
        {
          Log($"-> {sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod()}");
          break;
        }
        Log($"-> {sf.GetMethod().DeclaringType?.Name}.{sf.GetMethod()}");
      }
    }


    /// <summary>
    /// xd
    /// </summary>
    /// <param name="source"> This should be injected by compiler, don't set </param>
    public static string GetCallerFolderPath([CallerFilePath] string source = "") => Path.GetDirectoryName(source);

    /// <summary>
    /// Prints debug message with source path
    /// Works only if debug is true
    /// </summary>
    public static void Info(object msg, [CallerFilePath] string source = "", [CallerLineNumber] int lineNumber = 0)
    {
      if (Debug.PluginDebug)
      {
        var fi = new FileInfo(source);

        Log($"{fi.Directory.Name}/{fi.Name}:{lineNumber}", Color.Cyan * 0.5f);
        Log(msg, Color.Cyan);
      }
    }
  }
}
