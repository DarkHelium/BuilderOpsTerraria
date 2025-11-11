using Terraria;
using Terraria.ModLoader;

namespace BuilderOps.Players
{
	/// <summary>
	/// Manages god mode state for players (invincibility, flight, instant mining).
	/// </summary>
	public class GodModePlayer : ModPlayer
	{
		private const int InfiniteFlightTime = 999999;
		private const int DefaultWingType = 1; // Basic wings so flight logic engages even without accessories
		private const int DefaultRocketBootsTier = 3; // Spectre boots tier keeps the rocket-boost flight path active

		public bool GodModeActive { get; set; } = false;

		public override void ResetEffects()
		{
			if (GodModeActive)
			{
				// Grant immunity to damage
				Player.immune = true;
				Player.immuneTime = 60;
				Player.immuneNoBlink = true;

				// Instant block breaking
				Player.pickSpeed = 0.01f; // Near-instant mining
			}
		}

		public override bool CanBeHitByNPC(NPC npc, ref int cooldownSlot)
		{
			// Prevent damage from NPCs
			return !GodModeActive;
		}

		public override bool CanBeHitByProjectile(Projectile proj)
		{
			// Prevent damage from projectiles
			return !GodModeActive;
		}

		public override void PreUpdate()
		{
			if (GodModeActive)
			{
				// Prevent drowning
				Player.breath = Player.breathMax;
				Player.lavaTime = Player.lavaMax;

				// Prevent debuffs
				Player.buffImmune[20] = true; // Poisoned
				Player.buffImmune[21] = true; // Darkness
				Player.buffImmune[22] = true; // Cursed
				Player.buffImmune[23] = true; // On Fire!
				Player.buffImmune[24] = true; // Bleeding
				Player.buffImmune[30] = true; // Confused
				Player.buffImmune[31] = true; // Slow
				Player.buffImmune[32] = true; // Weak
				Player.buffImmune[33] = true; // Silenced
				Player.buffImmune[35] = true; // Broken Armor
				Player.buffImmune[36] = true; // Horror
				Player.buffImmune[44] = true; // Stoned
				Player.buffImmune[46] = true; // Chilled
				Player.buffImmune[47] = true; // Frozen
				Player.buffImmune[67] = true; // Burning
				Player.buffImmune[68] = true; // Suffocation
				Player.buffImmune[69] = true; // Ichor
				Player.buffImmune[70] = true; // Venom

				EnsureInfiniteFlight();
			}
		}

		private void EnsureInfiniteFlight()
		{
			// Top off all flight timers so holding jump gives indefinite flight
			Player.wingTimeMax = InfiniteFlightTime;
			Player.wingTime = InfiniteFlightTime;
			Player.rocketTimeMax = InfiniteFlightTime;
			Player.rocketTime = InfiniteFlightTime;
			Player.noFallDmg = true;

			// Guarantee the player actually has something that can use those timers
			if (Player.wings <= 0)
			{
				Player.wings = DefaultWingType;
			}

			if (Player.wingsLogic <= 0)
			{
				Player.wingsLogic = DefaultWingType;
			}

			if (Player.rocketBoots < DefaultRocketBootsTier)
			{
				Player.rocketBoots = DefaultRocketBootsTier;
			}
		}
	}
}
