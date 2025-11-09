using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Fills the current selection with a wall type.
	/// Usage: /bo_setwall <wall>
	/// </summary>
	public class BoSetWallCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_setwall";

		public override string Description => "Fill your selection with a wall type. Usage: /bo_setwall <wall>";

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
				caller.Reply("Usage: /bo_setwall <wall>", Color.Red);
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

			// Parse wall type
			ushort wallType = Utilities.TileParser.ParseWall(args[0]);

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.SetWallRegion(selection, wallType);
				caller.Reply($"[BuilderOps] Enqueued {count} walls to set to {wallType}", Color.Green);
			}
			else
			{
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

