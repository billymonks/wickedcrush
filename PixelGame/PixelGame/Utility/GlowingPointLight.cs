using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush.Utility
{
    public class GlowingPointLight : PointLight
    {
        private Vector2 offset;
        //public Vector3 pos;
        //public Vector4 diffuseColor, specularColor;
        //public float range, intensity, variation, savedRange;
        public float variation, savedIntensity;
        public double speed, time;

        Character anchor;

        //public bool readyForRemoval = false;

        public GlowingPointLight(Character anchor,
            float depth,
            Vector4 diffuseColor,
            Vector4 specularColor,
            float range,
            float intensity,
            Vector2 offset,
            float variation,
            double speed)
        {
            this.anchor = anchor;
            pos = new Vector3(anchor.pos.X, anchor.pos.Y, depth);
            this.diffuseColor = diffuseColor;
            this.specularColor = specularColor;
            this.range = range;
            this.intensity = intensity;
            this.savedIntensity = intensity;
            this.offset = offset;
            this.variation = variation;
            this.speed = speed;

            time = 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (anchor == null || anchor.readyForRemoval)
            {
                readyForRemoval = true;
            }
            else
            {
                pos.X = anchor.pos.X+offset.X;
                pos.Y = anchor.pos.Y+offset.Y;
            }

            time += gameTime.ElapsedGameTime.Milliseconds;

            intensity = savedIntensity + ((float)Math.Sin(time / speed) * variation);
        }
    }
}
