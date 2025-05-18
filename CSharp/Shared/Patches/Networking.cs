using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using Barotrauma;
using HarmonyLib;
using Microsoft.Xna.Framework;

using Barotrauma.Items.Components;
using Barotrauma.Networking;
#if CLIENT
using Barotrauma.MapCreatures.Behavior;
#endif

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

#if CLIENT
      Mod.Harmony.Patch(
        original: typeof(Hull).GetMethod("ClientEventRead", AccessTools.all),
        prefix: new HarmonyMethod(typeof(NetworkingPatch).GetMethod("Hull_ClientEventRead_Replace"))
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

#if CLIENT
    public static void Hull_ClientEventRead_Replace(Hull __instance, ref bool __runOriginal, IReadMessage msg, float sendingTime)
    {
      Hull _ = __instance;
      __runOriginal = false;

      Hull.EventType eventType = (Hull.EventType)msg.ReadRangedInteger((int)Hull.EventType.MinValue, (int)Hull.EventType.MaxValue);
      switch (eventType)
      {
        case Hull.EventType.Status:
          _.remoteOxygenPercentage = msg.ReadRangedSingle(0.0f, 100.0f, 8);

          _.SharedStatusRead(
              msg,
              out float newWaterVolume,
              out Hull.NetworkFireSource[] newFireSources);

          _.remoteWaterVolume = newWaterVolume;
          _.remoteFireSources = newFireSources;
          break;
        case Hull.EventType.BackgroundSections:
          _.SharedBackgroundSectionRead(
              msg,
              bsnu =>
              {
                int i = bsnu.SectionIndex;
                Color color = bsnu.Color;
                float colorStrength = bsnu.ColorStrength;

                var remoteBackgroundSection = _.remoteBackgroundSections.Find(s => s.Index == i);
                if (remoteBackgroundSection != null)
                {
                  remoteBackgroundSection.SetColorStrength(colorStrength);
                  remoteBackgroundSection.SetColor(color);
                }
                else
                {
                  _.remoteBackgroundSections.Add(new BackgroundSection(new Rectangle(0, 0, 1, 1), (ushort)i, colorStrength, color, 0));
                }
              }, out int sectorToUpdate);
          _.paintAmount = _.BackgroundSections.Sum(s => s.ColorStrength);
          break;
        case Hull.EventType.Decal:
          //int decalCount = msg.ReadRangedInteger(0, Hull.MaxDecalsPerHull);
          int decalCount = msg.ReadInt32();
          Mod.Log($"decalCount:{decalCount}");
          if (decalCount == 0) { _.decals.Clear(); }
          _.remoteDecals.Clear();
          for (int i = 0; i < decalCount; i++)
          {
            UInt32 decalId = msg.ReadUInt32();
            int spriteIndex = msg.ReadByte();
            float normalizedXPos = msg.ReadRangedSingle(0.0f, 1.0f, 8);
            float normalizedYPos = msg.ReadRangedSingle(0.0f, 1.0f, 8);
            float decalScale = msg.ReadRangedSingle(0.0f, 2.0f, 12);
            _.remoteDecals.Add(new Hull.RemoteDecal(decalId, spriteIndex, new Vector2(normalizedXPos, normalizedYPos), decalScale));
          }
          break;
        case Hull.EventType.BallastFlora:
          BallastFloraBehavior.NetworkHeader header = (BallastFloraBehavior.NetworkHeader)msg.ReadByte();
          if (header == BallastFloraBehavior.NetworkHeader.Spawn)
          {
            Identifier identifier = msg.ReadIdentifier();
            float x = msg.ReadSingle();
            float y = msg.ReadSingle();
            _.BallastFlora = new BallastFloraBehavior(_, BallastFloraPrefab.Find(identifier), new Vector2(x, y), firstGrowth: true)
            {
              PowerConsumptionTimer = msg.ReadSingle()
            };
          }
          else
          {
            _.BallastFlora?.ClientRead(msg, header);
          }
          break;
        default:
          throw new Exception($"Malformed incoming hull event: {eventType} is not a supported event type");
      }

      if (_.serverUpdateDelay > 0.0f) { return; }

      _.ApplyRemoteState();
    }
#endif
  }
}