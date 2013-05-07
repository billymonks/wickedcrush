using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WickedCrush
{
    public enum EntType
    {
        Platform = 0,
        Character = 1,
        Enemy = 2,
        Collectable = 3,
        Ramp = 4
    }
    public abstract class Entity
    {
        public EntType type;
        public Vector2 pos, size, offset, hitZone, velocity, accel; //hitZone between 0f and 1f, the % of sprite that involves collisions
        //public Texture2D tex;
        public Rectangle hitBox;
        public List<Entity> collisionList;

        public String name;
        public bool invuln = false;

        public int hp = 0;

        public abstract void Update(GameTime gameTime);
        public abstract Rectangle getHitBox();

        // checks the broad hitbox that surrounds every entity
        public virtual bool checkCollision(Entity e) 
        {
            if (e == null)
                return false;
            return e.getHitBox().Intersects(hitBox); //fast and wild baby
        }
        public bool inGrid(Vector2 v)
        {
            return hitBox.Contains((int)v.X, (int)v.Y);
        }

        public Point GetGrid(Vector2 pos) //incomplete
        {
            return new Point(0, 0);
        }

        public void applyForce(Vector2 force)
        {
            velocity += force;
        }

        public void updateBox()
        {
            //graphicBox.Offset((int)(pos.X - graphicBox.X), (int)(pos.Y - graphicBox.Y));
            //hitBox.Offset((int)(pos.X - graphicBox.X), (int)(pos.Y - graphicBox.Y));
        }

        public virtual float resolveHeight(float x)
        {
            return size.Y+pos.Y;
        }

        public float CalculateDistance(Entity e) // a lot of casting, performance?
        {
            return (float)Math.Sqrt(Math.Pow((double)(this.pos.X - e.pos.X), 2.0) + Math.Pow((double)(this.pos.Y - e.pos.Y), 2.0));
        }

        public float CalculateXDistance(float f)
        {
            return Math.Abs((pos.X - offset.X) - f);
        }

        public float CalculateYDistance(float f)
        {
            return Math.Abs((pos.Y - offset.Y) - f);
        }

    }
}
