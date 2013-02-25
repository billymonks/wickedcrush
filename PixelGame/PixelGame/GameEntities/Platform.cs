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
        float[] hMap; // between 0 and 1
        Rectangle[] detailedHitBox;

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
            //graphicBox = new Rectangle(0, 0, 0, 0);
            hitBox = new Rectangle(0, 0, 0, 0);
            hMap = new float[1] {1f};
            detailedHitBox = new Rectangle[hMap.Length];
            detailedHitBox[0] = new Rectangle(0, 0, 0, 0);

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
            //graphicBox = new Rectangle(x, y, width, height);
            //hitBox = new Rectangle((int)(pos.X + offset.X), (int)(pos.Y + offset.Y), (int)(size.X * hitZone.X), (int)(size.Y * hitZone.Y));
            hitBox = new Rectangle((int)(pos.X), (int)(pos.Y), (int)(size.X), (int)(size.Y));
            detailedHitBox = new Rectangle[1];
            detailedHitBox[0] = hitBox;

            matName = m.name;
            name = matName;

            invuln = true;
        }

        public override void Update(GameTime gameTime)
        {
            pos += velocity;
            updateBox();
        }

        /*public override Texture2D getTexture()
        {
            return tex;
        }

        public override void setTexture(Texture2D t)
        {
            tex = t;
        }*/

        //public override Rectangle getGraphicBox()
        //{
            //return graphicBox;
        //}

        public override Rectangle getHitBox()
        {
            return hitBox;
        }
        
        public float resolveHeight(float x)
        {
            int tempInt = (int)Math.Floor(((x - pos.X) / size.X) * detailedHitBox.Length);
            if (tempInt < 0) //should always be corrected by other sensor
                return 0f;
            if (tempInt >= detailedHitBox.Length) //should always be corrected by other sensor
                return 0f;
            return (float)detailedHitBox[tempInt].Y + detailedHitBox[tempInt].Height;
        }
        
        public Rectangle[] offsetDHB(Rectangle[] dHB)
        {
            Rectangle[] finalDHB = new Rectangle[dHB.Length];
            for (int i = 0; i < dHB.Length; i++)
            {
                finalDHB[i] = new Rectangle(dHB[i].X + (int)pos.X, dHB[i].Y + (int)pos.Y, dHB[i].Width + (int)pos.X, dHB[i].Height + (int)pos.Y);
            }
            return finalDHB;
        }

        public Rectangle[] createDHB(float[] hM, bool ceiling)
        {
            Rectangle[] finalDHB = new Rectangle[hM.Length];
            for (int i = 0; i < hM.Length; i++)
            {
                if (hM[i] != 0f)
                {
                    if (!ceiling)
                        finalDHB[i] = new Rectangle((int)((size.X / hM.Length) * i + pos.X), (int)pos.Y, (int)(size.Y / hM.Length), (int)(size.Y * hM[i]));
                    else
                        finalDHB[i] = new Rectangle((int)((size.X / hM.Length) * i + pos.X), (int)(pos.Y + size.Y * (1f - hM[i])), (int)(size.Y / hM.Length), (int)(size.Y * hM[i]));
                }
            }
            return finalDHB;
        }
        public bool checkDetailedCollision(Entity e)
        {
            bool hit = false;
            for (int i = 0; i < detailedHitBox.Length; i++)
            {
                if (e.type.Equals(EntType.Character)
                    && ((Character)e).leftWallSensor.collision(detailedHitBox[i]))
                {
                    hit = true;
                    ((Character)e).leftWallHit = true;
                    ((Character)e).leftTouchedRectangle = detailedHitBox[i];
                }
                else if (e.type.Equals(EntType.Character)
                    && ((Character)e).rightWallSensor.collision(detailedHitBox[i]))
                {
                    hit = true;
                    ((Character)e).rightWallHit = true;
                    ((Character)e).rightTouchedRectangle = detailedHitBox[i];
                }
                /*if (e.type.Equals(EntType.Character)
                    && detailedHitBox[i].Contains(((Character)e).wallSensor.start))
                {
                    hit = true;
                    ((Character)e).leftWallHit = true;
                    ((Character)e).leftTouchedRectangle = detailedHitBox[i];
                }
                else if (e.type.Equals(EntType.Character)
                    && detailedHitBox[i].Contains(((Character)e).wallSensor.end))
                {
                    hit = true;
                    ((Character)e).rightWallHit = true;
                    ((Character)e).rightTouchedRectangle = detailedHitBox[i];
                }*/
                else if (e.type.Equals(EntType.Character)
                    && (detailedHitBox[i].Contains(((Character)e).ceilingSensor.start)
                    || (detailedHitBox[i].Contains(((Character)e).ceilingSensor.end))))
                {
                    hit = true;
                    ((Character)e).hitHeadOnCeiling();
                }
                else
                {
                    if (e.hitBox.Intersects(detailedHitBox[i]))
                        hit = true;
                    if (e.type.Equals(EntType.Character)
                        && ((Character)e).underFeetSensors.Intersects(detailedHitBox[i]))
                        ((Character)e).underFeetCollisionList.Add(detailedHitBox[i]);
                }

                if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformLeft
                    && (detailedHitBox[i].Contains(((Character)e).floorSensor.start)))
                {
                    ((Character)e).platformLeft = true;
                }

                if (e.type.Equals(EntType.Character)
                    && !((Character)e).platformRight
                    && (detailedHitBox[i].Contains(((Character)e).floorSensor.end)))
                {
                    ((Character)e).platformRight = true;
                }

            }
            return hit;
        }

        public void editorCheckDetailedCollision(Character c)
        {
            for (int i = 0; i < detailedHitBox.Length; i++)
            {
                if (c.hitBox.Intersects(detailedHitBox[i]))
                    c.collisionList.Add(this);
                if (c.underFeetSensors.Intersects(detailedHitBox[i]))
                    c.underFeetCollisionList.Add(detailedHitBox[i]);
            }
        }
        /*public void checkDetailedFeetCollision(Character c) // deprecated
        {
            for (int i = 0; i < detailedHitBox.Length; i++)
            {
                if (c.underFeetSensors.Intersects(detailedHitBox[i]))
                    c.underFeetCollisionList.Add(detailedHitBox[i]);
                    //return true;
            }
            //return false;
        }*/
    }
}
