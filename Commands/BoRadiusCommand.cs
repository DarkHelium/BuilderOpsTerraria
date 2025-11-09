using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Sets the radius for a sphere selection.
	/// Usage: /bo_radius <blocks>
	/// </summary>
	public class BoRadiusCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_radius";

		public override string Description => "Set the radius for sphere selection. Usage: /bo_radius <blocks>";

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
				caller.Reply("Usage: /bo_radius <blocks>", Color.Red);
				return;
			}

			if (!int.TryParse(args[0], out int radius) || radius <= 0)
			{
				caller.Reply("Radius must be a positive number.", Color.Red);
				return;
			}

			// Check max selection size
			var config = ModContent.GetInstance<Config.BuilderOpsConfig>();
			int maxSize = config?.MaxSelectionSize ?? 10000;
			int estimatedTiles = (int)(radius * radius * 3.14159); // Approximate circle area

			if (estimatedTiles > maxSize)
			{
				caller.Reply($"Radius too large! Would create ~{estimatedTiles} tiles (max: {maxSize}). Try a smaller radius.", Color.Red);
				return;
			}

			// Set radius
			Systems.SelectionSystem.SetRadius(player.whoAmI, radius);
			
			// Set selection type to sphere
			Systems.SelectionSystem.SetSelectionType(player.whoAmI, Systems.SelectionSystem.SelectionType.Sphere);

			// Show selection info
			var sel = Systems.SelectionSystem.GetSelection(player.whoAmI);
			if (sel.IsValid())
			{
				int count = sel.GetTileCount();
				caller.Reply($"[BuilderOps] Sphere radius set to {radius} blocks (~{count} tiles)", Color.Yellow);
			}
			else
			{
				caller.Reply($"[BuilderOps] Sphere radius set to {radius} blocks. Use /bo_center to set the center point.", Color.Yellow);
			}
		}
	}
}

