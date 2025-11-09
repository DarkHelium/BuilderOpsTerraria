using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Systems
{
	/// <summary>
	/// Core WorldEdit-style manipulation logic for regions.
	/// </summary>
	public class WorldEditSystem : ModSystem
	{
		/// <summary>
		/// Fills a selection with a specific tile type.
		/// </summary>
		public static int SetRegion(SelectionSystem.Selection selection, ushort tileType)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					PlacementQueueSystem.EnqueueServer(x, y, tileType);
					count++;
				}
			}

			return count;
		}

		/// <summary>
		/// Replaces specific tiles in a selection with another tile type.
		/// </summary>
		public static int ReplaceRegion(SelectionSystem.Selection selection, ushort fromTile, ushort toTile)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					Tile tile = Main.tile[x, y];
					if (tile.HasTile && tile.TileType == fromTile)
					{
						PlacementQueueSystem.EnqueueServer(x, y, toTile);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Removes all tiles in a selection.
		/// </summary>
		public static int RemoveRegion(SelectionSystem.Selection selection)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					Tile tile = Main.tile[x, y];
					if (tile.HasTile)
					{
						PlacementQueueSystem.EnqueueOperation(x, y, 0, PlacementQueueSystem.OperationType.RemoveTile);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Creates hollow walls (outline only) in a cuboid selection.
		/// </summary>
		public static int WallsRegion(SelectionSystem.Selection selection, ushort tileType)
		{
			if (!selection.IsValid()) return 0;
			if (selection.Type != SelectionSystem.SelectionType.Cuboid)
			{
				// For sphere, just do a hollow sphere
				return HollowSphereRegion(selection, tileType);
			}

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;

					// Only place on edges
					bool isEdge = (x == bounds.X || x == bounds.X + bounds.Width - 1 ||
					               y == bounds.Y || y == bounds.Y + bounds.Height - 1);

					if (isEdge)
					{
						PlacementQueueSystem.EnqueueServer(x, y, tileType);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Creates a hollow sphere outline.
		/// </summary>
		private static int HollowSphereRegion(SelectionSystem.Selection selection, ushort tileType)
		{
			Rectangle bounds = selection.GetBounds();
			int count = 0;
			int radius = selection.Radius;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;

					int dx = x - selection.Center.X;
					int dy = y - selection.Center.Y;
					float dist = (float)System.Math.Sqrt(dx * dx + dy * dy);

					// Only place on the edge (within 1 block of radius)
					if (dist >= radius - 1 && dist <= radius + 1)
					{
						PlacementQueueSystem.EnqueueServer(x, y, tileType);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Counts tiles in a selection by type.
		/// </summary>
		public static Dictionary<ushort, int> CountTiles(SelectionSystem.Selection selection)
		{
			Dictionary<ushort, int> counts = new Dictionary<ushort, int>();
			if (!selection.IsValid()) return counts;

			Rectangle bounds = selection.GetBounds();

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					Tile tile = Main.tile[x, y];
					if (tile.HasTile)
					{
						ushort type = tile.TileType;
						if (!counts.ContainsKey(type))
						{
							counts[type] = 0;
						}
						counts[type]++;
					}
				}
			}

			return counts;
		}

		/// <summary>
		/// Fills a selection with a specific wall type.
		/// </summary>
		public static int SetWallRegion(SelectionSystem.Selection selection, ushort wallType)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					PlacementQueueSystem.EnqueueOperation(x, y, wallType, PlacementQueueSystem.OperationType.PlaceWall);
					count++;
				}
			}

			return count;
		}

		/// <summary>
		/// Replaces specific walls in a selection with another wall type.
		/// </summary>
		public static int ReplaceWallRegion(SelectionSystem.Selection selection, ushort fromWall, ushort toWall)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					Tile tile = Main.tile[x, y];
					if (tile.WallType == fromWall)
					{
						PlacementQueueSystem.EnqueueOperation(x, y, toWall, PlacementQueueSystem.OperationType.PlaceWall);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Removes all walls in a selection.
		/// </summary>
		public static int RemoveWallRegion(SelectionSystem.Selection selection)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					Tile tile = Main.tile[x, y];
					if (tile.WallType > 0)
					{
						PlacementQueueSystem.EnqueueOperation(x, y, 0, PlacementQueueSystem.OperationType.RemoveWall);
						count++;
					}
				}
			}

			return count;
		}

		/// <summary>
		/// Sets both tile and wall simultaneously in a selection.
		/// </summary>
		public static int SetBothRegion(SelectionSystem.Selection selection, ushort tileType, ushort wallType)
		{
			if (!selection.IsValid()) return 0;

			Rectangle bounds = selection.GetBounds();
			int count = 0;

			for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
			{
				for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
				{
					if (!WorldGen.InWorld(x, y)) continue;
					if (!selection.Contains(x, y)) continue;

					// Enqueue tile placement
					PlacementQueueSystem.EnqueueServer(x, y, tileType);
					// Enqueue wall placement
					PlacementQueueSystem.EnqueueOperation(x, y, wallType, PlacementQueueSystem.OperationType.PlaceWall);
					count++;
				}
			}

			return count;
		}
	}
}

