using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace BuilderOps.Commands
{
	/// <summary>
	/// Lists all available BuilderOps commands with descriptions.
	/// Usage: /bo_help
	/// </summary>
	public class BoHelpCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;

		public override string Command => "bo_help";

		public override string Description => "Show all BuilderOps commands.";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			// All commands in one list
			var commands = new List<(string cmd, string desc)>
			{
				// Selection commands
				("bo_pos1", "Set first corner of cuboid selection"),
				("bo_pos2", "Set second corner of cuboid selection"),
				("bo_center", "Set center point for sphere selection"),
				("bo_radius <blocks>", "Set radius for sphere selection"),
				("bo_sel [cuboid|sphere]", "Change selection mode"),
				("bo_desel", "Clear current selection"),
				
				// Tile manipulation
				("bo_set <tile>", "Fill selection with tile type"),
				("bo_replace <from> <to>", "Replace tiles in selection"),
				("bo_remove", "Remove all tiles in selection"),
				("bo_walls <tile>", "Create hollow walls (outline)"),
				("bo_count", "Count tiles in selection (BOM)"),
				
				// Wall manipulation
				("bo_setwall <wall>", "Fill selection with wall type"),
				("bo_replacewall <from> <to>", "Replace walls in selection"),
				("bo_removewall", "Remove all walls in selection"),
				("bo_setboth <tile> <wall>", "Set both tile and wall"),
				
			// Utility
			("bo_place [w] [h] [tile]", "Place rectangle of tiles"),
			("bo_stop", "Clear placement queue"),
			("bo_status", "Show queue status"),
			("bo_time [day|night|noon|midnight]", "Set time of day"),
			("bo_god", "Toggle god mode (invincibility, flight, instant mining)"),
			("bo_help", "Show this help message")
			};

			caller.Reply("=== BuilderOps Commands ===", Color.Cyan);
			
			foreach (var (cmd, desc) in commands)
			{
				caller.Reply($"/{cmd} - {desc}", Color.White);
			}
		}
	}
}

