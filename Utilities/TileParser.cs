using System.Reflection;
using Terraria.ID;

namespace BuilderOps.Utilities
{
	/// <summary>
	/// Helper class for parsing tile and wall names/IDs.
	/// </summary>
	public static class TileParser
	{
		/// <summary>
		/// Parses a tile name or numeric ID into a tile type.
		/// </summary>
		public static ushort ParseTile(string input)
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

		/// <summary>
		/// Parses a wall name or numeric ID into a wall type.
		/// </summary>
		public static ushort ParseWall(string input)
		{
			if (ushort.TryParse(input, out ushort id))
			{
				return id;
			}

			FieldInfo field = typeof(WallID).GetField(
				input,
				BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase
			);

			if (field != null && field.FieldType == typeof(ushort))
			{
				return (ushort)field.GetValue(null);
			}

			return WallID.Stone;
		}

		/// <summary>
		/// Gets the name of a tile type from its ID.
		/// </summary>
		public static string GetTileName(ushort tileId)
		{
			var fields = typeof(TileID).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(ushort) && (ushort)field.GetValue(null) == tileId)
				{
					return field.Name;
				}
			}
			return "Unknown";
		}

		/// <summary>
		/// Gets the name of a wall type from its ID.
		/// </summary>
		public static string GetWallName(ushort wallId)
		{
			var fields = typeof(WallID).GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach (var field in fields)
			{
				if (field.FieldType == typeof(ushort) && (ushort)field.GetValue(null) == wallId)
				{
					return field.Name;
				}
			}
			return "Unknown";
		}
	}
}

