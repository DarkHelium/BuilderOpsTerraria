using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Clears the current selection.
	/// Usage: /bo_desel
	/// </summary>
	public class BoDeselCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_desel";

		public override string Description => "Clear your current selection.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player ?? Main.LocalPlayer;
			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			Systems.SelectionSystem.ClearSelection(player.whoAmI);
			caller.Reply("[BuilderOps] Selection cleared.", Color.Gray);
		}
	}
}

