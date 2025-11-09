using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Chat command to set the in-game time to day, night, noon, or midnight.
	/// Usage: /bo_time [day|night|noon|midnight]
	/// </summary>
	public class BoTimeCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_time";

		public override string Description => "Set the time. Usage: /bo_time [day|night|noon|midnight]";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			if (args.Length == 0)
			{
				caller.Reply("Usage: /bo_time [day|night|noon|midnight]", Color.Red);
				return;
			}

			string timeArg = args[0].ToLower();

			switch (timeArg)
			{
				case "day":
				case "dawn":
				case "morning":
					// 4:30 AM - Start of day
					Main.dayTime = true;
					Main.time = 0;
					caller.Reply("[BuilderOps] Time set to dawn (4:30 AM).", Color.Yellow);
					break;

				case "noon":
				case "midday":
					// 12:00 PM - Middle of day
					Main.dayTime = true;
					Main.time = 27000; // Noon is at 27000 ticks
					caller.Reply("[BuilderOps] Time set to noon (12:00 PM).", Color.Yellow);
					break;

				case "night":
				case "dusk":
				case "evening":
					// 7:30 PM - Start of night
					Main.dayTime = false;
					Main.time = 0;
					caller.Reply("[BuilderOps] Time set to dusk (7:30 PM).", Color.DarkBlue);
					break;

				case "midnight":
					// 12:00 AM - Middle of night
					Main.dayTime = false;
					Main.time = 16200; // Midnight is at 16200 ticks
					caller.Reply("[BuilderOps] Time set to midnight (12:00 AM).", Color.DarkBlue);
					break;

				default:
					caller.Reply($"Unknown time '{timeArg}'. Use: day, night, noon, or midnight.", Color.Red);
					break;
			}

		if (Main.netMode == Terraria.ID.NetmodeID.Server)
		{
			NetMessage.SendData(Terraria.ID.MessageID.WorldData);
		}
		}
	}
}

