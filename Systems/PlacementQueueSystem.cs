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
		/// Represents a single tile placement operation.
		/// </summary>
		public struct PlacementOp
		{
			public int X;
			public int Y;
			public ushort Type;

			public PlacementOp(int x, int y, ushort type)
			{
				X = x;
				Y = y;
				Type = type;
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

			_queue.Enqueue(new PlacementOp(x, y, tileType));
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
		/// Processes queued tile placements up to the per-tick budget.
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

				// Attempt to place the tile
				bool placed = WorldGen.PlaceTile(op.X, op.Y, op.Type, mute: true, forced: true);

				if (placed)
				{
					// Sync to all clients in multiplayer
					if (Main.netMode == NetmodeID.Server)
					{
						NetMessage.SendTileSquare(-1, op.X, op.Y, 1);
					}
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

