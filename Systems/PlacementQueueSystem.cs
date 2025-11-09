using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Systems
{
	/// <summary>
	/// Server-side queue system for batched tile placement with per-tick budget control.
	/// </summary>
	public class PlacementQueueSystem : ModSystem
	{
	/// <summary>
	/// Operation type for tile/wall manipulation.
	/// </summary>
	public enum OperationType
	{
		PlaceTile,
		RemoveTile,
		PlaceWall,
		RemoveWall
	}

	/// <summary>
	/// Represents a single tile/wall placement/removal operation.
	/// </summary>
	public struct PlacementOp
	{
		public int X;
		public int Y;
		public ushort Type;        // Tile type or wall type depending on OpType
		public ushort WallType;    // For combined operations
		public OperationType OpType;

		public PlacementOp(int x, int y, ushort type, OperationType opType = OperationType.PlaceTile)
		{
			X = x;
			Y = y;
			Type = type;
			WallType = 0;
			OpType = opType;
		}

		public PlacementOp(int x, int y, ushort tileType, ushort wallType, OperationType opType)
		{
			X = x;
			Y = y;
			Type = tileType;
			WallType = wallType;
			OpType = opType;
		}
	}

		private static Queue<PlacementOp> _queue = new Queue<PlacementOp>();

		/// <summary>
		/// Gets the current number of operations in the queue.
		/// </summary>
		public static int Count => _queue.Count;

		/// <summary>
		/// Gets or sets the maximum number of tiles to place per tick.
		/// </summary>
		public static int TilesPerTickBudget { get; set; } = 64;

	/// <summary>
	/// Enqueues a single tile placement operation (server-side only).
	/// </summary>
	public static void EnqueueServer(int x, int y, ushort tileType)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		_queue.Enqueue(new PlacementOp(x, y, tileType, OperationType.PlaceTile));
	}

	/// <summary>
	/// Enqueues a tile operation with specific operation type (server-side only).
	/// </summary>
	public static void EnqueueOperation(int x, int y, ushort tileType, OperationType opType)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		_queue.Enqueue(new PlacementOp(x, y, tileType, opType));
	}

		/// <summary>
		/// Enqueues a rectangle of tiles (server-side only).
		/// </summary>
		public static void EnqueueRectServer(int startX, int startY, int width, int height, ushort tileType)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			for (int dy = 0; dy < height; dy++)
			{
				for (int dx = 0; dx < width; dx++)
				{
					_queue.Enqueue(new PlacementOp(startX + dx, startY + dy, tileType));
				}
			}
		}

		/// <summary>
		/// Clears the entire queue (server-side only).
		/// </summary>
		public static void ClearQueueServer()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return;
			}

			_queue.Clear();
		}

	/// <summary>
	/// Processes queued tile placements/removals up to the per-tick budget.
	/// </summary>
	public override void PreUpdateWorld()
	{
		// Only run on server or singleplayer
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		int budget = TilesPerTickBudget;

		while (budget > 0 && _queue.Count > 0)
		{
			PlacementOp op = _queue.Dequeue();
			bool success = false;

			switch (op.OpType)
			{
				case OperationType.PlaceTile:
					// Place tile
					success = WorldGen.PlaceTile(op.X, op.Y, op.Type, mute: true, forced: true);
					break;

				case OperationType.RemoveTile:
					// Remove tile
					WorldGen.KillTile(op.X, op.Y, false, false, true);
					success = true;
					break;

				case OperationType.PlaceWall:
					// Place wall
					WorldGen.PlaceWall(op.X, op.Y, op.Type, mute: true);
					success = true;
					break;

				case OperationType.RemoveWall:
					// Remove wall
					WorldGen.KillWall(op.X, op.Y, false);
					success = true;
					break;
			}

			if (success && Main.netMode == NetmodeID.Server)
			{
				// Sync to all clients in multiplayer
				NetMessage.SendTileSquare(-1, op.X, op.Y, 1);
			}

			budget--;
		}
	}

		/// <summary>
		/// Clears the queue on unload for hot-reload safety.
		/// </summary>
		public override void Unload()
		{
			_queue?.Clear();
			_queue = null;
		}
	}
}

