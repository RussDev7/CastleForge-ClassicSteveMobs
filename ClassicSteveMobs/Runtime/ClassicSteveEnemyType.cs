/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.
*/

using DNA.CastleMinerZ.AI;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Enemy type wrapper for the classic Steve test mob.
    ///
    /// This still derives from <see cref="ZombieEnemyType"/> so CMZ keeps the normal
    /// zombie health, damage, hit, death, collision, and network behavior. The key
    /// difference is that its chase/restart state is replaced with a small random
    /// wander state so the mob runs around like the old rd-132328 test mobs instead
    /// of pathing directly toward the local player.
    /// </summary>
    internal sealed class ClassicSteveEnemyType : ZombieEnemyType
    {
        #region Fields

        private readonly IFSMState<BaseZombie> _wanderState = new ClassicSteveWanderState();

        #endregion

        #region Construction

        public ClassicSteveEnemyType()
            : base(
                ClassicSteveRegistry.ClassicSteveEnemyType,
                EnemyType.ModelNameEnum.ZOMBIE,
                EnemyType.TextureNameEnum.ZOMBIE_0,
                EnemyType.FoundInEnum.ABOVEGROUND,
                maxDigHardness: 2,
                digMultiplier: 0.1f)
        {
        }
        #endregion

        #region AI State Overrides

        /// <summary>
        /// Replaces CMZ's normal target-following zombie chase with random wandering.
        /// </summary>
        public override IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
        {
            return _wanderState;
        }

        /// <summary>
        /// Hit reactions and other temporary states restart back into wandering.
        /// </summary>
        public override IFSMState<BaseZombie> GetRestartState(BaseZombie entity)
        {
            return _wanderState;
        }

        /// <summary>
        /// Reports the highest speed this enemy can normally request.
        /// </summary>
        public override float GetMaxSpeed()
        {
            return ClassicSteveSettings.SlowSpeed + ClassicSteveSettings.RandomSlowSpeed;
        }
        #endregion
    }
}
