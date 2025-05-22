using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;
using System.IO;

namespace MoreBlood
{

  /// <summary>
  /// Parse primitive types, can be extended with ParseMethods
  /// </summary>
  public static class Parser
  {
    public static object DefaultFor(Type T) => Activator.CreateInstance(T);

    public static T Parse<T>(string raw) => (T)Parse(raw, typeof(T));
    public static object Parse(string raw, Type T, bool verbose = true)
    {
      if (raw == null) return null;
      if (T == typeof(string)) return raw;

      if (T.IsPrimitive)
      {
        MethodInfo parse = T.GetMethod(
          "Parse",
          BindingFlags.Public | BindingFlags.Static,
          new Type[] { typeof(string) }
        );

        try
        {
          return parse.Invoke(null, new object[] { raw });
        }
        catch (Exception e)
        {
          if (verbose)
          {
            Mod.Warning($"Parser failed to parse [{raw}] into primitive type [{T}]");
          }
          else throw e;

          return DefaultFor(T);
        }
      }

      if (T.IsEnum)
      {
        try
        {
          return Enum.Parse(T, raw);
        }
        catch (Exception e)
        {
          if (verbose)
          {
            Mod.Warning($"Parser failed to parse [{raw}] into Enum [{T}]");
          }
          else throw e;

          return DefaultFor(T);
        }
      }

      if (!T.IsPrimitive)
      {
        throw new NotImplementedException("bruh");
      }

      return DefaultFor(T);
    }
  }
}
