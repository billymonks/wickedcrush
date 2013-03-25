using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class Rhino : Character
    {
        private CharacterFactory _cf;
        private Hero _hero;
        private Timer timer;

        private Random random;

        private float walkSpeed = 4f;

        public Rhino(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf, Hero hero, Direction d)
            : base(pos, new Vector2(256f, 256f), new Vector2(0.45f, 0.514f), new Vector2(-70.4f, -43f), 128f, gd)
        {
            _cf = cf;
            _hero = hero;

            facingDir = d;

            CreateCharacter(cm);

            random = new Random();
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 8;
            name = "Rhino";
            CreateAnimationList(cm);
            CreateStateMachine();

            //facingDir = Direction.Left;
        }

        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("rhino-idle", new Animation("rhino-idle", cm));
            animationList.Add("rhino-attack", new Animation("rhino-attack", cm));
            animationList["rhino-attack"].loop = false;

            animationList.Add("rhino-tell-attack", new Animation("rhino-tell-attack", cm));
            animationList["rhino-tell-attack"].loop = false;

            animationList.Add("rhino-hit", new Animation("rhino-hit", cm));
            animationList.Add("rhino-jump", new Animation("rhino-jump", cm));
            animationList.Add("rhino-run", new Animation("rhino-run", cm));

            defaultTexture = animationList["rhino-jump"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();

            stateList.Add("rhino-load", new State("rhino-load",
                c => sm.previousControlState == null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-idle"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-idle"];
                    this.velocity.X = 0f;
                    this.velocity.Y = 0f;
                    this.accel.X = 0f;
                    this.accel.Y = 0f;
                }));

            stateList.Add("rhino-run", new State("rhino-run",
                c => (sm.previousControlState.name.Equals("rhino-idle")
                    || sm.previousControlState.name.Equals("rhino-run")
                    || sm.previousControlState.name.Equals("rhino-jump"))
                && (underFeetCollisionList.Count > 0)
                && (CheckXDistance(_hero) < 512f)
                && (CheckYDistance(_hero) < 128f)
                && ((CheckXDisplacement(_hero) < 0f && facingDir.Equals(Direction.Right))
                || (CheckXDisplacement(_hero) > 0f && facingDir.Equals(Direction.Left))),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-run"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-run"];

                    timer.Enabled = false;

                    if (facingDir.Equals(Direction.Left))
                    {
                        this.velocity.X = -walkSpeed;
                    }
                    else
                    {
                        this.velocity.X = walkSpeed;
                    }

                    /*if (CheckYDistance(_hero) < 44f)
                    {
                        if (CheckXDisplacement(_hero) < 0f)
                        {
                            if (this.facingDir.Equals(Direction.Right) && CheckXDistance(_hero) < 256f)
                                StartAttackTimer();
                        }
                        else if (this.facingDir.Equals(Direction.Left) && CheckXDistance(_hero) < 256f)
                        {
                            StartAttackTimer();
                        }
                    }*/

                    if (CheckXDistance(_hero) < 96f)
                    {
                        sm.currentControlState = sm.control["rhino-tell-attack"];
                        sm.previousControlState = sm.control["rhino-tell-attack"];
                    } else if ((facingDir.Equals(Direction.Left) && (!platformLeft || leftWallHit))
                        || (facingDir.Equals(Direction.Right) && (!platformRight || rightWallHit)))
                    {
                        Jump();
                    }

                    //timer.Enabled = false;

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y <= 0f)
                        {
                            velocity.Y = 0f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));

            stateList.Add("rhino-jump", new State("rhino-jump",
                c => (sm.previousControlState.name.Equals("rhino-run")
                    || sm.previousControlState.name.Equals("rhino-jump"))
                && (underFeetCollisionList.Count == 0),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-jump"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-jump"];

                    timer.Enabled = false;

                    if (facingDir.Equals(Direction.Left))
                    {
                        this.velocity.X = -walkSpeed;
                    }
                    else
                    {
                        this.velocity.X = walkSpeed;
                    }

                    this.accel.X = 0f;
                    this.accel.Y = 0f;


                    ApplyGravity();

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));

            stateList.Add("rhino-idle", new State("rhino-idle",
                c => !sm.previousControlState.name.Equals("rhino-hit")
                && !sm.previousControlState.name.Equals("rhino-tell-attack")
                && !sm.previousControlState.name.Equals("rhino-attack"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-idle"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-idle"];

                    if(!sm.previousControlState.name.Equals("rhino-idle"))
                        StartTurnTimer();

                    this.velocity.X = 0f;

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y <= 0f)
                        {
                            velocity.Y = 0f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));

            

            

            stateList.Add("rhino-hit", new State("rhino-hit",
                c => sm.previousControlState.name.Equals("rhino-hit"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-hit"]))
                    {
                        currentAnimation.ResetAnimation();
                    }
                    currentAnimation = animationList["rhino-hit"];

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y <= 0f)
                        {
                            velocity.Y *= -1f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));

            stateList.Add("rhino-tell-attack", new State("rhino-tell-attack",
                c => sm.previousControlState.name.Equals("rhino-tell-attack"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-tell-attack"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-tell-attack"];

                    timer.Enabled = false;

                    this.velocity.X = 0f;

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y <= 0f)
                        {
                            velocity.Y = 0f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }

                    if (currentAnimation.complete)
                    {
                        sm.currentControlState = sm.control["rhino-attack"];
                        sm.previousControlState = sm.control["rhino-attack"];
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));

            stateList.Add("rhino-attack", new State("rhino-attack",
                c => sm.previousControlState.name.Equals("tree-attack"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["rhino-attack"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["rhino-attack"];

                    timer.Enabled = false;

                    this.velocity.X = 0f;
                    this.accel.Y = 0f;


                    if (currentAnimation.getCurrentFrameNumber() <= 3)
                    {
                        if (facingDir.Equals(Direction.Right))
                            this.velocity.X = 2f;
                        else
                            this.velocity.X = -2f;

                        foreach (Entity ch in collisionList)
                        {
                            if (ch.type.Equals(EntType.Character))
                            {
                                if (!ch.invuln
                                    && ((CheckXDisplacement((Character)ch) < 0f && facingDir.Equals(Direction.Right))
                                    || (CheckXDisplacement((Character)ch) > 0f && facingDir.Equals(Direction.Left))))
                                {
                                    ((Character)ch).TakeDamage(2, 3f, facingDir);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.velocity.X = 0f;
                    }

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y <= 0f)
                        {
                            velocity.Y = 0f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        _cf.AddCharacterToList(new SmallExplosion(_cf._cm, _cf._gd,
                            this.pos + new Vector2(-35f, -25f),
                            _cf));
                        readyForRemoval = true;
                        timer.Stop();
                    }

                    if (currentAnimation.complete)
                    {
                        StartTurnTimer();

                        sm.currentControlState = sm.control["rhino-idle"];
                        sm.previousControlState = sm.control["rhino-idle"];
                    }
                }));

            sm = new StateMachine(stateList);
        }

        public override void TakeDamage(int dmgAmount, float force, Direction atkDir)
        {
            timer.Enabled = false;

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

                sm.currentControlState = sm.control["rhino-hit"];
                sm.previousControlState = sm.control["rhino-hit"];

                StartDamageTimer(force);
            }
        }

        private void Die()
        {
            _cf.AddCharacterToList(new BigExplosion(_cf._cm, _cf._gd,
                            this.pos + this.offset,
                            _cf));
            readyForRemoval = true;
            timer.Stop();
        }

        private void Jump()
        {
            velocity.Y = 7f;
            sm.currentControlState = sm.control["rhino-jump"];
            sm.previousControlState = sm.control["rhino-jump"];
        }

        private void StartTurnTimer()
        {
            if (timer != null)
                timer.Enabled = false;

            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(turnTimerUp);
            timer.Interval = 1600;
            timer.Enabled = true;
        }

        private void StartDamageTimer(float force)
        {
            if (timer != null)
                timer.Enabled = false;

            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(damageTimerUp);
            timer.Interval = 400 * force;
            timer.Enabled = true;
        }

        private void damageTimerUp(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;

            StartTurnTimer();

            sm.currentControlState = sm.control["rhino-idle"];
            sm.previousControlState = sm.control["rhino-idle"];

            //StartAttackTimer();
        }

        private void turnTimerUp(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;

            if (facingDir.Equals(Direction.Right))
                facingDir = Direction.Left;
            else
                facingDir = Direction.Right;

            StartTurnTimer();
        }
    }
}
