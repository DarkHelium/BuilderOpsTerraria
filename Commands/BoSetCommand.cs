using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Fills the current selection with a tile type.
	/// Usage: /bo_set <tile>
	/// </summary>
	public class BoSetCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_set";

		public override string Description => "Fill your selection with a tile type. Usage: /bo_set <tile>";

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

			if (args.Length == 0)
			{
				caller.Reply("Usage: /bo_set <tile>", Color.Red);
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

			// Parse tile type
			ushort tileType = ParseTile(args[0]);

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.SetRegion(selection, tileType);
				caller.Reply($"[BuilderOps] Enqueued {count} tiles to set to {tileType}", Color.Green);
			}
			else
			{
				// TODO: Add multiplayer packet support for WorldEdit operations
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

