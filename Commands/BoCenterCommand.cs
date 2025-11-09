using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Sets the center point for a sphere selection.
	/// Usage: /bo_center
	/// </summary>
	public class BoCenterCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_center";

		public override string Description => "Set the center point for sphere selection at your position.";

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

			// Set center
			Systems.SelectionSystem.SetCenter(player.whoAmI, new Point(tileX, tileY));
			
			// Set selection type to sphere
			Systems.SelectionSystem.SetSelectionType(player.whoAmI, Systems.SelectionSystem.SelectionType.Sphere);

			caller.Reply($"[BuilderOps] Sphere center set to ({tileX}, {tileY})", Color.Yellow);
		}
	}
}

