using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
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
		private const uint DoubleTapWindowTicks = 20;
		private const float FlightHorizontalAcceleration = 0.8f;
		private const float FlightVerticalAcceleration = 0.7f;
		private const float FlightHorizontalDamp = 0.85f;
		private const float FlightVerticalDamp = 0.85f;
		private const float MaxHorizontalSpeed = 12f;
		private const float MaxVerticalSpeed = 9f;

		private bool godFlightActive;
		private bool jumpWasPressed;
		private bool awaitingSecondJumpTap;
		private uint lastJumpTapTick;

		public bool GodModeActive { get; set; } = false;
		public bool GodFlightActive => godFlightActive;

		public override void ResetEffects()
		{
			if (!GodModeActive && godFlightActive)
			{
				ToggleGodFlight(false, announce: false);
				ResetJumpTapTracking();
			}

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

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (Main.dedServ)
			{
				return;
			}

			bool jumpPressed = triggersSet.Jump;

			if (GodModeActive && jumpPressed && !jumpWasPressed)
			{
				HandleJumpJustPressed();
			}

			jumpWasPressed = jumpPressed;

			if (!GodModeActive && godFlightActive)
			{
				ToggleGodFlight(false, announce: false);
				ResetJumpTapTracking();
			}
		}

		public override void PreUpdateMovement()
		{
			if (!GodModeActive || !godFlightActive)
			{
				return;
			}

			ApplyGodFlightMovement();
		}

		public override void OnRespawn()
		{
			ToggleGodFlight(false, announce: false);
			ResetJumpTapTracking();
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

		private void HandleJumpJustPressed()
		{
			uint currentTick = Main.GameUpdateCount;

			if (awaitingSecondJumpTap)
			{
				uint diff = TickDifference(currentTick, lastJumpTapTick);
				if (diff <= DoubleTapWindowTicks)
				{
					ToggleGodFlight(!godFlightActive);
					awaitingSecondJumpTap = false;
					return;
				}
			}

			awaitingSecondJumpTap = true;
			lastJumpTapTick = currentTick;
		}

		private void ToggleGodFlight(bool enable, bool announce = true)
		{
			if (godFlightActive == enable)
			{
				return;
			}

			godFlightActive = enable;

			if (!enable)
			{
				Player.gravity = Player.defaultGravity;
			}
			else
			{
				Player.velocity *= 0.5f;
				Player.velocity.Y = 0f;
				Player.gravity = 0f;
			}

			if (!Main.dedServ && Player.whoAmI == Main.myPlayer && announce)
			{
				string msg = enable
					? "[BuilderOps] God flight enabled (double tap jump to disable)"
					: "[BuilderOps] God flight disabled";
				Color color = enable ? Color.LightSkyBlue : Color.Gray;
				Main.NewText(msg, color);
			}
		}

		private static uint TickDifference(uint current, uint previous)
		{
			return current >= previous
				? current - previous
				: uint.MaxValue - previous + current + 1u;
		}

		private void ResetJumpTapTracking()
		{
			awaitingSecondJumpTap = false;
			lastJumpTapTick = 0u;
		}

		private void ApplyGodFlightMovement()
		{
			Player.gravity = 0f;
			Player.noFallDmg = true;
			Player.fallStart = (int)(Player.position.Y / 16f);

			if (!Player.controlLeft && !Player.controlRight)
			{
				Player.velocity.X = 0f;
			}
			else
			{
				if (Player.controlLeft)
				{
					Player.velocity.X -= FlightHorizontalAcceleration;
				}

				if (Player.controlRight)
				{
					Player.velocity.X += FlightHorizontalAcceleration;
				}
			}

			if (!Player.controlUp && !Player.controlDown)
			{
				Player.velocity.Y = 0f;
			}
			else
			{
				if (Player.controlUp)
				{
					Player.velocity.Y -= FlightVerticalAcceleration;
				}

				if (Player.controlDown)
				{
					Player.velocity.Y += FlightVerticalAcceleration;
				}
			}

			Player.velocity.X = MathHelper.Clamp(Player.velocity.X, -MaxHorizontalSpeed, MaxHorizontalSpeed);
			Player.velocity.Y = MathHelper.Clamp(Player.velocity.Y, -MaxVerticalSpeed, MaxVerticalSpeed);
		}
	}
}
