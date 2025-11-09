using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Systems
{
	/// <summary>
	/// Manages per-player selections for WorldEdit-style operations.
	/// </summary>
	public class SelectionSystem : ModSystem
	{
		/// <summary>
		/// Selection type enumeration.
		/// </summary>
		public enum SelectionType
		{
			Cuboid,
			Sphere
		}

		/// <summary>
		/// Represents a player's selection region.
		/// </summary>
		public class Selection
		{
			public Point Point1 { get; set; } = Point.Zero;
			public Point Point2 { get; set; } = Point.Zero;
			public SelectionType Type { get; set; } = SelectionType.Cuboid;
			public Point Center { get; set; } = Point.Zero;
			public int Radius { get; set; } = 0;
			public bool HasPoint1 { get; set; } = false;
			public bool HasPoint2 { get; set; } = false;
			public bool HasCenter { get; set; } = false;
			public bool HasRadius { get; set; } = false;

			/// <summary>
			/// Gets whether the selection is valid and ready to use.
			/// </summary>
			public bool IsValid()
			{
				if (Type == SelectionType.Cuboid)
				{
					return HasPoint1 && HasPoint2;
				}
				else // Sphere
				{
					return HasCenter && HasRadius && Radius > 0;
				}
			}

			/// <summary>
			/// Gets the bounding rectangle for this selection.
			/// </summary>
			public Rectangle GetBounds()
			{
				if (Type == SelectionType.Cuboid)
				{
					int minX = Math.Min(Point1.X, Point2.X);
					int minY = Math.Min(Point1.Y, Point2.Y);
					int maxX = Math.Max(Point1.X, Point2.X);
					int maxY = Math.Max(Point1.Y, Point2.Y);
					return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
				}
				else // Sphere
				{
					return new Rectangle(
						Center.X - Radius,
						Center.Y - Radius,
						Radius * 2 + 1,
						Radius * 2 + 1
					);
				}
			}

			/// <summary>
			/// Checks if a point is contained within this selection.
			/// </summary>
			public bool Contains(int x, int y)
			{
				if (Type == SelectionType.Cuboid)
				{
					Rectangle bounds = GetBounds();
					return x >= bounds.X && x < bounds.X + bounds.Width &&
					       y >= bounds.Y && y < bounds.Y + bounds.Height;
				}
				else // Sphere
				{
					int dx = x - Center.X;
					int dy = y - Center.Y;
					return (dx * dx + dy * dy) <= (Radius * Radius);
				}
			}

			/// <summary>
			/// Gets the total number of tiles in this selection.
			/// </summary>
			public int GetTileCount()
			{
				if (!IsValid()) return 0;

				if (Type == SelectionType.Cuboid)
				{
					Rectangle bounds = GetBounds();
					return bounds.Width * bounds.Height;
				}
				else // Sphere
				{
					// Approximate count for sphere
					int count = 0;
					Rectangle bounds = GetBounds();
					for (int x = bounds.X; x < bounds.X + bounds.Width; x++)
					{
						for (int y = bounds.Y; y < bounds.Y + bounds.Height; y++)
						{
							if (Contains(x, y))
							{
								count++;
							}
						}
					}
					return count;
				}
			}

			/// <summary>
			/// Clears the selection.
			/// </summary>
			public void Clear()
			{
				HasPoint1 = false;
				HasPoint2 = false;
				HasCenter = false;
				HasRadius = false;
				Point1 = Point.Zero;
				Point2 = Point.Zero;
				Center = Point.Zero;
				Radius = 0;
			}
		}

		// Per-player selections (keyed by player whoAmI)
		private static Dictionary<int, Selection> _selections = new Dictionary<int, Selection>();

		// Particle display timer
		private int _particleTimer = 0;

		/// <summary>
		/// Gets or creates a selection for a player.
		/// </summary>
		public static Selection GetSelection(int playerWhoAmI)
		{
			if (!_selections.ContainsKey(playerWhoAmI))
			{
				_selections[playerWhoAmI] = new Selection();
			}
			return _selections[playerWhoAmI];
		}

		/// <summary>
		/// Sets the first corner point for a player's selection.
		/// </summary>
		public static void SetPos1(int playerWhoAmI, Point position)
		{
			Selection sel = GetSelection(playerWhoAmI);
			sel.Point1 = position;
			sel.HasPoint1 = true;
		}

		/// <summary>
		/// Sets the second corner point for a player's selection.
		/// </summary>
		public static void SetPos2(int playerWhoAmI, Point position)
		{
			Selection sel = GetSelection(playerWhoAmI);
			sel.Point2 = position;
			sel.HasPoint2 = true;
		}

		/// <summary>
		/// Sets the center point for a sphere selection.
		/// </summary>
		public static void SetCenter(int playerWhoAmI, Point position)
		{
			Selection sel = GetSelection(playerWhoAmI);
			sel.Center = position;
			sel.HasCenter = true;
		}

		/// <summary>
		/// Sets the radius for a sphere selection.
		/// </summary>
		public static void SetRadius(int playerWhoAmI, int radius)
		{
			Selection sel = GetSelection(playerWhoAmI);
			sel.Radius = radius;
			sel.HasRadius = true;
		}

		/// <summary>
		/// Sets the selection type for a player.
		/// </summary>
		public static void SetSelectionType(int playerWhoAmI, SelectionType type)
		{
			Selection sel = GetSelection(playerWhoAmI);
			sel.Type = type;
		}

		/// <summary>
		/// Clears a player's selection.
		/// </summary>
		public static void ClearSelection(int playerWhoAmI)
		{
			if (_selections.ContainsKey(playerWhoAmI))
			{
				_selections[playerWhoAmI].Clear();
			}
		}

		/// <summary>
		/// Updates selection visual feedback (dust particles).
		/// </summary>
		public override void PostUpdateEverything()
		{
			if (Main.netMode == NetmodeID.Server) return;

			_particleTimer++;
			if (_particleTimer < 10) return; // Update every 10 ticks
			_particleTimer = 0;

			// Get local player's selection
			Player localPlayer = Main.LocalPlayer;
			if (localPlayer == null || !localPlayer.active) return;

			Selection sel = GetSelection(localPlayer.whoAmI);
			if (!sel.IsValid()) return;

			// Check config for particle display
			var config = ModContent.GetInstance<Config.BuilderOpsConfig>();
			if (config != null && !config.EnableSelectionParticles) return;

			// Display particles based on selection type
			if (sel.Type == SelectionType.Cuboid && sel.HasPoint1 && sel.HasPoint2)
			{
				// Show corners and edges
				ShowCuboidParticles(sel);
			}
			else if (sel.Type == SelectionType.Sphere && sel.HasCenter && sel.HasRadius)
			{
				// Show sphere outline
				ShowSphereParticles(sel);
			}
		}

		/// <summary>
		/// Displays dust particles for cuboid selection corners.
		/// </summary>
		private void ShowCuboidParticles(Selection sel)
		{
			// Show the 4 corners
			Point[] corners = new Point[]
			{
				sel.Point1,
				sel.Point2,
				new Point(sel.Point1.X, sel.Point2.Y),
				new Point(sel.Point2.X, sel.Point1.Y)
			};

			foreach (Point corner in corners)
			{
				Vector2 worldPos = new Vector2(corner.X * 16 + 8, corner.Y * 16 + 8);
				Dust.NewDust(worldPos, 0, 0, DustID.Electric, 0, 0, 100, Color.Cyan, 1.5f);
			}
		}

		/// <summary>
		/// Displays dust particles for sphere selection outline.
		/// </summary>
		private void ShowSphereParticles(Selection sel)
		{
			// Show particles around the circumference
			int segments = Math.Min(32, sel.Radius * 4); // More segments for larger spheres
			for (int i = 0; i < segments; i++)
			{
				float angle = (float)(i * 2 * Math.PI / segments);
				int x = sel.Center.X + (int)(Math.Cos(angle) * sel.Radius);
				int y = sel.Center.Y + (int)(Math.Sin(angle) * sel.Radius);
				Vector2 worldPos = new Vector2(x * 16 + 8, y * 16 + 8);
				Dust.NewDust(worldPos, 0, 0, DustID.Electric, 0, 0, 100, Color.Yellow, 1.5f);
			}
		}

		/// <summary>
		/// Clears all selections on unload.
		/// </summary>
		public override void Unload()
		{
			_selections?.Clear();
			_selections = null;
		}
	}
}

