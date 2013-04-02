using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.Utility;
using WickedCrush.GameStates;
using Microsoft.Xna.Framework.Audio;

namespace WickedCrush
{
    public enum LightingType
    {
        PerPixel = 0,
        FullBright = 1
    }
    /* a character is a sprite with logic rendered in 3d space */
    public class Character : Entity
    {
        #region fields
        public Dictionary<String, Animation> animationList;
        public Animation currentAnimation;
        public Animation currentAnimationNorm;
        public Texture2D defaultTexture, defaultNormal; //for editor only
        public StateMachine sm;

        public VertexPositionNormalTextureTangentBinormal[] vertices;

        public Direction facingDir = Direction.Right;

        public Rectangle underFeetSensors;
        private const int SENSOR_HEIGHT = 20;
        private const float GRAVITY = 0.25f;

        public Line ceilingSensor, wallSensor, floorSensor, leftFloorSensor, rightFloorSensor, leftWallSensor, rightWallSensor;
        public bool leftWallHit, rightWallHit, platformLeft, platformRight;
        public Rectangle leftTouchedRectangle, rightTouchedRectangle;

        public List<Entity> underFeetCollisionList;

        public bool walkThrough = false;
        public bool ignorePlatforms = false;
        public bool immobile = false;

        public bool hurtFlash = false;
        public bool blockFlash = false;

        private Vector2 maxVelocity = new Vector2(10f, 20f);

        //vertex information
        protected Vector3 normal, tangent, binormal;
        protected Vector4 topLeft, bottomLeft, bottomRight, topRight;
        protected Vector2 texTopLeft, texBottomLeft, texBottomRight, texTopRight;

        public float specular = 1f;
        
        float depth;

        public bool readyForRemoval = false;
        
        public VertexBuffer vb;
        public int primCount;

        public bool variableSize = false;
        public bool bright = false;
        public bool airborne = false;

        protected Vector3 centerPoint;

        #endregion

        public Character()
        {
            animationList = new Dictionary<String, Animation>();

            type = EntType.Character;

            pos = new Vector2(0f, 0f);
            size = new Vector2(1f, 1f);
            offset = new Vector2(0f, 0f);
            hitZone = new Vector2(1f, 1f);
            velocity = new Vector2(0f, 0f);
            depth = 128f; //half of gridSize, static for now

            collisionList = new List<Entity>();
            underFeetCollisionList = new List<Entity>();

            currentAnimationNorm = null;

            SetupHitBox();
            InitSensors();
            SetupVerts();

            centerPoint = new Vector3(pos.X, pos.Y, depth);

            name = "Unnamed_Character";
        }

        public Character(Vector2 pos, Vector2 size, GraphicsDevice gd)
        {
            animationList = new Dictionary<String, Animation>();

            type = EntType.Character;

            this.pos = pos;
            this.size = size;
            offset = new Vector2(0f, 0f);
            hitZone = new Vector2(1f, 1f);
            velocity = new Vector2(0f, 0f);
            depth = 128f; //half of gridSize, static for now

            collisionList = new List<Entity>();
            underFeetCollisionList = new List<Entity>();

            currentAnimationNorm = null;

            SetupHitBox();
            InitSensors();
            SetupVerts();

            SetupVertexBuffer(gd);

            centerPoint = new Vector3(pos.X - offset.X, pos.Y - offset.Y, depth);

            name = "Unnamed_Character";
        }

        public Character(Vector2 pos, Vector2 size, Vector2 hitZone, GraphicsDevice gd)
        {
            animationList = new Dictionary<String, Animation>();

            type = EntType.Character;

            this.pos = pos;
            this.size = size;
            this.hitZone = hitZone;
            offset = new Vector2(0f, 0f);
            velocity = new Vector2(0f, 0f);
            depth = 128f; //half of gridSize, static for now

            collisionList = new List<Entity>();
            underFeetCollisionList = new List<Entity>();

            currentAnimationNorm = null;

            SetupHitBox();
            InitSensors();
            SetupVerts();

            SetupVertexBuffer(gd);

            centerPoint = new Vector3(pos.X, pos.Y, depth);

            name = "Unnamed_Character";
        }

