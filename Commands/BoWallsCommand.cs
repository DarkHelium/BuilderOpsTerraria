using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Creates hollow walls (outline only) in the selection.
	/// Usage: /bo_walls <tile>
	/// </summary>
	public class BoWallsCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_walls";

		public override string Description => "Create hollow walls (outline only). Usage: /bo_walls <tile>";

		private ushort ParseTile(string input)
		{
			if (ushort.TryParse(input, out ushort id))
			{
				return id;
			}

			FieldInfo field = typeof(TileID).GetField(
				input,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase
			);

			if (field != null && field.FieldType == typeof(ushort))
			{
				return (ushort)field.GetValue(null);
			}

			return TileID.Stone;
		}

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
				caller.Reply("Usage: /bo_walls <tile>", Color.Red);
				return;
			}

			// Get selection
			var selection = Systems.SelectionSystem.GetSelection(player.whoAmI);
			if (!selection.IsValid())
			{
				caller.Reply("No valid selection! Use /bo_pos1 and /bo_pos2 to select a region.", Color.Red);
				return;
			}

			// Parse tile type
			ushort tileType = ParseTile(args[0]);

			// Execute operation
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				int count = Systems.WorldEditSystem.WallsRegion(selection, tileType);
				caller.Reply($"[BuilderOps] Creating hollow walls with {count} tiles of {tileType}", Color.Green);
			}
			else
			{
				caller.Reply("[BuilderOps] Multiplayer support for WorldEdit commands coming soon!", Color.Yellow);
			}
		}
	}
}

