// https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents
// https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_mod_loader_1_1_mod_system.html
// https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_mod_loader_1_1_keybind_loader.html
// https://docs.tmodloader.net/docs/1.4-stable/class_terraria_1_1_mod_loader_1_1_mod_packet.html

/* Docs
 * override void PostUpdatePlayers() — runs every update after players update; good place for client input checks. Not called on dedicated servers when guarded.
 * Main.dedServ — true on dedicated server; don’t run client input here.
 * BuilderOps.PlaceTestKey.JustPressed — query the registered mod keybind (already set up in BuilderOps.cs).
 * Pixel→tile: tile = (int)(pixel / 16f) (Terraria uses 16 px per tile).
 * Single-player placement: WorldGen.PlaceTile(x, y, TileID.Stone, mute: true, forced: true); feedback via Main.NewText(...).
 * Multiplayer request: global::BuilderOps.BuilderOps.SendPlaceStonePacket(x, y) calls into the mod’s packet helper; server handles HandlePacket(...) and syncs with NetMessage.SendTileSquare.
 */

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BuilderOps.Systems
{
    /// <summary>
    /// Handles client-side BuilderOps input and dispatches local or networked placement requests.
    /// </summary>
    public class BuilderOpsSystem : ModSystem
    {
        /// <summary>
        /// Attempts to place the BuilderOps test tile for the provided player context.
        /// </summary>
        /// <param name="player">The player requesting placement.</param>
        internal static void TryHandleTestPlacement(Player player)
        {
            if (Main.dedServ || player == null || !player.active)
            {
                return;
            }

            int targetTileX = (int)(player.Center.X / 16f) + 3;
            int targetTileY = (int)(player.Center.Y / 16f) + 3;

            // TODO: Queue up placements for batching rather than firing immediately.
            // TODO: Enforce per-tick budgets to avoid overwhelming the server.

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                bool placed = WorldGen.PlaceTile(targetTileX, targetTileY, TileID.Stone, mute: true, forced: true);
                if (placed)
                {
                    Main.NewText($"[BuilderOps] Placed Stone at {targetTileX},{targetTileY}", Color.LightGreen);
                }
                else
                {
                    Main.NewText($"[BuilderOps] Failed to place Stone at {targetTileX},{targetTileY}", Color.IndianRed);
                }

                return;
            }

            global::BuilderOps.BuilderOps.SendPlaceStonePacket(targetTileX, targetTileY);

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                Main.NewText($"[BuilderOps] Requested Stone at {targetTileX},{targetTileY}", Color.LightGray);
            }
        }
    }
}
