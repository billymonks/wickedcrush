using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WickedCrush.Utility
{
    public class PointLight
    {
        private Vector2 offset;
        public Vector3 pos;
        public Vector4 diffuseColor, specularColor;
        public float range, intensity;

        Character anchor;

        public bool readyForRemoval = false;

        public PointLight()
        {
            this.anchor = null;
            pos = Vector3.Zero;
            this.diffuseColor = Vector4.Zero;
            this.specularColor = Vector4.Zero;
            this.range = 0f;
            this.intensity = 0f;
            this.offset = Vector2.Zero;
        }

        public PointLight(Character anchor,
            float depth,
            Vector4 diffuseColor,
            Vector4 specularColor,
            float range,
            float intensity,
            Vector2 offset)
        {
            this.anchor = anchor;
            pos = new Vector3(anchor.pos.X, anchor.pos.Y, depth);
            this.diffuseColor = diffuseColor;
            this.specularColor = specularColor;
            this.range = range;
            this.intensity = intensity;
            this.offset = offset;
        }

        public virtual void Update(GameTime gameTime)
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
        }
    }
}
