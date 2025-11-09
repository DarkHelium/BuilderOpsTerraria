using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BuilderOps.Config
{
	/// <summary>
	/// Configuration for BuilderOps mod settings.
	/// </summary>
	public class BuilderOpsConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

	[Label("Tiles Per Tick Budget")]
	[Tooltip("Maximum number of tiles to place per game tick (higher = faster building, but may cause lag)")]
	[Range(1, 2000)]
	[DefaultValue(64)]
	[Slider]
	public int TilesPerTick { get; set; }

	[Label("Enable Selection Particles")]
	[Tooltip("Show visual dust particles around your selection boundaries")]
	[DefaultValue(true)]
	public bool EnableSelectionParticles { get; set; }

	[Label("Max Selection Size")]
	[Tooltip("Maximum number of tiles allowed in a selection (prevents lag from huge selections)")]
	[Range(100, 50000)]
	[DefaultValue(10000)]
	public int MaxSelectionSize { get; set; }

	public override void OnChanged()
	{
		// Update the placement queue system's budget when config changes
		Systems.PlacementQueueSystem.TilesPerTickBudget = TilesPerTick;
	}
	}
}