        public Character(Vector2 pos, Vector2 size, Vector2 hitZone, Vector2 offset, GraphicsDevice gd)
        {
            animationList = new Dictionary<String, Animation>();

            type = EntType.Character;

            this.pos = pos;
            this.size = size;
            this.hitZone = hitZone;
            this.offset = offset;
            velocity = new Vector2(0f, 0f);
            accel = new Vector2(0f, 0f);
            depth = 128f; //half of gridSize, static for now

            collisionList = new List<Entity>();
            underFeetCollisionList = new List<Entity>();

            currentAnimationNorm = null;

            SetupHitBox();
            InitSensors();
            SetupVerts();

            SetupVertexBuffer(gd);

            centerPoint = new Vector3(pos.X, pos.Y, depth);

            name = "Unnamed_Character";
        }

        public Character(Vector2 pos, Vector2 size, Vector2 hitZone, Vector2 offset, float depth, GraphicsDevice gd)
        {
            animationList = new Dictionary<String, Animation>();

            type = EntType.Character;

            this.pos = pos-offset;
            this.size = size;
            this.hitZone = hitZone;
            this.offset = offset;

            velocity = new Vector2(0f, 0f);
            accel = new Vector2(0f, 0f);
            this.depth = depth;

            collisionList = new List<Entity>();
            underFeetCollisionList = new List<Entity>();

            currentAnimationNorm = null;

            SetupHitBox();
            InitSensors();
            SetupVerts();

            SetupVertexBuffer(gd);

            centerPoint = new Vector3(pos.X, pos.Y, depth);

            name = "Unnamed_Character";
        }

        public virtual void TakeDamage(int dmgAmount, float force, Direction atkDir) //can be called by other characters & overridden to trigger states
        {
            hurtFlash = true;

            hp -= dmgAmount;
            if (atkDir == Direction.Left)
                this.facingDir = Direction.Right;
            else
                this.facingDir = Direction.Left;
        }

        public virtual void AttackCallback()
        {

        }

        public void SetupHitBox()
        {
            hitBox = new Rectangle((int)(pos.X), (int)(pos.Y), (int)(size.X*hitZone.X), (int)(size.Y*hitZone.Y));
            underFeetSensors = new Rectangle(hitBox.Left+6, hitBox.Y - SENSOR_HEIGHT, hitBox.Width-12, SENSOR_HEIGHT);
        }

        protected virtual void SetupVerts()
        {
            normal = new Vector3(0f, 0f, 1f);
            tangent = new Vector3(0f, 1f, 0f);
            binormal = Vector3.Cross(tangent, normal);

            topLeft = new Vector4(pos.X + offset.X, pos.Y + size.Y + offset.Y, depth, 1f);
            bottomLeft = new Vector4(pos.X + offset.X, pos.Y + offset.Y, depth, 1f);
            bottomRight = new Vector4(pos.X + size.X + offset.X, pos.Y + offset.Y, depth, 1f);
            topRight = new Vector4(pos.X + size.X + offset.X, pos.Y + size.Y + offset.Y, depth, 1f);

            if (currentAnimation != null)
            {
                texTopLeft = currentAnimation.getTopLeftCoordinate();
                texBottomLeft = currentAnimation.getBottomLeftCoordinate();
                texBottomRight = currentAnimation.getBottomRightCoordinate();
                texTopRight = currentAnimation.getTopRightCoordinate();
            }
            else
            {
                texTopLeft = new Vector2(0f, 0f);
                texBottomLeft = new Vector2(0f, 1f);
                texBottomRight = new Vector2(1f, 1f);
                texTopRight = new Vector2(1f, 0f);
            }

            vertices = new VertexPositionNormalTextureTangentBinormal[6];
            //top left
            vertices[0] = new VertexPositionNormalTextureTangentBinormal(
                    topLeft,
                    normal,
                    texTopLeft,
                    texTopLeft, 
                    tangent, 
                    binormal);
            //bottom left
            vertices[1] = new VertexPositionNormalTextureTangentBinormal(
                bottomLeft,
                normal,
                texBottomLeft,
                texBottomLeft, 
                tangent,
                binormal);
            //bottom right
            vertices[2] = new VertexPositionNormalTextureTangentBinormal(
                bottomRight,
                normal,
                texBottomRight,
                texBottomRight, 
                tangent,
                binormal);
            //top right
            vertices[3] = new VertexPositionNormalTextureTangentBinormal(
                topRight,
                normal,
                texTopRight,
                texTopRight, 
                tangent,
                binormal);

            vertices[4] = vertices[0];
            vertices[5] = vertices[2];
        }

