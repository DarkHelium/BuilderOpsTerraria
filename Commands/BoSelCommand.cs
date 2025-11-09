using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Changes the selection mode between cuboid and sphere.
	/// Usage: /bo_sel [cuboid|sphere]
	/// </summary>
	public class BoSelCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_sel";

		public override string Description => "Change selection mode. Usage: /bo_sel [cuboid|sphere]";

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
				// Show current mode
				var sel = Systems.SelectionSystem.GetSelection(player.whoAmI);
				caller.Reply($"[BuilderOps] Current selection mode: {sel.Type}", Color.Cyan);
				return;
			}

			string mode = args[0].ToLower();
			Systems.SelectionSystem.SelectionType newType;

			switch (mode)
			{
				case "cuboid":
				case "cube":
				case "box":
				case "rect":
				case "rectangle":
					newType = Systems.SelectionSystem.SelectionType.Cuboid;
					break;

				case "sphere":
				case "circle":
				case "round":
					newType = Systems.SelectionSystem.SelectionType.Sphere;
					break;

				default:
					caller.Reply($"Unknown selection mode '{mode}'. Use 'cuboid' or 'sphere'.", Color.Red);
					return;
			}

			Systems.SelectionSystem.SetSelectionType(player.whoAmI, newType);
			caller.Reply($"[BuilderOps] Selection mode set to {newType}", Color.Cyan);
		}
	}
}

