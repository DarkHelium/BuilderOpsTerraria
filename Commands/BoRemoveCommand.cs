using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Removes all tiles in the current selection.
	/// Usage: /bo_remove or /bo_clear
	/// </summary>
	public class BoRemoveCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_remove";

		public override string Description => "Remove all tiles in your selection.";

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

			// Check selection size
			var config = ModContent.GetInstance<Config.BuilderOpsConfig>();
			int maxSize = config?.MaxSelectionSize ?? 10000;
			int tileCount = selection.GetTileCount();

			if (tileCount > maxSize)
			{
				caller.Reply($"Selection too large! ({tileCount} tiles, max: {maxSize})", Color.Red);
				return;
			}

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.RemoveRegion(selection);
				caller.Reply($"[BuilderOps] Removing {count} tiles", Color.Orange);
			}
			else
			{
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

