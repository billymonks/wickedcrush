using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Timers;
using WickedCrush.Utility;
using Microsoft.Xna.Framework.Audio;

namespace WickedCrush.GameEntities
{
    public class TreeMob : Character
    {
        private CharacterFactory _cf;
        private Hero _hero;
        private Timer atimer, ttimer, dtimer;
        SoundManager _sound;

        private AudioEmitter emitter;

        private Random random;

        private float walkSpeed = 1.5f;

        public TreeMob(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf, Hero hero, Direction d, SoundManager sound)
            : base(pos, new Vector2(72f, 100f), new Vector2(0.8f, 0.8f), new Vector2(-7.2f, -2f), 128f, gd)
        {
            _cf = cf;
            _hero = hero;
            _sound = sound;

            facingDir = d;

            CreateCharacter(cm);

            random = new Random();
            
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 4;
            name = "TreeMob";
            CreateAnimationList(cm);
            CreateStateMachine();
            SetupSounds();

            //facingDir = Direction.Left;

            //StartAttackTimer();
        }

        private void SetupSounds()
        {
            emitter = new AudioEmitter();
            
            _sound.addSound("flameout", "fireshot");
        }

        protected override void UpdateSoundPosition()
        {
            base.UpdateSoundPosition();
            if(emitter!=null)
                emitter.Position = centerPoint;
        }

        public override void StopSoundInstances()
        {
        }

        private void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("tree-idle", new Animation("tree-idle", cm));
            animationList.Add("tree-walk", new Animation("tree-walk", cm));
            animationList.Add("tree-attack", new Animation("tree-attack", cm));
            animationList.Add("tree-hit", new Animation("tree-hit", cm));
            animationList["tree-hit"].loop = true;
            animationList["tree-attack"].loop = false;

            defaultTexture = animationList["tree-hit"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("tree-load", new State("tree-load",
                c => sm.previousControlState == null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-idle"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["tree-idle"];
                    this.velocity.X = 0f;
                    this.velocity.Y = 0f;
                    this.accel.X = 0f;
                    this.accel.Y = 0f;
                }));
            stateList.Add("tree-walk", new State("tree-walk",
                c => !sm.previousControlState.name.Equals("tree-hit")
                    && !sm.previousControlState.name.Equals("tree-attack")
                    && !sm.previousControlState.name.Equals("tree-turn")
                    && !sm.previousControlState.name.Equals("tree-attack-tell"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-walk"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["tree-walk"];

                    if (facingDir.Equals(Direction.Left))
                    {
                        this.velocity.X = -walkSpeed;
                    }
                    else
                    {
                        this.velocity.X = walkSpeed;
                    }

                    if (CheckYDistance(_hero) < 44f)
                    {
                        if (CheckXDisplacement(_hero) < 0f)
                        {
                            if (this.facingDir.Equals(Direction.Right) && CheckXDistance(_hero) < 456f)
                                StartAttackTimer();
                        }
                        else if (this.facingDir.Equals(Direction.Left) && CheckXDistance(_hero) < 456f)
                        {
                            StartAttackTimer();
                        }
                    }

                    if (facingDir.Equals(Direction.Left) && ((!platformLeft && platformRight) || leftWallHit))
                    {
                        StartTurnTimer();
                    }

                    if (facingDir.Equals(Direction.Right) &&((!platformRight && platformLeft) || rightWallHit))
                    {
                        StartTurnTimer();
                    }

                    //timer.Enabled = false;

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
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));
            //turn around
            stateList.Add("tree-turn", new State("tree-turn",
                c => sm.previousControlState.name.Equals("tree-turn"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-idle"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["tree-idle"];
                    
                    this.velocity.X = 0f;

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (CheckYDistance(_hero) < 44f)
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
                    }

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
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));
            stateList.Add("tree-attack-tell", new State("tree-attack-tell",
                c => sm.previousControlState.name.Equals("tree-attack-tell"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-idle"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["tree-idle"];

                    this.velocity.X = 0f;

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
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        Die();
                    }

                }));
            stateList.Add("tree-hit", new State("tree-hit",
                c => sm.previousControlState.name.Equals("tree-hit"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-hit"]))
                    {
                        currentAnimation.ResetAnimation();
                    }
                    
                    currentAnimation = animationList["tree-hit"];

                    

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (underFeetCollisionList.Count > 0)
                    {
                        if (velocity.Y < 0f)
                        {
                            velocity.Y *= -1f;
                            this.pos.Y = GetHighestSensorPoint();
                        }
                    }
                    else
                    {
                        ApplyGravity();
                    }
                    
                }));
            stateList.Add("tree-attack", new State("tree-attack",
                c => sm.previousControlState.name.Equals("tree-attack"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["tree-attack"]))
                        currentAnimation.ResetAnimation();
                    currentAnimation = animationList["tree-attack"];

                    this.velocity.X = 0f;
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
                        ApplyGravity();
                    }

