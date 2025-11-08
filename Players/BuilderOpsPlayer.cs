using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

using BuilderOps.Systems;

namespace BuilderOps.Players
{
    /// <summary>
    /// Listens for BuilderOps keybinds and forwards them to the placement system.
    /// </summary>
    public sealed class BuilderOpsPlayer : ModPlayer
    {
        /// <summary>
        /// Called every frame to handle player-specific input triggers.
        /// </summary>
        /// <param name="triggersSet">The current trigger state snapshot.</param>
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Main.dedServ)
            {
                return;
            }

            ModKeybind key = global::BuilderOps.BuilderOps.PlaceTestKey;
            if (key == null || !key.JustPressed)
            {
                return;
            }

            BuilderOpsSystem.TryHandleTestPlacement(Player);
        }
    }
}

