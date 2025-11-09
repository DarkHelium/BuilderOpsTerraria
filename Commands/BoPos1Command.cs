using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Sets the first corner of a cuboid selection.
	/// Usage: /bo_pos1
	/// </summary>
	public class BoPos1Command : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_pos1";

		public override string Description => "Set the first corner of your selection at your position.";

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

			// Set position 1
			Systems.SelectionSystem.SetPos1(player.whoAmI, new Point(tileX, tileY));
			
			// Set selection type to cuboid
			Systems.SelectionSystem.SetSelectionType(player.whoAmI, Systems.SelectionSystem.SelectionType.Cuboid);

			caller.Reply($"[BuilderOps] Position 1 set to ({tileX}, {tileY})", Color.Cyan);
		}
	}
}

