using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

/* Docs
 * Mod.GetPacket(int capacity = 256) : ModPacket — create a packet to write & send.
 * override void HandlePacket(BinaryReader reader, int whoAmI) — read/dispatch custom net messages.
 * static ModKeybind KeybindLoader.RegisterKeybind(Mod mod, string name, string defaultBinding) — register a configurable keybind; returns a ModKeybind.
 * WorldGen.PlaceTile(int x, int y, int type, bool mute = false, bool forced = false) — place a tile server-side/SP.
 * NetMessage.SendTileSquare(int remoteClient, int tileX, int tileY, int size) — sync a square of tiles to clients.
 * Main.netMode and NetmodeID.Server / NetmodeID.SinglePlayer — determine server/client context.
 * Terraria.Chat.ChatHelper.BroadcastChatMessage(NetworkText, Color) / Main.NewText(string, Color) — server/SP feedback.
 */

namespace BuilderOps
{
	/// <summary>
	/// Root mod class responsible for registering BuilderOps input and network helpers.
	/// </summary>
	public class BuilderOps : Mod
	{
		/// <summary>
		/// Keybind used to request placing a test Stone tile.
		/// </summary>
		public static ModKeybind PlaceTestKey { get; private set; }

	internal enum PacketId : byte
	{
		PlaceStoneTest = 1,
		EnqueueRect = 2,
		ClearQueue = 3
	}

		#region Lifecycle

		/// <summary>
		/// Registers keybinds and other runtime hooks.
		/// </summary>
		public override void Load()
		{
			PlaceTestKey = KeybindLoader.RegisterKeybind(this, "Place Test Tile", "P");

			// TODO: Batch placement pipeline.
			// TODO: Blueprint ingestion workflow.
			// TODO: Bill-of-materials generator.
		}

		/// <summary>
		/// Cleans up static references for hot-reload safety.
		/// </summary>
		public override void Unload()
		{
			PlaceTestKey = null;
		}

		#endregion

		#region Net

	/// <summary>
	/// Sends a PlaceStoneTest request to the authoritative server.
	/// </summary>
	public static void SendPlaceStonePacket(int x, int y)
	{
		ModPacket packet = ModContent.GetInstance<BuilderOps>().GetPacket();
		packet.Write((byte)PacketId.PlaceStoneTest);
		packet.Write((short)x);
		packet.Write((short)y);
		packet.Send();
	}

	/// <summary>
	/// Sends an EnqueueRect request to the server to queue a rectangle of tiles.
	/// </summary>
	public static void SendEnqueueRectPacket(int x, int y, int width, int height, ushort tileType)
	{
		ModPacket packet = ModContent.GetInstance<BuilderOps>().GetPacket();
		packet.Write((byte)PacketId.EnqueueRect);
		packet.Write((short)x);
		packet.Write((short)y);
		packet.Write((short)width);
		packet.Write((short)height);
		packet.Write(tileType);
		packet.Send();
	}

	/// <summary>
	/// Sends a ClearQueue request to the server.
	/// </summary>
	public static void SendClearQueuePacket()
	{
		ModPacket packet = ModContent.GetInstance<BuilderOps>().GetPacket();
		packet.Write((byte)PacketId.ClearQueue);
		packet.Send();
	}

	/// <summary>
	/// Handles network packets for BuilderOps functionality.
	/// </summary>
	public override void HandlePacket(BinaryReader reader, int whoAmI)
	{
		PacketId packetId = (PacketId)reader.ReadByte();
		switch (packetId)
		{
			case PacketId.PlaceStoneTest:
				if (Main.netMode != NetmodeID.Server)
				{
					return;
				}

				int x = reader.ReadInt16();
				int y = reader.ReadInt16();
				bool placed = WorldGen.PlaceTile(x, y, TileID.Stone, mute: true, forced: true);
				if (!placed)
				{
					return;
				}

				NetMessage.SendTileSquare(-1, x, y, 1);
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"[BuilderOps] Placed Stone at {x},{y}"), Color.LimeGreen);
				break;

			case PacketId.EnqueueRect:
				if (Main.netMode != NetmodeID.Server)
				{
					return;
				}

				int rectX = reader.ReadInt16();
				int rectY = reader.ReadInt16();
				int width = reader.ReadInt16();
				int height = reader.ReadInt16();
				ushort tileType = reader.ReadUInt16();

				Systems.PlacementQueueSystem.EnqueueRectServer(rectX, rectY, width, height, tileType);
				ChatHelper.BroadcastChatMessage(
					NetworkText.FromLiteral($"[BuilderOps] Enqueued {width}x{height} of tile {tileType} at ({rectX},{rectY})"),
					Color.Orange
				);
				break;

			case PacketId.ClearQueue:
				if (Main.netMode != NetmodeID.Server)
				{
					return;
				}

				int count = Systems.PlacementQueueSystem.Count;
				Systems.PlacementQueueSystem.ClearQueueServer();
				ChatHelper.BroadcastChatMessage(
					NetworkText.FromLiteral($"[BuilderOps] Queue cleared ({count} operations removed)."),
					Color.Yellow
				);
				break;

			default:
				Logger.Warn($"BuilderOps received unknown packet id: {(byte)packetId}");
				break;
		}
	}

		#endregion
	}
}
