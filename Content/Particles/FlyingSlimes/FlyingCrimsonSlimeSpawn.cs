﻿using CalamityHunt.Common.Systems.Particles;
using CalamityHunt.Content.Projectiles;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Particles.FlyingSlimes
{
    public class FlyingCrimsonSlimeSpawn : Particle
    {
        public int time;
        public float distanceFade;
        public bool spiked;

        public override void OnSpawn()
        {
            scale *= Main.rand.NextFloat(0.9f, 1.1f);
            spiked = Main.rand.NextBool();
        }

        public override void Update()
        {
            if (data is Vector2)
            {
                velocity = Vector2.Lerp(velocity, position.DirectionTo((Vector2)data).SafeNormalize(Vector2.Zero) * 22f, 0.1f);
                if (position.Distance((Vector2)data) < 10)
                {
                    Active = false;
                    SoundEngine.PlaySound(GoozmaSpawn.slimeabsorb, position);
                }

                distanceFade = Utils.GetLerpValue(20, 80, position.Distance((Vector2)data), true);
            }
            else
                distanceFade = 1f;

            if (Main.rand.NextBool(10))
            {
                Dust slime = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20, 20), 306, velocity * 0.2f, DustID.Blood, color, 0.5f + Main.rand.NextFloat() * 0.3f);
                slime.noGravity = true;
            }

            time++;

            if (time > 500)
                Active = false;

            velocity *= 0.98f;
            rotation = velocity.ToRotation();

            color = Color.White;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
            if (spiked)
                texture = ModContent.Request<Texture2D>(Texture + "Spiked");

            float fadeIn = Utils.GetLerpValue(0, 30, time, true) * distanceFade;

            for (int i = 0; i < 10; i++)
                spriteBatch.Draw(texture.Value, position - velocity * i * 0.5f - Main.screenPosition, null, color.MultiplyRGBA(Lighting.GetColor(position.ToTileCoordinates())) * fadeIn * ((10f - i) / 80f), rotation + MathHelper.PiOver2, texture.Size() * 0.5f, scale * distanceFade * 1.05f, 0, 0);

            spriteBatch.Draw(texture.Value, position - Main.screenPosition, null, color.MultiplyRGBA(Lighting.GetColor(position.ToTileCoordinates())) * fadeIn, rotation + MathHelper.PiOver2, texture.Size() * 0.5f, scale * distanceFade, 0, 0);
        }
    }
}