using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Chat command to stop and clear the placement queue.
	/// Usage: /bo_stop
	/// </summary>
	public class BoStopCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_stop";

		public override string Description => "Stop and clear the BuilderOps placement queue.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.PlacementQueueSystem.Count;
				Systems.PlacementQueueSystem.ClearQueueServer();
				Main.NewText($"[BuilderOps] Queue cleared ({count} operations removed).", Color.Yellow);
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				// Server can clear directly
				int count = Systems.PlacementQueueSystem.Count;
				Systems.PlacementQueueSystem.ClearQueueServer();
				caller.Reply($"[BuilderOps] Queue cleared ({count} operations removed).", Color.Yellow);
			}
			else
			{
				// Client needs to send a packet to request clear
				BuilderOps.SendClearQueuePacket();
				caller.Reply("[BuilderOps] Requested queue clear from server.", Color.Yellow);
			}
		}
	}
}

