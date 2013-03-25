using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class Ramp : Platform
    {
        public Corner corner;
        public Line rampLine;

        public Ramp()
        {
            type = EntType.Ramp;
            pos = new Vector2(0, 0);
            velocity = new Vector2(0, 0);
            size = new Vector2(0, 0);
            offset = new Vector2(0, 0);
            hitZone = new Vector2(1f, 1f);
            hitBox = new Rectangle(0, 0, 0, 0);
            corner = Corner.BottomLeft;

            if (corner.Equals(Corner.TopLeft) || corner.Equals(Corner.BottomRight))
            {
                rampLine = new Line(new Point(this.hitBox.Left, this.hitBox.Bottom), new Point(this.hitBox.Right, this.hitBox.Top));
            }
            else
            {
                rampLine = new Line(new Point(this.hitBox.Left, this.hitBox.Top), new Point(this.hitBox.Right, this.hitBox.Bottom));
            }

            invuln = true;
        }

        public Ramp(int x, int y, int width, int height, Corner c, Material m)
        {
            type = EntType.Ramp;
            pos = new Vector2(x, y);
            velocity = new Vector2(0, 0);
            size = new Vector2(width, height);
            offset = new Vector2(0f, 0f);
            hitZone = new Vector2(1f, 1f);
            hitBox = new Rectangle((int)(pos.X), (int)(pos.Y), (int)(size.X), (int)(size.Y));
            corner = c;

            if (corner.Equals(Corner.TopLeft) || corner.Equals(Corner.BottomRight))
            {
                rampLine = new Line(new Point(this.hitBox.Left, this.hitBox.Top), new Point(this.hitBox.Right, this.hitBox.Bottom));
            }
            else
            {
                rampLine = new Line(new Point(this.hitBox.Left, this.hitBox.Bottom), new Point(this.hitBox.Right, this.hitBox.Top));
            }

            matName = m.name;
            name = matName;

            invuln = true;
        }

        public override float resolveHeight(float x) //not assuming square
        {
            if (x < pos.X || x > pos.X + size.X)
                return 0f;

            if(corner.Equals(Corner.TopLeft) || corner.Equals(Corner.BottomRight))
            {
                return (((x-pos.X)/size.X)*size.Y)+pos.Y;
                //return size.Y + pos.Y;
            } else {
                return ((1f-((x - pos.X) / size.X)) * size.Y) + pos.Y;
            }
        }

        public override bool checkCollision(Entity e) //todo: modify for ramp
        {
            bool hit = false;
            Rectangle eHitBox = e.getHitBox();

            if (e.type.Equals(EntType.Character)
                && ((Character)e).leftWallSensor.intersects(this.rampLine))
            {
                hit = true;
                ((Character)e).leftWallHit = true;
                ((Character)e).leftTouchedRectangle = this.hitBox;
            }
            else if (e.type.Equals(EntType.Character)
                    && ((Character)e).rightWallSensor.intersects(this.rampLine))
            {
                hit = true;
                ((Character)e).rightWallHit = true;
                ((Character)e).rightTouchedRectangle = this.hitBox;
            }
            
            if (e.type.Equals(EntType.Character)
                    && this.rampLine.intersects(((Character)e).ceilingSensor))
            {
                hit = true;
                ((Character)e).hitHeadOnCeiling();
            }
            
            if (this.rampLine.collision(e.hitBox))
                hit = true;

            if (e.type.Equals(EntType.Character)
                && (this.rampLine.collision(((Character)e).underFeetSensors)))
                ((Character)e).underFeetCollisionList.Add(this);
            

            if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformLeft
                    && (this.rampLine.intersects(((Character)e).leftFloorSensor)))
            {
                ((Character)e).platformLeft = true;
            }

            if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformRight
                    && (this.rampLine.intersects(((Character)e).rightFloorSensor)))
            {
                ((Character)e).platformRight = true;
            }

            return hit;
        }
    }
}
