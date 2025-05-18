using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using Barotrauma.Items.Components;
using Barotrauma.Networking;

namespace NoDecalLimit
{
  public class NetworkingPatch
  {
    public static void PatchAll()
    {
      // I really shouldn't replace those methods just to remove 1 line, i should use transpilers, in some distant future
#if SERVER
      Mod.Harmony.Patch(
        original: typeof(Hull).GetMethod("ServerEventWrite", AccessTools.all),
        prefix: new HarmonyMethod(typeof(NetworkingPatch).GetMethod("Hull_ServerEventWrite_Replace"))
      );
#endif
    }

#if SERVER
    public static void Hull_ServerEventWrite_Replace(Hull __instance, ref bool __runOriginal, IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
    {
      Hull _ = __instance;
      __runOriginal = false;

      if (!(extraData is Hull.IEventData eventData)) { throw new Exception($"Malformed hull event: expected {nameof(Hull)}.{nameof(Hull.IEventData)}"); }
      msg.WriteRangedInteger((int)eventData.EventType, (int)Hull.EventType.MinValue, (int)Hull.EventType.MaxValue);

      switch (eventData)
      {
        case Hull.StatusEventData statusEventData:
          msg.WriteRangedSingle(MathHelper.Clamp(_.OxygenPercentage, 0.0f, 100.0f), 0.0f, 100.0f, 8);
          _.SharedStatusWrite(msg);
          break;
        case Hull.BackgroundSectionsEventData backgroundSectionsEventData:
          _.SharedBackgroundSectionsWrite(msg, backgroundSectionsEventData);
          break;
        case Hull.DecalEventData decalEventData:
          // no thx
          // msg.WriteRangedInteger(_.decals.Count, 0, Hull.MaxDecalsPerHull);
          msg.WriteInt32(_.decals.Count);
          foreach (Decal decal in _.decals)
          {
            msg.WriteUInt32(decal.Prefab.UintIdentifier);
            msg.WriteByte((byte)decal.SpriteIndex);
            float normalizedXPos = MathHelper.Clamp(MathUtils.InverseLerp(0.0f, _.rect.Width, decal.CenterPosition.X), 0.0f, 1.0f);
            float normalizedYPos = MathHelper.Clamp(MathUtils.InverseLerp(-_.rect.Height, 0.0f, decal.CenterPosition.Y), 0.0f, 1.0f);
            msg.WriteRangedSingle(normalizedXPos, 0.0f, 1.0f, 8);
            msg.WriteRangedSingle(normalizedYPos, 0.0f, 1.0f, 8);
            msg.WriteRangedSingle(decal.Scale, 0f, 2f, 12);
          }
          break;
        case Hull.BallastFloraEventData ballastFloraEventData:
          ballastFloraEventData.Behavior.ServerWrite(msg, ballastFloraEventData.SubEventData);
          break;
        default:
          throw new Exception($"Malformed hull event: did not expect {eventData.GetType().Name}");
      }
    }
#endif


  }
}