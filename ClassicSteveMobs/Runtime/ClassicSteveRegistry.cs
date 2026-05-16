/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.
*/

using DNA.CastleMinerZ.AI;
using System;

using static ModLoader.LogSystem;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Registers the unused TREASURE_ZOMBIE enum slot as the test classic Steve mob.
    ///
    /// Why TREASURE_ZOMBIE:
    /// - It already exists in CMZ's EnemyTypeEnum.
    /// - SpawnEnemyMessage serializes the enemy type as a byte.
    /// - Reusing an existing enum value avoids changing vanilla network serialization.
    /// </summary>
    internal static class ClassicSteveRegistry
    {
        #region Constants

        public const EnemyTypeEnum ClassicSteveEnemyType = EnemyTypeEnum.TREASURE_ZOMBIE;

        #endregion

        #region Public API

        /// <summary>
        /// Ensures EnemyType.Types is large enough and installs the test enemy type.
        /// Safe to call from startup, after EnemyType.Init(), or before command spawns.
        /// </summary>
        public static void EnsureRegistered()
        {
            try
            {
                if (EnemyType.Types == null)
                    EnemyType.Init();

                int requiredLength = (int)EnemyTypeEnum.COUNT;
                if (EnemyType.Types.Length < requiredLength)
                    Array.Resize(ref EnemyType.Types, requiredLength);

                var steveType = EnemyType.Types[(int)ClassicSteveEnemyType] as ZombieEnemyType;
                bool newlyRegistered = false;

                if (!(steveType is ClassicSteveEnemyType) || steveType.EType != ClassicSteveEnemyType)
                {
                    steveType = new ClassicSteveEnemyType();

                    EnemyType.Types[(int)ClassicSteveEnemyType] = steveType;
                    newlyRegistered = true;
                }

                // Keep it weak/early-game for testing. Re-apply every call so /stevemob reload works.
                steveType.StartingHealth        = ClassicSteveSettings.Health;
                steveType.SpawnRadius           = ClassicSteveSettings.SpawnRadius;
                steveType.StartingDistanceLimit = ClassicSteveSettings.DistanceLimit;
                steveType.BaseSlowSpeed         = ClassicSteveSettings.SlowSpeed;
                steveType.RandomSlowSpeed       = ClassicSteveSettings.RandomSlowSpeed;
                steveType.BaseFastSpeed         = ClassicSteveSettings.FastSpeed;
                steveType.HasRunFast            = ClassicSteveSettings.HasRunFast;
                steveType.ChanceOfBulletStrike  = 1f;

                if (newlyRegistered)
                    Log("Registered TREASURE_ZOMBIE as the classic Steve test mob with random wander AI.");
            }
            catch (Exception ex)
            {
                Log($"Failed to register classic Steve enemy type: {ex}.");
            }
        }

        /// <summary>
        /// Returns true when the given zombie is one of our test mobs.
        /// </summary>
        public static bool IsClassicSteve(BaseZombie zombie)
        {
            return zombie != null &&
                   zombie.EType != null &&
                   zombie.EType.EType == ClassicSteveEnemyType;
        }
        #endregion
    }
}
