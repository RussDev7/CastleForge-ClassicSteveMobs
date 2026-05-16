/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.
*/

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using DNA.CastleMinerZ.AI;
using System;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Simple rd-132328-inspired random movement state for the blocky Steve test mob.
    ///
    /// Vanilla CMZ zombies use <see cref="ZombieChase"/> and constantly steer toward
    /// <see cref="BaseZombie.Target"/>. This state intentionally avoids target seeking:
    /// each entity keeps a heading, occasionally picks a new one, turns away from walls,
    /// and sometimes hops while preserving CMZ's normal physics/collision resolution.
    /// </summary>
    internal sealed class ClassicSteveWanderState : EnemyBaseState
    {
        #region Per-Entity State

        /// <summary>
        /// Stored per zombie because EnemyType state objects are shared by many enemies.
        /// </summary>
        private sealed class WanderData
        {
            public float Heading;
            public float Speed;
            public float TurnTimer;
            public float JumpCooldown;
        }

        private readonly ConditionalWeakTable<BaseZombie, WanderData> _wanderData =
            new ConditionalWeakTable<BaseZombie, WanderData>();

        #endregion

        #region State Lifecycle

        public override bool IsRestartable()
        {
            return true;
        }

        public override void Enter(BaseZombie entity)
        {
            entity.IsBlocking = true;
            entity.IsHittable = true;
            entity.ResetFrustration();

            WanderData data = GetData(entity);
            PickNewHeading(entity, data, forceBigTurn: true);
            data.JumpCooldown = 0.25f;

            // The custom renderer draws the block model, but keeping a vanilla animation
            // clip active avoids upsetting any base skinned-model update assumptions.
            TryPlayMoveAnimation(entity);
        }

        public override void Update(BaseZombie entity, float dt)
        {
            if (dt <= 0f)
                return;

            if (dt > 0.1f)
                dt = 0.1f;

            WanderData data = GetData(entity);
            data.TurnTimer -= dt;
            data.JumpCooldown -= dt;

            if (data.TurnTimer <= 0f)
                PickNewHeading(entity, data, forceBigTurn: false);

            Vector3 velocity = entity.PlayerPhysics.WorldVelocity;

            if (entity.OnGround && entity.TouchingWall)
            {
                // Bounce away from the wall and hop, close to the goofy early-test-mob feel.
                data.Heading += MathHelper.Pi * 0.75f + RandomSigned(entity, MathHelper.PiOver2);
                data.TurnTimer = 0.25f + RandomRange(entity, 0.25f, 0.75f);

                if (data.JumpCooldown <= 0f)
                {
                    velocity.Y += ClassicSteveSettings.WanderJumpSpeed;
                    data.JumpCooldown = ClassicSteveSettings.WanderJumpCooldown;
                }
            }
            else if (entity.OnGround && ClassicSteveSettings.WanderRandomJumpChance > 0f)
            {
                // Chance is configured per second, then scaled by dt.
                double roll = entity.Rnd.NextDouble();
                if (roll < ClassicSteveSettings.WanderRandomJumpChance * dt && data.JumpCooldown <= 0f)
                {
                    velocity.Y += ClassicSteveSettings.WanderJumpSpeed;
                    data.JumpCooldown = ClassicSteveSettings.WanderJumpCooldown;
                }
            }

            float speed = data.Speed;
            if (!entity.OnGround)
                speed *= 0.5f;

            velocity.X = (float)Math.Cos(data.Heading) * speed;
            velocity.Z = (float)Math.Sin(data.Heading) * speed;
            entity.PlayerPhysics.WorldVelocity = velocity;

            FaceVelocity(entity, velocity);
            DespawnIfTooFar(entity);
        }

        public override void Exit(BaseZombie entity)
        {
            // Keep vanilla temporary states free to zero/reduce velocity as needed.
        }
        #endregion

        #region Helpers

        private WanderData GetData(BaseZombie entity)
        {
            return _wanderData.GetValue(entity, _ => new WanderData());
        }

        private void PickNewHeading(BaseZombie entity, WanderData data, bool forceBigTurn)
        {
            float min = ClassicSteveSettings.WanderTurnIntervalMin;
            float max = ClassicSteveSettings.WanderTurnIntervalMax;
            if (max < min)
                max = min;

            data.TurnTimer = RandomRange(entity, min, max);

            if (forceBigTurn)
                data.Heading = RandomRange(entity, 0f, MathHelper.TwoPi);
            else
                data.Heading += RandomSigned(entity, ClassicSteveSettings.WanderTurnAmountRadians);

            data.Speed = RandomRange(
                entity,
                ClassicSteveSettings.SlowSpeed,
                ClassicSteveSettings.SlowSpeed + ClassicSteveSettings.RandomSlowSpeed);
        }

        private static void TryPlayMoveAnimation(BaseZombie entity)
        {
            try
            {
                entity.CurrentPlayer = entity.PlayClip("walk", true, TimeSpan.FromSeconds(0.25));
                if (entity.CurrentPlayer != null)
                    entity.CurrentPlayer.Speed = 1f;
            }
            catch
            {
                // Rendering is custom; animation failure should not break this test mob.
            }
        }

        private void FaceVelocity(BaseZombie entity, Vector3 velocity)
        {
            Vector3 flat = velocity;
            flat.Y = 0f;

            if (flat.LengthSquared() <= 0.001f)
                return;

            float heading = (float)Math.Atan2(-flat.Z, flat.X) + MathHelper.PiOver2;
            entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(MakeHeading(entity, heading), 0f, 0f);
        }

        private static void DespawnIfTooFar(BaseZombie entity)
        {
            if (entity.Target == null || !entity.Target.IsLocal)
                return;

            Vector3 delta = entity.Target.WorldPosition - entity.WorldPosition;
            delta.Y = 0f;

            float limit = Math.Max(16f, entity.PlayerDistanceLimit);
            if (delta.LengthSquared() > limit * limit)
                entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
        }

        private static float RandomRange(BaseZombie entity, float min, float max)
        {
            if (max <= min)
                return min;

            return min + (float)entity.Rnd.NextDouble() * (max - min);
        }

        private static float RandomSigned(BaseZombie entity, float amount)
        {
            if (amount <= 0f)
                return 0f;

            return ((float)entity.Rnd.NextDouble() * 2f - 1f) * amount;
        }
        #endregion
    }
}
