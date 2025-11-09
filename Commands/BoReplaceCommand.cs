using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Replaces specific tiles in the selection with another tile type.
	/// Usage: /bo_replace <fromTile> <toTile>
	/// </summary>
	public class BoReplaceCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_replace";

		public override string Description => "Replace tiles in selection. Usage: /bo_replace <fromTile> <toTile>";

		private ushort ParseTile(string input)
		{
			if (ushort.TryParse(input, out ushort id))
			{
				return id;
			}

			FieldInfo field = typeof(TileID).GetField(
				input,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase
			);

			if (field != null && field.FieldType == typeof(ushort))
			{
				return (ushort)field.GetValue(null);
			}

			return TileID.Stone;
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player ?? Main.LocalPlayer;
			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			if (args.Length < 2)
			{
				caller.Reply("Usage: /bo_replace <fromTile> <toTile>", Color.Red);
				return;
			}

			// Get selection
			var selection = Systems.SelectionSystem.GetSelection(player.whoAmI);
			if (!selection.IsValid())
			{
				caller.Reply("No valid selection! Use /bo_pos1 and /bo_pos2 to select a region.", Color.Red);
				return;
			}

			// Check selection size
			var config = ModContent.GetInstance<Config.BuilderOpsConfig>();
			int maxSize = config?.MaxSelectionSize ?? 10000;
			int tileCount = selection.GetTileCount();

			if (tileCount > maxSize)
			{
				caller.Reply($"Selection too large! ({tileCount} tiles, max: {maxSize})", Color.Red);
				return;
			}

			// Parse tile types
			ushort fromTile = ParseTile(args[0]);
			ushort toTile = ParseTile(args[1]);

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.ReplaceRegion(selection, fromTile, toTile);
				caller.Reply($"[BuilderOps] Replacing {count} tiles ({fromTile} → {toTile})", Color.Green);
			}
			else
			{
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

