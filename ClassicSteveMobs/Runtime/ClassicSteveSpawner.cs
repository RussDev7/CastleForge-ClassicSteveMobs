/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.
*/

using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ;
using System;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Small helper for command-based test spawning.
    /// </summary>
    internal static class ClassicSteveSpawner
    {
        #region Public API

        /// <summary>
        /// Spawns one or more classic Steve mobs near the local player.
        /// Uses EnemyManager.SpawnEnemy so CMZ still owns networking, IDs, and enemy lists.
        /// </summary>
        public static int SpawnNearLocalPlayer(int amount)
        {
            ClassicSteveRegistry.EnsureRegistered();

            var game = CastleMinerZGame.Instance;
            var player = game?.LocalPlayer;
            var manager = EnemyManager.Instance;

            if (game == null || player == null || manager == null)
                return 0;

            int spawned = 0;
            amount = Math.Max(1, Math.Min(amount, 1000));

            for (int i = 0; i < amount; i++)
            {
                Vector3 pos = PickSpawnPosition(
                    player.WorldPosition,
                    player.LocalToWorld.Forward,
                    i,
                    amount);

                manager.SpawnEnemy(pos, ClassicSteveRegistry.ClassicSteveEnemyType, Vector3.Zero, 0, null);
                spawned++;
            }

            return spawned;
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Picks a simple in-front-of-player test position and snaps it to terrain if possible.
        /// </summary>
        /// <summary>
        /// Picks a square-ish grid spawn position in front of the player and snaps it to terrain if possible.
        /// </summary>
        private static Vector3 PickSpawnPosition(Vector3 playerPos, Vector3 forward, int index, int totalAmount)
        {
            if (forward.LengthSquared() < 0.001f)
                forward = Vector3.Forward;

            forward.Y = 0f;
            if (forward.LengthSquared() < 0.001f)
                forward = Vector3.Forward;

            forward.Normalize();

            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            if (right.LengthSquared() < 0.001f)
                right = Vector3.Right;

            right.Normalize();

            // Makes the spawn layout square-ish.
            // Example:
            // 20  -> 5x4
            // 100 -> 10x10
            // 200 -> 15x14
            int columns = (int)Math.Ceiling(Math.Sqrt(totalAmount));
            // int rows = (int)Math.Ceiling(totalAmount / (float)columns);

            int col = index % columns;
            int row = index / columns;

            const float spacing = 1.75f;

            // Center the grid left/right.
            float sideOffset = (col - ((columns - 1) * 0.5f)) * spacing;

            // Keep the grid in front of the player.
            float forwardOffset = row * spacing;

            Vector3 pos =
                playerPos +
                forward * (ClassicSteveSettings.CommandSpawnDistance + forwardOffset) +
                right * sideOffset;

            pos.Y += 1f;

            try
            {
                if (BlockTerrain.Instance != null && BlockTerrain.Instance.RegionIsLoaded(pos))
                    pos = BlockTerrain.Instance.FindTopmostGroundLocation(pos);
            }
            catch
            {
                // Command spawning should still work even if terrain snapping fails.
            }

            return pos;
        }
        #endregion
    }
}
