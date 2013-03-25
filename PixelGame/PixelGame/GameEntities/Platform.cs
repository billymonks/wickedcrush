using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class Platform : Entity
    {
        public string matName;

        public enum EdgeSides
        {
            Top = 0,
            Bottom = 1,
            Left = 2,
            Right = 3
        }

        public Platform()
        {
            type = EntType.Platform;
            pos = new Vector2(0, 0);
            velocity = new Vector2(0, 0);
            size = new Vector2(0, 0);
            offset = new Vector2(0, 0);
            hitZone = new Vector2(1f, 1f);
            hitBox = new Rectangle(0, 0, 0, 0);

            invuln = true;
        }

        public Platform(int x, int y, int width, int height, Material m)
        {
            type = EntType.Platform;
            pos = new Vector2(x, y);
            velocity = new Vector2(0, 0);
            size = new Vector2(width, height);
            offset = new Vector2(0f, 0f);
            hitZone = new Vector2(1f, 1f);
            hitBox = new Rectangle((int)(pos.X), (int)(pos.Y), (int)(size.X), (int)(size.Y));

            matName = m.name;
            name = matName;

            invuln = true;
        }

        public override void Update(GameTime gameTime)
        {
            pos += velocity;
            updateBox();
        }

        public override Rectangle getHitBox()
        {
            return hitBox;
        }

        //public float resolveHeight(float x)
        //{
            //return size.Y;
        //}
        
        public override bool checkCollision(Entity e) 
        {
            bool hit = false;
            Rectangle eHitBox = e.getHitBox();

            if (e.type.Equals(EntType.Character)
                && ((Character)e).leftWallSensor.collision(this.hitBox))
            {
                hit = true;
                ((Character)e).leftWallHit = true;
                ((Character)e).leftTouchedRectangle = this.hitBox;
            }
            else if (e.type.Equals(EntType.Character)
                    && ((Character)e).rightWallSensor.collision(this.hitBox))
            {
                hit = true;
                ((Character)e).rightWallHit = true;
                ((Character)e).rightTouchedRectangle = this.hitBox;
            }
            else
            {
                if (e.hitBox.Intersects(this.hitBox))
                    hit = true;
                if (e.type.Equals(EntType.Character)
                    && ((Character)e).underFeetSensors.Intersects(this.hitBox))
                    ((Character)e).underFeetCollisionList.Add(this);
            }
            
            if (e.type.Equals(EntType.Character)
                && ((Character)e).ceilingSensor.collision(this.hitBox))
                    //&& (this.hitBox.Contains(((Character)e).ceilingSensor.start)
                    //|| (this.hitBox.Contains(((Character)e).ceilingSensor.end))))
            {
                hit = true;
                ((Character)e).hitHeadOnCeiling();
            }
            

            if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformLeft
                    && (this.hitBox.Contains(((Character)e).floorSensor.start)))
            {
                ((Character)e).platformLeft = true;
            }

            if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformRight
                    && (this.hitBox.Contains(((Character)e).floorSensor.end)))
            {
                ((Character)e).platformRight = true;
            }

            return hit;
        }
    }
}