        public override void Update(GameTime gameTime)
        {
            hurtFlash = false;
            blockFlash = false;

            ResolveCollisions();

            if (sm != null)
            {
                sm.Update(gameTime, this);
            }

            if (!immobile)
            {
                if (leftWallHit)
                {
                    //pos.X = GetLeftWallFixedPosition(); //needs improvement
                    if (velocity.X < 0f)
                        velocity.X = 0f;
                }
                if (rightWallHit)
                {
                    //pos.X = GetRightWallFixedPosition(); //needs improvement
                    if (velocity.X > 0f)
                        velocity.X = 0f;
                }
            }

            

            collisionList.Clear();
            underFeetCollisionList.Clear();

            

            velocity += accel * ((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f) * 60.0f;
            pos += velocity * ((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f) * 60.0f;

            centerPoint.X = pos.X - offset.X;
            centerPoint.Y = pos.Y - offset.Y;
            centerPoint.Z = -depth;

            EnforceMaxVelocity();

            UpdateAnimation(gameTime);
            UpdateHitBox();
            UpdateSensors();
            UpdateVertexPositions();
            UpdateVertexBuffer();
            UpdateSoundPosition();
        }

        private void ResolveCollisions()
        {
            /*foreach (Entity e in collisionList)
            {
                if (((Character)e).hitBox.Contains(wallSensor.start))
                {
                    leftWallHit = true;
                }
                else if (((Character)e).hitBox.Contains(wallSensor.end))
                {
                    rightWallHit = true;
                }
                if (((Character)e).hitBox.Intersects(underFeetSensors))
                {
                    underFeetCollisionList.Add(((Character)e).hitBox);
                }

            }*/
        }

        public void EditorUpdate()
        {
            UpdateHitBox();
            UpdateSensors();
            UpdateVertexPositions();
            UpdateVertexBuffer();
        }

        protected void UpdateHitBox()
        {
            hitBox.X = (int)(pos.X);
            hitBox.Y = (int)(pos.Y);

            underFeetSensors.X = hitBox.X+6;
            underFeetSensors.Y = hitBox.Y - SENSOR_HEIGHT;
        }

        protected virtual void UpdateTextureCoords()
        {
            if (facingDir == Direction.Right)
            {
                texTopLeft = currentAnimation.getTopLeftCoordinate();
                texBottomLeft = currentAnimation.getBottomLeftCoordinate();
                texBottomRight = currentAnimation.getBottomRightCoordinate();
                texTopRight = currentAnimation.getTopRightCoordinate();
            }
            else
            {
                texTopLeft = currentAnimation.getTopRightCoordinate();
                texBottomLeft = currentAnimation.getBottomRightCoordinate();
                texBottomRight = currentAnimation.getBottomLeftCoordinate();
                texTopRight = currentAnimation.getTopLeftCoordinate();
            }

            vertices[0].TextureCoordinate = texTopLeft;
            vertices[1].TextureCoordinate = texBottomLeft;
            vertices[2].TextureCoordinate = texBottomRight;
            vertices[3].TextureCoordinate = texTopRight;

            vertices[4].TextureCoordinate = vertices[0].TextureCoordinate;
            vertices[5].TextureCoordinate = vertices[2].TextureCoordinate;

            if (currentAnimationNorm != null)
            {
                if (facingDir == Direction.Right)
                {
                    vertices[0].NormalCoordinate = currentAnimationNorm.getTopLeftCoordinate();
                    vertices[1].NormalCoordinate = currentAnimationNorm.getBottomLeftCoordinate();
                    vertices[2].NormalCoordinate = currentAnimationNorm.getBottomRightCoordinate();
                    vertices[3].NormalCoordinate = currentAnimationNorm.getTopRightCoordinate();
                }
                else
                {
                    vertices[0].NormalCoordinate = currentAnimationNorm.getTopRightCoordinate();
                    vertices[1].NormalCoordinate = currentAnimationNorm.getBottomRightCoordinate();
                    vertices[2].NormalCoordinate = currentAnimationNorm.getBottomLeftCoordinate();
                    vertices[3].NormalCoordinate = currentAnimationNorm.getTopLeftCoordinate();
                }

                vertices[4].NormalCoordinate = vertices[0].NormalCoordinate;
                vertices[5].NormalCoordinate = vertices[2].NormalCoordinate;
            }
        }

        protected virtual void UpdateVertexPositions()
        {
            topLeft.X = pos.X + offset.X;
            topLeft.Y = pos.Y + size.Y + offset.Y;
            bottomLeft.X = pos.X + offset.X;
            bottomLeft.Y = pos.Y + offset.Y;
            bottomRight.X = pos.X + size.X + offset.X;
            bottomRight.Y = pos.Y + offset.Y;
            topRight.X = pos.X + size.X + offset.X;
            topRight.Y = pos.Y + size.Y + offset.Y;

            vertices[0].Position = topLeft;
            vertices[1].Position = bottomLeft;
            vertices[2].Position = bottomRight;
            vertices[3].Position = topRight;

            vertices[4].Position = vertices[0].Position;
            vertices[5].Position = vertices[2].Position;
        }

        public override Rectangle getHitBox()
        {
            return hitBox;
        }

        private void UpdateStateMachine(GameTime gameTime)
        {
            sm.Update(gameTime, this);
        }

        protected void UpdateAnimation(GameTime gameTime)
        {
            if (currentAnimation != null)
            {
                currentAnimation.Update(gameTime);
                UpdateTextureCoords();
            }
            if (currentAnimationNorm != null)
                currentAnimationNorm.Update(gameTime);
        }

        public float GetHighestSensorPoint()
        {
            float tempHeight = 0f;
            for (int i = 0; i < underFeetCollisionList.Count; i++)
            {
                /*if (tempHeight < underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Left))
                    tempHeight = underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Left);
                if (tempHeight < underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Right))
                    tempHeight = underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Right);*/
                if (underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Left) > tempHeight)
                    tempHeight = underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Left); //kinda weird because 3d y goes up, 2d y goes down
                if (underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Right) > tempHeight)
                    tempHeight = underFeetCollisionList[i].resolveHeight((float)underFeetSensors.Right); //kinda weird because 3d y goes up, 2d y goes down
            }
            return tempHeight;
        }

