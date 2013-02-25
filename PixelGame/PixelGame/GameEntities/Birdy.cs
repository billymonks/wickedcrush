using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class Birdy : Character
    {
        private CharacterFactory _cf;
        private Vector2 targetPos;
        private float range = 40f;

        private float speed = 1f;
        private float maxSpeed = 15f;
        private Random random;

        public Birdy(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf, Direction d)
            : base(pos, new Vector2(64f, 64f), new Vector2(0.8f, 0.34f), new Vector2(-6.4f, -21.12f), 128f, gd)
        {
            _cf = cf;
            targetPos = new Vector2(pos.X, pos.Y);
            random = new Random();

            facingDir = d;

            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 2;
            name = "Birdy";
            CreateAnimationList(cm);
            CreateStateMachine();

            airborne = true;

            //facingDir = Direction.Right;
        }

        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("bird-right", new Animation("bird-right", cm));

            defaultTexture = animationList["bird-right"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("bird-load", new State("bird-load",
                c => sm.previousControlState == null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["bird-right"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["bird-right"];
                    /*this.velocity.X = 4f;
                    this.velocity.Y = (float)random.NextDouble() * 2f;
                    this.accel.X = (float)random.NextDouble() * speed;
                    this.accel.Y = (float)random.NextDouble() * speed;*/

                    this.velocity.X = 0f;
                    this.velocity.Y = 0f;
                    this.accel.X = 0f;
                    this.accel.Y = 0f;
                }));

            stateList.Add("bird-fly", new State("bird-fly",
                c => sm.previousControlState != null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["bird-right"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["bird-right"];

                    /*if (BelowRange())
                    {
                        this.accel.Y = 0.3f;
                        animationList["bird-right"].frameInterval = TimeSpan.FromMilliseconds(20);
                    }
                    else if (AboveRange())
                    {
                        this.accel.Y = 0.2f;
                        animationList["bird-right"].frameInterval = TimeSpan.FromMilliseconds(80);
                    }
                    else
                    {
                        this.accel.Y = 0.25f;
                        animationList["bird-right"].frameInterval = TimeSpan.FromMilliseconds(40);
                    }

                    if (LeftOfRange())
                    {
                        facingDir = Direction.Right;
                        this.accel.X = 0.05f;
                    }
                    else if (RightOfRange())
                    {
                        facingDir = Direction.Left;
                        this.accel.X = -0.05f;
                    }
                    else
                    {
                        this.accel.X = 0f;
                    }*/

                    this.velocity.X = 0f;
                    this.velocity.Y = 0f;
                    this.accel.X = 0f;
                    this.accel.Y = 0f;
                    

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y < 0f)
                        {
                            velocity.Y = 0f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        //ApplyGravity();
                    }

                    //BirdEnforceMaxSpeed();

                    if (hp <= 0)
                    {
                        _cf.AddCharacterToList(new SmallExplosion(_cf._cm, _cf._gd,
                            this.pos + new Vector2(-35f, -25f),
                            _cf));
                        readyForRemoval = true;
                    }
                }));

            sm = new StateMachine(stateList);
        }

        private bool BelowRange()
        {
            if (pos.Y < targetPos.Y - range)
                return true;
            else
                return false;
        }

        private bool AboveRange()
        {
            if (pos.Y > targetPos.Y + range)
                return true;
            else
                return false;
        }

        private bool LeftOfRange()
        {
            if (pos.X < targetPos.X - range)
                return true;
            else
                return false;
        }

        private bool RightOfRange()
        {
            if (pos.X > targetPos.X + range)
                return true;
            else
                return false;
        }

        private void BirdEnforceMaxSpeed()
        {
            if (velocity.X > maxSpeed)
                velocity.X = maxSpeed;
            if (velocity.X < -maxSpeed)
                velocity.X = -maxSpeed;
            if (velocity.Y > maxSpeed)
                velocity.Y = maxSpeed;
            if (velocity.Y < -maxSpeed)
                velocity.Y = -maxSpeed;
        }

        public override void TakeDamage(int dmgAmount, float force, Direction atkDir)
        {
            //timer.Enabled = false;

            hp -= dmgAmount;
            hurtFlash = true;

            _cf._damageNumbersList.Add(new DamageNumber((int)((dmgAmount * 50) + (random.NextDouble() - 0.5) * 4), pos + offset));

            if (atkDir == Direction.Left)
                this.facingDir = Direction.Right;
            else
                this.facingDir = Direction.Left;

            if (hp > 0)
            {

                if (facingDir.Equals(Direction.Left))
                {
                    this.velocity.X = 2f * force;
                }
                else
                {
                    this.velocity.X = -2f * force;
                }

                this.velocity.Y = 3f;

                //sm.currentControlState = sm.control["rhino-hit"];
                //sm.previousControlState = sm.control["rhino-hit"];

                //StartDamageTimer(force);
            }
        }
    }
}
