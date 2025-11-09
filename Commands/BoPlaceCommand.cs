using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Chat command to enqueue a rectangle of tiles for placement.
	/// Usage: /bo_place [width] [height] [tileNameOrId]
	/// </summary>
	public class BoPlaceCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_place";

		public override string Description => "Enqueue a rectangle of tiles. Usage: /bo_place [width] [height] [tileNameOrId]";

		/// <summary>
		/// Parses a tile name or numeric ID into a tile type.
		/// </summary>
		private ushort ParseTile(string input)
		{
			// Try parsing as a number first
			if (ushort.TryParse(input, out ushort id))
			{
				return id;
			}

			// Try to find a matching field in TileID class (case-insensitive)
			FieldInfo? field = typeof(TileID).GetField(
				input,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase
			);

			if (field != null && field.FieldType == typeof(ushort))
			{
				return (ushort)field.GetValue(null);
			}

			// Default to Stone if not found
			return TileID.Stone;
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			// Parse arguments with defaults
			int width = 12;
			int height = 6;
			ushort tileType = TileID.Stone;

			if (args.Length >= 1 && int.TryParse(args[0], out int w))
			{
				width = w;
			}

			if (args.Length >= 2 && int.TryParse(args[1], out int h))
			{
				height = h;
			}

			if (args.Length >= 3)
			{
				tileType = ParseTile(args[2]);
			}

			// Get the player (caller or local player)
			Player player = caller.Player ?? Main.LocalPlayer;

			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			// Calculate tile position (offset from player center)
			int targetX = (int)(player.Center.X / 16f) + 2;
			int targetY = (int)(player.Center.Y / 16f) + 2;

			// Handle singleplayer vs multiplayer
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				Systems.PlacementQueueSystem.EnqueueRectServer(targetX, targetY, width, height, tileType);
				Main.NewText($"[BuilderOps] Enqueued {width}x{height} of tile {tileType} at ({targetX},{targetY})", Color.Orange);
			}
			else
			{
				// Send packet to server to enqueue
				BuilderOps.SendEnqueueRectPacket(targetX, targetY, width, height, tileType);
				caller.Reply($"[BuilderOps] Requested {width}x{height} of tile {tileType} at ({targetX},{targetY})", Color.Orange);
			}
		}
	}
}

