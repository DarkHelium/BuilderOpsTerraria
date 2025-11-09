using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Counts tiles in the selection by type (Bill of Materials).
	/// Usage: /bo_count
	/// </summary>
	public class BoCountCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_count";

		public override string Description => "Count tiles in your selection (Bill of Materials).";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player ?? Main.LocalPlayer;
			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			// Get selection
			var selection = Systems.SelectionSystem.GetSelection(player.whoAmI);
			if (!selection.IsValid())
			{
				caller.Reply("No valid selection! Use /bo_pos1 and /bo_pos2 to select a region.", Color.Red);
				return;
			}

			// Count tiles
			Dictionary<ushort, int> counts = Systems.WorldEditSystem.CountTiles(selection);

			if (counts.Count == 0)
			{
				caller.Reply("[BuilderOps] Selection contains no tiles.", Color.Gray);
				return;
			}

			// Display results
			caller.Reply($"[BuilderOps] Bill of Materials ({counts.Values.Sum()} total tiles):", Color.Cyan);

			// Sort by count descending
			var sorted = counts.OrderByDescending(kvp => kvp.Value).Take(10);

			foreach (var kvp in sorted)
			{
				string tileName = GetTileName(kvp.Key);
				caller.Reply($"  {tileName} (ID {kvp.Key}): {kvp.Value}", Color.White);
			}

			if (counts.Count > 10)
			{
				caller.Reply($"  ... and {counts.Count - 10} more tile types", Color.Gray);
			}
		}

		private string GetTileName(ushort tileId)
		{
			// Try to get the tile name from TileID constants
			var fields = typeof(TileID).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(ushort) && (ushort)field.GetValue(null) == tileId)
				{
					return field.Name;
				}
			}
			return "Unknown";
		}
	}
}

