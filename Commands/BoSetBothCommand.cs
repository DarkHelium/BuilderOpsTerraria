using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Sets both tile and wall simultaneously in the selection.
	/// Usage: /bo_setboth <tile> <wall>
	/// </summary>
	public class BoSetBothCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_setboth";

		public override string Description => "Set both tile and wall in selection. Usage: /bo_setboth <tile> <wall>";

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
				caller.Reply("Usage: /bo_setboth <tile> <wall>", Color.Red);
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

			// Parse types
			ushort tileType = Utilities.TileParser.ParseTile(args[0]);
			ushort wallType = Utilities.TileParser.ParseWall(args[1]);

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.SetBothRegion(selection, tileType, wallType);
				caller.Reply($"[BuilderOps] Setting {count} tiles to {tileType} with wall {wallType}", Color.Green);
			}
			else
			{
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

