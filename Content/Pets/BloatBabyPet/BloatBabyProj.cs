﻿using CalamityHunt.Common.Systems.Particles;
using CalamityHunt.Content.Bosses.Goozma;
using CalamityHunt.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Pets.BloatBabyPet
{
    public class BloatBabyProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;

            ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, 1)
                .WithOffset(-2, -22f)
                .WithCode(PreviewVisual);
        }

        public static void PreviewVisual(Projectile proj, bool walking)
        {
            proj.position.X += 10;
            proj.position.Y += (float)Math.Sin(Main.timeForVisualEffects % 220 / 220f * MathHelper.TwoPi) * 2f;
            proj.velocity = new Vector2(2f, -1f);
            proj.rotation = proj.velocity.ToRotation() + MathHelper.PiOver2;
            proj.scale = 1f;
            proj.localAI[0]++;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.EyeOfCthulhuPet);
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 targetPos = player.MountedCenter + new Vector2((-100 + (float)Math.Sin(Projectile.ai[0] * 0.005) * 60f) * player.direction, -30 + (float)Math.Sin(Projectile.ai[0] * 0.01) * 60f);

            Projectile.rotation = Utils.AngleLerp(Projectile.velocity.X * 0.05f, Projectile.velocity.ToRotation() + MathHelper.PiOver2, Math.Clamp(Projectile.velocity.Length() * 0.5f, 0f, 1f));
            crownRotation = -Projectile.velocity.X * 0.015f;

            if (Projectile.Distance(player.MountedCenter) > 1000)
                Projectile.Center = Vector2.Lerp(Projectile.Center, Projectile.Center + Projectile.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * (Projectile.Distance(targetPos) - 1000) * 0.2f, 0.5f);

            int waitTime = 600;
            if (player.velocity.Length() < 5f)
            {
                Projectile.ai[1]++;
                if (Projectile.ai[1] <= waitTime)
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * (Projectile.Distance(targetPos) - 2) * 0.1f, 0.02f);
            }
            else
            {
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(targetPos).SafeNormalize(Vector2.Zero) * (Projectile.Distance(targetPos) - 2) * 0.1f, 0.02f);
                Projectile.ai[1] = 0;
            }

            if (Projectile.ai[1] > waitTime)
            {
                Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero) * Projectile.Distance(Main.MouseWorld) * 0.00005f;
                Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero) * 0.0005f;
                Projectile.velocity *= 0.98f;
            }

            if (Projectile.velocity.X > 2f)
                Projectile.direction = 1;
            if (Projectile.velocity.X < -2f)
                Projectile.direction = -1;

            if (oldVels == null)
            {
                oldVels = new Vector2[10];
                for (int i = 0; i < oldVels.Length; i++)
                    oldVels[i] = Projectile.velocity;
            }
            for (int i = 9; i > 0; i--)
                oldVels[i] = Vector2.Lerp(oldVels[i], oldVels[i - 1] * 1.2f, 0.5f);
            oldVels[0] = Vector2.Lerp(oldVels[0], (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2(), 0.5f);

            Projectile.ai[0]++;
            Projectile.localAI[0]++;

            if (!player.dead && player.HasBuff<BloatBabyBuff>())
                Projectile.timeLeft = 2;

            if (Main.rand.NextBool(7))
            {
                Particle hue = Particle.NewParticle(Particle.ParticleType<HueLightDust>(), Projectile.Center + Main.rand.NextVector2Circular(30, 30), Projectile.velocity * 0.2f, Color.White, 1f);
                hue.data = Projectile.localAI[0];
            }

            Lighting.AddLight(Projectile.Center, new GradientColor(SlimeUtils.GoozColorArray, 0.2f, 0.2f).ValueAt(Projectile.localAI[0]).ToVector3() * 0.2f);
        }

        public float crownRotation;

        public override bool PreDraw(ref Color lightColor) => false;

        public override void PostDraw(Color lightColor)
        {
            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
            Asset<Texture2D> crown = ModContent.Request<Texture2D>(Texture + "Crown");
            Asset<Texture2D> glow = ModContent.Request<Texture2D>($"{nameof(CalamityHunt)}/Assets/Textures/Goozma/GlowSoft");
            Asset<Texture2D> ring = ModContent.Request<Texture2D>($"{nameof(CalamityHunt)}/Assets/Textures/Goozma/GlowRing");
            SpriteEffects flip = Projectile.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Color glowColor = new GradientColor(SlimeUtils.GoozColorArray, 0.2f, 0.2f).ValueAt(Projectile.localAI[0]);
            glowColor.A = 0;

            float auraSize = 0.9f + (float)Math.Sin(Projectile.localAI[0] * 0.03f) * 0.1f;
            Main.EntitySpriteDraw(glow.Value, Projectile.Center - Main.screenPosition, null, glowColor * 0.2f * auraSize, 0, glow.Size() * 0.5f, Projectile.scale * 5f * auraSize, 0, 0);
            Main.EntitySpriteDraw(ring.Value, Projectile.Center - Main.screenPosition, null, glowColor * 0.08f * auraSize, 0, ring.Size() * 0.5f, Projectile.scale * 2f * auraSize, 0, 0);

            DrawTentacles(lightColor, glowColor);

            Rectangle frame = texture.Frame(3, 1, 0, 0);
            Rectangle glowFrame = texture.Frame(3, 1, 1, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * new Vector2(0.5f, 0.33f), Projectile.scale, flip, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - Main.screenPosition, glowFrame, glowColor * 0.9f, Projectile.rotation, frame.Size() * new Vector2(0.5f, 0.33f), Projectile.scale, flip, 0);
            Main.EntitySpriteDraw(texture.Value, Projectile.Center - Main.screenPosition, glowFrame, glowColor * 0.5f, Projectile.rotation, frame.Size() * new Vector2(0.5f, 0.33f), Projectile.scale * 1.05f, flip, 0);
            Main.EntitySpriteDraw(crown.Value, Projectile.Center + new Vector2(0, -17).RotatedBy(crownRotation) * Projectile.scale - Main.screenPosition, null, Color.Lerp(lightColor, Color.White, 0.5f), crownRotation, crown.Size() * new Vector2(0.5f, 0.9f), Projectile.scale * 1.05f, flip, 0);
            Main.EntitySpriteDraw(crown.Value, Projectile.Center + new Vector2(0, -17).RotatedBy(crownRotation) * Projectile.scale - Main.screenPosition, null, glowColor * 0.3f, crownRotation, crown.Size() * new Vector2(0.5f, 0.9f), Projectile.scale * 1.15f, flip, 0);
            Main.EntitySpriteDraw(glow.Value, Projectile.Center - Main.screenPosition, null, glowColor * 0.2f, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale, 0, 0);
            Main.EntitySpriteDraw(glow.Value, Projectile.Center - Main.screenPosition, null, glowColor * 0.1f, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale * 3f, 0, 0);
        }

        public Vector2[] oldVels;

        public void DrawTentacles(Color lightColor, Color growColor)
        {
            Asset<Texture2D> tentacleTexture = ModContent.Request<Texture2D>(Texture + "Tentacle");
            Asset<Texture2D> glow = ModContent.Request<Texture2D>($"{nameof(CalamityHunt)}/Assets/Textures/Goozma/GlowSoft");

            if (oldVels == null)
            {
                oldVels = new Vector2[10];
                for (int i = 0; i < oldVels.Length; i++)
                    oldVels[i] = Projectile.velocity;
            }

            float tentaCount = 2;
            for (int j = 0; j < tentaCount; j++)
            {
                int dir = j > 0 ? 1 : -1;

                float rot = Projectile.rotation + MathHelper.PiOver2;
                Vector2 pos = Projectile.Center + new Vector2(6 * dir, 20).RotatedBy(Projectile.rotation);
                Vector2 stick = (rot.ToRotationVector2() * 12 - Projectile.velocity * 0.05f) * (0.5f + Projectile.scale * 0.5f);
                int segments = 8;

                Vector2 lastPos = pos;

                for (int i = 0; i < segments; i++)
                {
                    float prog = i / (float)segments;
                    int segFrame = Math.Clamp((int)(prog * 5f), 1, 3);
                    if (i == 0)
                        segFrame = 0;
                    if (i == segments - 1)
                        segFrame = 4;

                    Rectangle frame = tentacleTexture.Frame(3, 5, 0, segFrame);
                    Rectangle glowFrame = tentacleTexture.Frame(3, 5, 1, segFrame);

                    Vector2 nextStick = stick.RotatedBy(oldVels[i].RotatedBy(-Projectile.rotation).ToRotation() + MathHelper.PiOver2).RotatedBy((float)Math.Sin((Projectile.localAI[0] * 0.15 - i * 0.8f) % MathHelper.TwoPi) * dir * 0.22f - dir * 0.06f);
                    float stickRot = lastPos.AngleTo(lastPos + nextStick);
                    Vector2 stretch = new Vector2(1f, 0.5f + lastPos.Distance(lastPos + nextStick) / 16f) * MathHelper.Lerp(Projectile.scale, 1f, i / (float)segments);
                    lastPos += nextStick;

                    float bloomScale = (float)Math.Pow(prog, 1.25f);
                    Main.EntitySpriteDraw(tentacleTexture.Value, lastPos - Main.screenPosition, frame, lightColor, stickRot - MathHelper.PiOver2, frame.Size() * 0.5f, stretch, 0, 0);
                    Main.EntitySpriteDraw(tentacleTexture.Value, lastPos - Main.screenPosition, glowFrame, growColor * bloomScale, stickRot - MathHelper.PiOver2, frame.Size() * 0.5f, stretch, 0, 0);
                    Main.EntitySpriteDraw(tentacleTexture.Value, lastPos - Main.screenPosition, glowFrame, growColor * 0.8f * bloomScale, stickRot - MathHelper.PiOver2, frame.Size() * 0.5f, stretch * 1.05f, 0, 0);
                    Main.EntitySpriteDraw(glow.Value, lastPos + oldVels[i] * 0.2f - Main.screenPosition, null, growColor * bloomScale * 0.2f, Projectile.rotation, glow.Size() * 0.5f, Projectile.scale * new Vector2(1f, 1.5f) * bloomScale, 0, 0);
                }
            }
        }
    }
}