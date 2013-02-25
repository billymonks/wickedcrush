using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush.Utility
{
    public class FlickeringPointLight : PointLight
    {
        private Vector2 offset;
        //public Vector3 pos;
        //public Vector4 diffuseColor, specularColor;
        //public float range, intensity, variation, savedRange;
        public float variation, savedIntensity;
        private Random random;

        Character anchor;

        //public bool readyForRemoval = false;

        public FlickeringPointLight(Character anchor,
            float depth,
            Vector4 diffuseColor,
            Vector4 specularColor,
            float range,
            float intensity,
            Vector2 offset,
            float variation)
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

            random = new Random();
        }

        public override void Update(GameTime gameTime)
        {
            if (anchor == null || anchor.readyForRemoval)
            {
                readyForRemoval = true;
            }
            else
            {
                pos.X = anchor.pos.X + offset.X;
                pos.Y = anchor.pos.Y + offset.Y;
            }

            intensity = savedIntensity + ((float)random.NextDouble() * variation);
        }
    }
}