        public bool checkFeetCollision(Entity e)
        {
            if (e == null)
                return false;
            return e.getHitBox().Intersects(underFeetSensors); //fast and wild baby
        }

        public bool checkCharacterCollision(Character c)
        {
            bool hit = false;

            if(this.hitBox.Intersects(c.hitBox))
                hit = true;

            if(hit && !c.walkThrough && !this.walkThrough)
            {
                if (this.leftWallSensor.collision(c.hitBox))
                {
                    this.leftWallHit = true;
                    this.leftTouchedRectangle = c.hitBox;
                }
                else if (this.rightWallSensor.collision(c.hitBox))
                {
                    this.rightWallHit = true;
                    this.rightTouchedRectangle = c.hitBox;
                }
                
                /*if (c.hitBox.Contains(this.wallSensor.start))
                {
                    this.leftWallHit = true;
                    this.leftTouchedRectangle = c.hitBox;
                } else if (c.hitBox.Contains(this.wallSensor.end)) {
                    this.rightWallHit = true;
                    this.rightTouchedRectangle = c.hitBox;
                }*/

                if (c.leftWallSensor.collision(this.hitBox))
                {
                    c.leftWallHit = true;
                    c.leftTouchedRectangle = this.hitBox;
                }
                else if (c.rightWallSensor.collision(this.hitBox))
                {
                    c.rightWallHit = true;
                    c.rightTouchedRectangle = this.hitBox;
                }

                /*if (this.hitBox.Contains(c.wallSensor.start))
                {
                    c.leftWallHit = true;
                    c.leftTouchedRectangle = this.hitBox;
                }
                else if (this.hitBox.Contains(c.wallSensor.end))
                {
                    c.rightWallHit = true;
                    c.rightTouchedRectangle = this.hitBox;
                }*/
            }

            return hit;
        }

        protected virtual void UpdateSoundPosition()
        {

        }

        public virtual void StopSoundInstances()
        {

        }

        private void InitSensors()
        {
            ceilingSensor = new Line(
                new Point(hitBox.Left+3, hitBox.Bottom),
                new Point(hitBox.Right-3, hitBox.Bottom));
            wallSensor = new Line(
                new Point(hitBox.Left, hitBox.Top + SENSOR_HEIGHT),
                new Point(hitBox.Right, hitBox.Top + SENSOR_HEIGHT));
            floorSensor = new Line(
                new Point(underFeetSensors.Left, underFeetSensors.Top),
                new Point(underFeetSensors.Right, underFeetSensors.Top));

            leftFloorSensor = new Line(
                new Point(underFeetSensors.Left+2, underFeetSensors.Top),
                new Point(underFeetSensors.Left+2, underFeetSensors.Bottom));

            rightFloorSensor = new Line(
                new Point(underFeetSensors.Right-2, underFeetSensors.Top),
                new Point(underFeetSensors.Right-2, underFeetSensors.Bottom));

            leftWallSensor = new Line(
                new Point(hitBox.Left, hitBox.Top + SENSOR_HEIGHT),
                new Point(hitBox.Left, hitBox.Bottom - SENSOR_HEIGHT));

            rightWallSensor = new Line(
                new Point(hitBox.Right, hitBox.Top + SENSOR_HEIGHT),
                new Point(hitBox.Right, hitBox.Bottom - SENSOR_HEIGHT));

            leftWallHit = false;
            rightWallHit = false;
            platformLeft = false;
            platformRight = false;

            leftTouchedRectangle = Rectangle.Empty;
            rightTouchedRectangle = Rectangle.Empty;
        }