                    if (hp <= 0)
                    {
                        _cf.AddCharacterToList(new SmallExplosion(_cf._cm, _cf._gd,
                            this.pos + new Vector2(-35f, -25f),
                            _cf));
                        readyForRemoval = true;
                        StopAllTimers();
                    }

                    if (currentAnimation.complete)
                    {
                        sm.currentControlState = sm.control["tree-walk"];
                        sm.previousControlState = sm.control["tree-walk"];
                    }
                }));

            sm = new StateMachine(stateList);

        }

        private void Die()
        {
            StopSoundInstances();

            _cf.AddCharacterToList(new SmallExplosion(_cf._cm, _cf._gd,
                            this.pos + new Vector2(-35f, -25f),
                            _cf));
            readyForRemoval = true;
            StopAllTimers();
        }

        private void StopAllTimers()
        {
            if (ttimer != null)
                ttimer.Stop();
            if (atimer != null)
                atimer.Stop();
            if (dtimer != null)
                dtimer.Stop();
        }

        public override void TakeDamage(int dmgAmount, float force, Direction atkDir)
        {
            hp -= dmgAmount;
            hurtFlash = true;

            _cf._damageNumbersList.Add(new DamageNumber((int)((dmgAmount * 50) + (random.NextDouble()-0.5) * 4), pos+offset));

            if (atkDir == Direction.Left)
                this.facingDir = Direction.Right;
            else
                this.facingDir = Direction.Left;

            //ttimer.Stop();

            if (hp > 0)
            {
                StartDamageTimer(force);

                if (facingDir.Equals(Direction.Left))
                {
                    this.velocity.X = 2f * force;
                }
                else
                {
                    this.velocity.X = -2f * force;
                }
                
                this.velocity.Y = 3f;

                sm.currentControlState = sm.control["tree-hit"];
                sm.previousControlState = sm.control["tree-hit"];
            }
        }

        private void damageTimerUp(object source, ElapsedEventArgs e)
        {
            dtimer.Enabled = false;
            
            sm.currentControlState = sm.control["tree-walk"];
            sm.previousControlState = sm.control["tree-walk"];

            //StartAttackTimer();
        }

        private void attackTimerUp(object source, ElapsedEventArgs e)
        {
            atimer.Enabled = false;
            ShootFireball();
            //StartAttackTimer();
        }

        private void ShootFireball()
        {
            sm.currentControlState = sm.control["tree-attack"];
            sm.previousControlState = sm.control["tree-attack"];

            SoundEffectInstance flameout = _sound.getSoundInstance("flameout");
            flameout.Apply3D(_sound.listener, emitter);
            flameout.Play();

            if (facingDir.Equals(Direction.Left))
            {
                _cf.AddCharacterToList(
                        new Fireball(_cf._cm, _cf._gd,
                            pos + new Vector2(0f, 16f),
                            1,
                            1.5f,
                            facingDir,
                            this,
                            _cf));
            }
            else
            {
                _cf.AddCharacterToList(
                        new Fireball(_cf._cm, _cf._gd,
                            pos + new Vector2(50f, 16f),
                            1,
                            1.5f,
                            facingDir,
                            this,
                            _cf));
            }
        }

        private void turnTimerUp(object source, ElapsedEventArgs e)
        {
            ttimer.Enabled = false;

            if (facingDir.Equals(Direction.Right))
                facingDir = Direction.Left;
            else
                facingDir = Direction.Right;

            sm.currentControlState = sm.control["tree-walk"];
            sm.previousControlState = sm.control["tree-walk"];
        }

        private void StartDamageTimer(float force)
        {
            if (dtimer != null)
                dtimer.Enabled = false;

            dtimer = new Timer();
            dtimer.Elapsed += new ElapsedEventHandler(damageTimerUp);
            dtimer.Interval = 400 * force;
            dtimer.Enabled = true;
        }

        private void StartAttackTimer()
        {
            if (atimer != null)
                atimer.Enabled = false;

            sm.currentControlState = sm.control["tree-attack-tell"];
            sm.previousControlState = sm.control["tree-attack-tell"];

            atimer = new Timer();
            atimer.Elapsed += new ElapsedEventHandler(attackTimerUp);
            atimer.Interval = 800;
            atimer.Enabled = true;
        }
        private void StartTurnTimer()
        {
            if (ttimer != null)
                ttimer.Enabled = false;

            sm.currentControlState = sm.control["tree-turn"];
            sm.previousControlState = sm.control["tree-turn"];

            ttimer = new Timer();
            ttimer.Elapsed += new ElapsedEventHandler(turnTimerUp);
            ttimer.Interval = 800;
            ttimer.Enabled = true;
        }
    }
}
