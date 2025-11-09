using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Chat command to check the current status of the placement queue.
	/// Usage: /bo_status
	/// </summary>
	public class BoStatusCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_status";

		public override string Description => "Show the current BuilderOps queue status.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			int count = Systems.PlacementQueueSystem.Count;
			int budget = Systems.PlacementQueueSystem.TilesPerTickBudget;

			string message = $"[BuilderOps] Queue: {count} operations | Budget: {budget} tiles/tick";
			
			if (Main.netMode == Terraria.ID.NetmodeID.SinglePlayer)
			{
				Main.NewText(message, Color.Cyan);
			}
			else
			{
				caller.Reply(message, Color.Cyan);
			}
		}
	}
}

