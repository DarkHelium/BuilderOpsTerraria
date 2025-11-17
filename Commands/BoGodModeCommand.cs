using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Toggles god mode: invincibility, flight, and instant mining.
	/// Usage: /bo_god
	/// </summary>
	public class BoGodModeCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_god";

		public override string Description => "Toggle god mode (invincibility, flight, instant mining).";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player player = caller.Player ?? Main.LocalPlayer;
			if (player == null || !player.active)
			{
				caller.Reply("No active player found.", Color.Red);
				return;
			}

			// Get the god mode player
			var godModePlayer = player.GetModPlayer<Players.GodModePlayer>();
			
			// Toggle god mode
			godModePlayer.GodModeActive = !godModePlayer.GodModeActive;

			if (godModePlayer.GodModeActive)
			{
				caller.Reply("[BuilderOps] God mode ENABLED", Color.Gold);
				caller.Reply("  ✓ Invincibility", Color.LightGreen);
				caller.Reply("  ✓ Creative flight (double-tap jump to toggle hover)", Color.LightGreen);
				caller.Reply("  ✓ Instant mining", Color.LightGreen);
			}
			else
			{
				caller.Reply("[BuilderOps] God mode DISABLED", Color.Gray);
			}
		}
	}
}