        private void UpdateSensors()
        {
            ceilingSensor.start.X = hitBox.Left+5;
            ceilingSensor.end.X = hitBox.Right-5;
            ceilingSensor.start.Y = hitBox.Bottom;
            ceilingSensor.end.Y = hitBox.Bottom;

            wallSensor.start.X = hitBox.Left;
            wallSensor.end.X = hitBox.Right;
            wallSensor.start.Y = hitBox.Top + SENSOR_HEIGHT;
            wallSensor.end.Y = hitBox.Top + SENSOR_HEIGHT;

            floorSensor.start.X = underFeetSensors.Left+2;
            floorSensor.start.Y = underFeetSensors.Top;
            floorSensor.end.X = underFeetSensors.Right-2;
            floorSensor.end.Y = underFeetSensors.Top;

            leftFloorSensor.start.X = underFeetSensors.Left+3;
            leftFloorSensor.start.Y = underFeetSensors.Bottom;
            leftFloorSensor.end.X = underFeetSensors.Left+3;
            leftFloorSensor.end.Y = underFeetSensors.Top;

            rightFloorSensor.start.X = underFeetSensors.Right-2;
            rightFloorSensor.start.Y = underFeetSensors.Bottom;
            rightFloorSensor.end.X = underFeetSensors.Right-2;
            rightFloorSensor.end.Y = underFeetSensors.Top;

            leftWallSensor.start.X = hitBox.Left-1;
            leftWallSensor.start.Y = hitBox.Top + SENSOR_HEIGHT;
            leftWallSensor.end.X = hitBox.Left-1;
            leftWallSensor.end.Y = hitBox.Bottom - SENSOR_HEIGHT;

            rightWallSensor.start.X = hitBox.Right+1;
            rightWallSensor.start.Y = hitBox.Top + SENSOR_HEIGHT;
            rightWallSensor.end.X = hitBox.Right+1;
            rightWallSensor.end.Y = hitBox.Bottom - SENSOR_HEIGHT;


            leftWallHit = false;
            rightWallHit = false;
            platformLeft = false;
            platformRight = false;

            leftTouchedRectangle = Rectangle.Empty;
            rightTouchedRectangle = Rectangle.Empty;
        }

        private void EnforceMaxVelocity()
        {
            if (velocity.X > maxVelocity.X)
                velocity.X = maxVelocity.X;
            if (velocity.X < -maxVelocity.X)
                velocity.X = -maxVelocity.X;
            if (velocity.Y > maxVelocity.Y)
                velocity.Y = maxVelocity.Y;
            if (velocity.Y < -maxVelocity.Y)
                velocity.Y = -maxVelocity.Y;
        }

        protected void ApplyGravity()
        {
            accel.Y -= GRAVITY;
        }

        public void hitHeadOnCeiling()
        {
            if (velocity.Y > 0f)
                velocity.Y = 0f;
        }

        protected float GetLeftWallFixedPosition()
        {
            return leftTouchedRectangle.Right;
        }
        protected float GetRightWallFixedPosition()
        {
            return rightTouchedRectangle.Left-hitBox.Width;
        }

        private void UpdateVertexBuffer()
        {
            vb.SetData(vertices);
        }
        private void SetupVertexBuffer(GraphicsDevice gd)
        {
            vb = new VertexBuffer(gd, typeof(VertexPositionNormalTextureTangentBinormal), vertices.Length, BufferUsage.None);
            vb.SetData(vertices);
            primCount = vertices.Length / 3;
        }

        public float CheckXDistance(Character c)
        {
            //return Math.Abs((pos.X - offset.X) - (c.pos.X - c.offset.X));
            return Math.Abs(centerPoint.X - c.centerPoint.X);
        }

        public float CheckYDistance(Character c)
        {
            return Math.Abs((pos.Y - offset.Y) - (c.pos.Y - c.offset.Y));
        }

        public float CheckXDisplacement(Character c)
        {
            return ((pos.X - offset.X) - (c.pos.X - c.offset.X));
        }
    }
}
