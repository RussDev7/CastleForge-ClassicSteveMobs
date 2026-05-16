/*
SPDX-License-Identifier: GPL-3.0-or-later
Copyright (c) 2025 RussDev7
This file is part of https://github.com/RussDev7/CastleForge - see LICENSE for details.

Test-only note:
This renderer expects the original 64x32 rd-132328 char.png layout.
*/

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using DNA.CastleMinerZ.AI;
using System.IO;
using System;

using static ModLoader.LogSystem;

namespace ClassicSteveMobs
{
    /// <summary>
    /// Immediate-mode style XNA renderer for the old Minecraft rd-132328 six-cube mob.
    ///
    /// The model is intentionally not an XNB/skinned model. It draws textured cuboids directly
    /// so this test mod can use the classic char.png and sine-wave limb animation.
    /// </summary>
    internal static class ClassicSteveRenderer
    {
        #region Fields

        private const float TextureWidth  = 64f;
        private const float TextureHeight = 32f;
        private const float ModelBottomY  = 24f;

        private static BasicEffect _effect;
        private static Texture2D   _texture;
        private static bool        _loadAttempted;

        #endregion

        #region Public API

        /// <summary>
        /// Disposes cached XNA resources when the mod shuts down.
        /// </summary>
        public static void Dispose()
        {
            _effect?.Dispose();
            _effect = null;
            _texture?.Dispose();
            _texture = null;

            _loadAttempted = false;
        }

        /// <summary>
        /// Draws the classic six-cube body for a TREASURE_ZOMBIE test enemy.
        /// </summary>
        public static void Draw(BaseZombie zombie, GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
        {
            if (zombie == null || device == null)
                return;

            EnsureResources(device);
            if (_effect == null || _texture == null)
                return;

            var vertices = new List<VertexPositionTexture>(24 * 6);
            var indices  = new List<short>(36 * 6);

            float time = (float)(gameTime.TotalGameTime.TotalSeconds * ClassicSteveSettings.AnimationSpeed + zombie.EnemyID * 0.137f);

            // rd-132328 animation values.
            float headYRot     = (float)Math.Sin(time * 0.83f);
            float headXRot     = (float)Math.Sin(time) * 0.8f;
            float rightArmXRot = (float)Math.Sin(time * 0.6662f + Math.PI) * 2.0f;
            float rightArmZRot = (float)(Math.Sin(time * 0.2312f) + 1.0f);
            float leftArmXRot  = (float)Math.Sin(time * 0.6662f) * 2.0f;
            float leftArmZRot  = (float)(Math.Sin(time * 0.2812f) - 1.0f);
            float rightLegXRot = (float)Math.Sin(time * 0.6662f) * 1.4f;
            float leftLegXRot  = (float)Math.Sin(time * 0.6662f + Math.PI) * 1.4f;

            AddBox(vertices, indices, 0,  0,  -4, -8, -4, 8,  8, 8,  Vector3.Zero,             headXRot,     headYRot, 0f);          // Head.
            AddBox(vertices, indices, 16, 16, -4,  0, -2, 8, 12, 4,  Vector3.Zero,             0f,           0f,       0f);          // Body.
            AddBox(vertices, indices, 40, 16, -3, -2, -2, 4, 12, 4,  new Vector3(-5, 2, 0),    rightArmXRot, 0f,       rightArmZRot); // Right arm.
            AddBox(vertices, indices, 40, 16, -1, -2, -2, 4, 12, 4,  new Vector3( 5, 2, 0),    leftArmXRot,  0f,       leftArmZRot);  // Left arm.
            AddBox(vertices, indices, 0,  16, -2,  0, -2, 4, 12, 4,  new Vector3(-2, 12, 0),   rightLegXRot, 0f,       0f);          // Right leg.
            AddBox(vertices, indices, 0,  16, -2,  0, -2, 4, 12, 4,  new Vector3( 2, 12, 0),   leftLegXRot,  0f,       0f);          // Left leg.

            if (vertices.Count == 0 || indices.Count == 0)
                return;

            BlendState oldBlend = device.BlendState;
            DepthStencilState oldDepth = device.DepthStencilState;
            RasterizerState oldRasterizer = device.RasterizerState;
            SamplerState oldSampler = device.SamplerStates[0];

            try
            {
                device.BlendState = BlendState.AlphaBlend;
                device.DepthStencilState = DepthStencilState.Default;
                device.RasterizerState = RasterizerState.CullNone;
                device.SamplerStates[0] = SamplerState.PointClamp;

                _effect.World = Matrix.CreateRotationY(MathHelper.ToRadians(ClassicSteveSettings.YawOffsetDegrees)) * zombie.LocalToWorld;
                _effect.View = view;
                _effect.Projection = projection;
                _effect.Texture = _texture;
                _effect.TextureEnabled = true;
                _effect.VertexColorEnabled = false;
                _effect.LightingEnabled = false;
                _effect.DiffuseColor = Vector3.One;
                _effect.Alpha = 1f;

                foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        vertices.ToArray(),
                        0,
                        vertices.Count,
                        indices.ToArray(),
                        0,
                        indices.Count / 3);
                }
            }
            finally
            {
                device.BlendState = oldBlend;
                device.DepthStencilState = oldDepth;
                device.RasterizerState = oldRasterizer;
                device.SamplerStates[0] = oldSampler;
            }
        }
        #endregion

