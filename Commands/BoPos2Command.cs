using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Sets the second corner of a cuboid selection.
	/// Usage: /bo_pos2
	/// </summary>
	public class BoPos2Command : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_pos2";

		public override string Description => "Set the second corner of your selection at your position.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player ?? Main.LocalPlayer;
			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			// Get tile position at player center
			int tileX = (int)(player.Center.X / 16f);
			int tileY = (int)(player.Center.Y / 16f);

			// Set position 2
			Systems.SelectionSystem.SetPos2(player.whoAmI, new Point(tileX, tileY));
			
			// Set selection type to cuboid
			Systems.SelectionSystem.SetSelectionType(player.whoAmI, Systems.SelectionSystem.SelectionType.Cuboid);

			// Show selection info
			var sel = Systems.SelectionSystem.GetSelection(player.whoAmI);
			if (sel.IsValid())
			{
				Rectangle bounds = sel.GetBounds();
				int count = sel.GetTileCount();
				caller.Reply($"[BuilderOps] Position 2 set to ({tileX}, {tileY}). Selection: {bounds.Width}x{bounds.Height} ({count} tiles)", Color.Cyan);
			}
			else
			{
				caller.Reply($"[BuilderOps] Position 2 set to ({tileX}, {tileY})", Color.Cyan);
			}
		}
	}
}