        #region Resource Loading

        /// <summary>
        /// Lazily loads the extracted char.png texture and creates the shared BasicEffect.
        /// </summary>
        private static void EnsureResources(GraphicsDevice device)
        {
            if (_effect == null)
                _effect = new BasicEffect(device);

            if (_texture != null || _loadAttempted)
                return;

            _loadAttempted = true;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "!Mods", "ClassicSteveMobs", "Textures", "char.png");
            try
            {
                if (!File.Exists(path))
                {
                    Log($"Missing texture: {path}.");
                    return;
                }

                using (FileStream stream = File.OpenRead(path))
                    _texture = Texture2D.FromStream(device, stream);

                Log("Loaded Textures\\char.png.");
            }
            catch (Exception ex)
            {
                Log($"Failed to load char.png: {ex.Message}.");
            }
        }
        #endregion

        #region Cube Building

        /// <summary>
        /// Port of rd-132328 Cube_addBox + Cube_render into XNA vertex data.
        /// </summary>
        private static void AddBox(
            List<VertexPositionTexture> vertices,
            List<short> indices,
            int texOffX,
            int texOffY,
            float ox,
            float oy,
            float oz,
            int w,
            int h,
            int d,
            Vector3 pivot,
            float xRot,
            float yRot,
            float zRot)
        {
            float x = ox + w;
            float y = oy + h;
            float z = oz + d;

            Vector3 b1 = new Vector3(ox, oy, oz);
            Vector3 b2 = new Vector3(x,  oy, oz);
            Vector3 b3 = new Vector3(ox, oy, z);
            Vector3 b4 = new Vector3(x,  oy, z);

            Vector3 t1 = new Vector3(x,  y, z);
            Vector3 t2 = new Vector3(ox, y, z);
            Vector3 t3 = new Vector3(x,  y, oz);
            Vector3 t4 = new Vector3(ox, y, oz);

            Matrix rotation = Matrix.CreateRotationX(xRot) * Matrix.CreateRotationY(yRot) * Matrix.CreateRotationZ(zRot);

            // right
            AddQuad(vertices, indices, rotation, pivot, b4, b2, t3, t1,
                texOffX + d + w,     texOffY + d,
                texOffX + d + w + d, texOffY + d + h);

            // left
            AddQuad(vertices, indices, rotation, pivot, b1, b3, t2, t4,
                texOffX,             texOffY + d,
                texOffX + d,         texOffY + d + h);

            // bottom
            AddQuad(vertices, indices, rotation, pivot, b4, b3, b1, b2,
                texOffX + d,         texOffY,
                texOffX + d + w,     texOffY + d);

            // top
            AddQuad(vertices, indices, rotation, pivot, t3, t4, t2, t1,
                texOffX + d + w,     texOffY,
                texOffX + d + w + w, texOffY + d);

            // front
            AddQuad(vertices, indices, rotation, pivot, b2, b1, t4, t3,
                texOffX + d,         texOffY + d,
                texOffX + d + w,     texOffY + d + h);

            // back
            AddQuad(vertices, indices, rotation, pivot, b3, b4, t1, t2,
                texOffX + d + w + d,     texOffY + d,
                texOffX + d + w + d + w, texOffY + d + h);
        }

        /// <summary>
        /// Adds one textured quad using the same UV remap/order as the original OpenGL renderer.
        /// </summary>
        private static void AddQuad(
            List<VertexPositionTexture> vertices,
            List<short> indices,
            Matrix rotation,
            Vector3 pivot,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            int minU,
            int minV,
            int maxU,
            int maxV)
        {
            int start = vertices.Count;

            // Original Polygon_init_uv remaps a,b,c,d to:
            // v0=(a,maxU,minV), v1=(b,minU,minV), v2=(c,minU,maxV), v3=(d,maxU,maxV),
            // then renders v3,v2,v1,v0. We keep that order here.
            AddVertex(vertices, rotation, pivot, d, maxU, maxV);
            AddVertex(vertices, rotation, pivot, c, minU, maxV);
            AddVertex(vertices, rotation, pivot, b, minU, minV);
            AddVertex(vertices, rotation, pivot, a, maxU, minV);

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 0));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 3));
        }

        /// <summary>
        /// Converts original Minecraft model coordinates into CMZ/XNA local coordinates.
        /// </summary>
        private static void AddVertex(List<VertexPositionTexture> vertices, Matrix rotation, Vector3 pivot, Vector3 pos, int u, int v)
        {
            Vector3 modelPos = Vector3.Transform(pos, rotation) + pivot;

            // Original model Y goes down. CMZ/XNA Y goes up. Feet are at model Y=24.
            float scale = ClassicSteveSettings.ModelScale;
            Vector3 xnaPos = new Vector3(
                modelPos.X * scale,
                (ModelBottomY - modelPos.Y) * scale,
                modelPos.Z * scale);

            vertices.Add(new VertexPositionTexture(
                xnaPos,
                new Vector2(u / TextureWidth, v / TextureHeight)));
        }
        #endregion
    }
}
