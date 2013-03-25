using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using WickedCrush.GameEntities;
using System.Timers;
using WickedCrush.Utility;
using Microsoft.Xna.Framework.Audio;

namespace WickedCrush.GameEntities
{
    public class Hero : Character
    {
        public ControlsManager _controls;
        private CharacterFactory _cf;
        SoundManager _sound;

        private AudioEmitter emitter;

        private bool attackDeployable = true; //switches to false during attacking states once the attack character has been deployed so it won't deploy multiple times per frame
        private bool blocking = false;
        private Timer timer;
        private bool attackBuffered = false;

        private float runspeed = 5f;
        private float blockwalkspeed = 1.8f;
        private float jump_amount = 10f;

        private SoundEffectInstance walkingSound;
        private SoundEffectInstance swordSound1;
        private SoundEffectInstance swordSound2;
        private SoundEffectInstance swordSound3;

        public Hero(ContentManager cm, GraphicsDevice gd, Vector2 pos, CharacterFactory cf, SoundManager sound)
            : base(pos, new Vector2(128f, 129.1f), new Vector2(0.45f, 0.9f), new Vector2(-35.2f, -2f), 128.5f, gd)
        {
            _cf = cf;
            _sound = sound;
            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 3;
            name = "Hero";
            CreateAnimationList(cm);
            CreateStateMachine();
            SetupSounds();

            facingDir = Direction.Right;
            variableSize = true;
        }

        public void RestoreHero()
        {
            hp = 3;
            //CreateAnimationList(cm);
            //CreateStateMachine();
            sm.previousControlState = null;
            pos = new Vector2(-256f, 800f);
        }

        private void SetupSounds()
        {
            emitter = new AudioEmitter();

            _sound.addSound("footsteps", "fart");
            _sound.addSound("swordSound1", "Arm Whoosh 02");
            _sound.addSound("swordSound2", "Arm Whoosh 03");
            _sound.addSound("swordSound3", "Arm Whoosh 10");

            walkingSound = _sound.getSoundInstance("footsteps");
            walkingSound.IsLooped = true;

            swordSound1 = _sound.getSoundInstance("swordSound1");
            swordSound1.IsLooped = false;

            swordSound2 = _sound.getSoundInstance("swordSound2");
            swordSound2.IsLooped = false;

            swordSound3 = _sound.getSoundInstance("swordSound3");
            swordSound3.IsLooped = false;
        }

        private void CreateAnimationList(ContentManager cm)
        {
            Animation temp;
            animationList = new Dictionary<String, Animation>();
            //animationList.Add("rad-walk-right", new Animation("rad-walk-right", cm));
            //animationList.Add("rad-idle-right", new Animation("rad-idle-right", cm));
            //animationList.Add("rad-jump-up-right", new Animation("rad-jump-up-right", cm));
            //animationList.Add("rad-jump-down-right", new Animation("rad-jump-down-right", cm));

            //temp = new Animation("assplosion", cm);
            //temp.loop = false;

            //animationList.Add("explosion", temp);


            

            animationList.Add("block", new Animation("boy_hero_block", cm)); // block start
            animationList["block"].loop = false;
            animationList["block"].frameInterval = TimeSpan.FromMilliseconds(30);

            animationList.Add("block_held", new Animation("boy_hero_block_held", cm));

            animationList.Add("block_walk_bkw", new Animation("boy_hero_block_walk_bkw", cm));
            animationList.Add("block_walk_fwd", new Animation("boy_hero_block_walk_fwd", cm));
            animationList.Add("fall", new Animation("boy_hero_fall", cm));
            animationList.Add("idle", new Animation("boy_hero_idle", cm));
            animationList.Add("jump", new Animation("boy_hero_jump", cm));
            animationList.Add("melee_1", new Animation("boy_hero_melee_1", cm));
            animationList["melee_1"].loop = false;

            animationList.Add("melee_1_post", new Animation("boy_hero_melee_1_post", cm));
            animationList.Add("melee_2", new Animation("boy_hero_melee_2", cm));
            animationList["melee_2"].loop = false;

            animationList.Add("melee_2_post", new Animation("boy_hero_melee_2_post", cm));
            animationList.Add("melee_3", new Animation("boy_hero_melee_3", cm));
            animationList["melee_3"].loop = false;

            animationList.Add("melee_3_post", new Animation("boy_hero_melee_3_post", cm));
            animationList.Add("melee_3_recovery", new Animation("boy_hero_melee_3_recovery", cm));
            animationList["melee_3_recovery"].loop = false;

            animationList.Add("aerial_attack", new Animation("boy_hero_aerial_attack", cm));
            animationList["aerial_attack"].loop = false;

            animationList.Add("hurt", new Animation("boy_hero_hurt", cm));

            animationList.Add("run", new Animation("boy_hero_run", cm));

            temp = new Animation("boy_hero_run_melee_tell", cm);
            temp.loop = false;

            animationList.Add("run_melee_tell", temp);

            temp = new Animation("boy_hero_stand_melee_tell", cm);
            temp.loop = false;

            animationList.Add("stand_melee_tell", temp);

            defaultTexture = animationList["idle"].animationSheet;
        }

        private void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();

            stateList.Add("load", new State("load",
                c => sm.previousControlState == null,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["idle"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["idle"];
                    this.velocity.X = 0f;

                    attackDeployable = true;
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("idle", new State("idle",
                c => underFeetCollisionList.Count > 0
                && !sm.previousControlState.name.Equals("prep_block")
                && !sm.previousControlState.name.Equals("block_hit_deflect")
                && !sm.previousControlState.name.Contains("melee")
                && !sm.previousControlState.name.Equals("aerial_attack")
                && !sm.previousControlState.name.Equals("hurt")
                && _controls.XAxis() == 0f
                && !_controls.JumpPressed()
                && !_controls.ActionPressed()
                && (!_controls.BlockHeld()
                || sm.previousControlState.name.Equals("fall"))
                && velocity.Y <= 0f,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["idle"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["idle"];

                    
                    this.velocity.X = 0f;

                    if (velocity.Y <= 0f)
                    {
                        velocity.Y = 0f;
                        this.pos.Y = GetHighestSensorPoint();
                    }

                    this.accel.X = 0f;
                    this.accel.Y = 0f;
                    blocking = false;

                    walkingSound.Stop();

                    if (hp <= 0)
                        Die();

                }));
            stateList.Add("run", new State("run",
                c => underFeetCollisionList.Count > 0
                    && !sm.previousControlState.name.Equals("prep_block")
                    && !sm.previousControlState.name.Equals("block_hit_deflect")
                    && !sm.previousControlState.name.Contains("melee")
                    && !sm.previousControlState.name.Equals("aerial_attack")
                    && !sm.previousControlState.name.Equals("hurt")
                    && Math.Abs(_controls.XAxis()) > 0f
                    && !_controls.JumpPressed()
                    && !_controls.ActionPressed()
                    && (!_controls.BlockHeld()
                    || sm.previousControlState.name.Equals("fall"))
                    && velocity.Y <= 0f,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["run"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["run"];

                    if (_controls.XAxis() > 0f)
                        this.facingDir = Direction.Right;
                    else
                        this.facingDir = Direction.Left;

                    this.velocity.X = _controls.XAxis() * runspeed;

                    if (velocity.Y <= 0f)
                    {
                        velocity.Y = 0f;
                        this.pos.Y = GetHighestSensorPoint();
                    }

                    this.accel.Y = 0f;
                    blocking = false;

                    //walkingSound.Apply3D(_sound.listener, emitter);
                    //walkingSound.Play();

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("jump", new State("jump",
                c => ((underFeetCollisionList.Count > 0 && _controls.JumpPressed())
                || velocity.Y > 0f)
                && !_controls.JumpReleased()
                && !sm.previousControlState.name.Equals("block_hit_deflect")
                && !sm.previousControlState.name.Contains("melee")
                && !sm.previousControlState.name.Equals("aerial_attack")
                && !sm.previousControlState.name.Equals("hurt")
                && !_controls.ActionPressed()
                && this.hp > 0,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["jump"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["jump"];

                    if (!sm.previousControlState.name.Contains("jump"))
                        this.velocity.Y = jump_amount;

                    this.velocity.X = _controls.XAxis() * runspeed;

                    this.accel.Y = 0f;

                    ApplyGravity();
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("jump-cutoff", new State("jump-cutoff",
                c => underFeetCollisionList.Count == 0
                && !sm.previousControlState.name.Equals("block_hit_deflect")
                && !sm.previousControlState.name.Equals("aerial_attack")
                && !sm.previousControlState.name.Equals("hurt")
                && !_controls.ActionPressed()
                && velocity.Y > 0f
                && this.hp > 0,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["jump"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["jump"];

                    if (!sm.previousControlState.name.Contains("jump"))
                        this.velocity.Y = 7f;

                    if (velocity.Y > 3f)
                        velocity.Y = 3f;

                    this.velocity.X = _controls.XAxis() * runspeed;

                    this.accel.Y = 0f;

                    ApplyGravity();
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("fall", new State("fall",
                c => underFeetCollisionList.Count == 0
                && !sm.previousControlState.name.Equals("ru_melee_1")
                && !sm.previousControlState.name.Equals("st_melee_1")
                && !sm.previousControlState.name.Equals("melee_2")
                && !sm.previousControlState.name.Equals("melee_3")
                && !sm.previousControlState.name.Equals("block_hit_deflect")
                && !sm.previousControlState.name.Equals("aerial_attack")
                && !sm.previousControlState.name.Equals("hurt")
                && !_controls.ActionPressed(),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["fall"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["fall"];

                    

                    this.velocity.X = _controls.XAxis() * runspeed;

                    this.accel.Y = 0f;

                    ApplyGravity();
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));

            stateList.Add("aerial_attack", new State("aerial_attack",
                c => (((sm.previousControlState.name.Equals("fall")
                    || sm.previousControlState.name.Equals("jump-cutoff")
                    || sm.previousControlState.name.Equals("jump"))
                    && _controls.ActionPressed())
                    || sm.previousControlState.name.Equals("aerial_attack"))
                    && underFeetCollisionList.Count == 0,
                    c =>
                    {
                        if (currentAnimation != null && !currentAnimation.Equals(animationList["aerial_attack"]))
                        {
                            currentAnimation.ResetAnimation();
                            SetAerialAttackFrame();
                            attackDeployable = true;
                        }
                        currentAnimation = animationList["aerial_attack"];

                        this.velocity.X = _controls.XAxis() * runspeed;

                        this.accel.Y = 0f;

                        ApplyGravity();
                        blocking = false;

                        if (attackDeployable && currentAnimation.getCurrentFrameNumber() >= 1)
                        {
                            attackDeployable = false;

                            if (facingDir.Equals(Direction.Right)) // attack deployment
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X + 32, this.pos.Y - 100),
                                    new Vector2(128f, 192f),
                                    1,
                                    1f,
                                    true,
                                    this.facingDir,
                                    this));

                            }
                            else
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X - 96, this.pos.Y - 100), //no, i don't know why it's different, i mean i have an idea why but i don't really give a shittt
                                    new Vector2(128f, 192f),
                                    1,
                                    1f,
                                    true,
                                    this.facingDir,
                                    this));
                            } // end of attack deployment

                            //_sound.getSoundInstance("swordSound2").Play();

                            swordSound2 = _sound.getSoundInstance("swordSound2");
                            swordSound2.Apply3D(_sound.listener, emitter);
                            swordSound2.Play();

                        }

                        if (currentAnimation.complete)
                        {
                            sm.previousControlState = sm.control["idle"];
                            sm.currentControlState = sm.control["idle"];
                        }

                        if (hp <= 0)
                            Die();
                    }));


            stateList.Add("prep_block", new State("prep_block",
                c => ((sm.previousControlState.name.Equals("idle")
                || sm.previousControlState.name.Equals("run"))
                && _controls.BlockHeld())
                || sm.previousControlState.name.Equals("prep_block"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["block"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    } 
                    currentAnimation = animationList["block"];

                    if (velocity.Y <= 0f)
                    {
                        velocity.Y = 0f;
                        this.pos.Y = GetHighestSensorPoint();
                    }

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    if (currentAnimation.complete)
                    {
                        sm.previousControlState = sm.control["block"];
                        sm.currentControlState = sm.control["block"];
                    }
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("block", new State("block",
                c => (sm.previousControlState.name.Equals("block")
                || sm.previousControlState.name.Equals("block_walk"))
                && underFeetCollisionList.Count > 0
                && _controls.BlockHeld()
                && _controls.XAxis() == 0f,
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["block_held"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    }
                    currentAnimation = animationList["block_held"];

                    this.velocity.X = 0f;
                    if (velocity.Y <= 0f)
                    {
                        velocity.Y = 0f;
                        this.pos.Y = GetHighestSensorPoint();
                    }

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    blocking = true;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("block_hit_deflect", new State("block_hit_deflect",
                c => sm.previousControlState.name.Equals("block_hit_deflect"),
                c =>
                {

                    if (currentAnimation != null && !currentAnimation.Equals(animationList["block_held"])) // change it so it's like this
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    }
                    currentAnimation = animationList["block_held"];

                    if (velocity.X > 0.2f)
                    {
                        accel.X = -0.1f;
                    }
                    else if (velocity.X < -0.2f)
                    {
                        accel.X = 0.1f;
                    }
                    else
                    {
                        accel.X = 0f;
                        velocity.X = 0f;
                    }

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

                    blocking = true;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("block_walk", new State("block_walk",
                c => (sm.previousControlState.name.Equals("block")
                || sm.previousControlState.name.Equals("block_walk"))
                && underFeetCollisionList.Count > 0
                && _controls.XAxis() != 0f,
                c =>
                {
                    if (facingDir == Direction.Right)
                    {
                        if (_controls.XAxis() > 0f)
                        {
                            if (currentAnimation != null && !currentAnimation.Equals(animationList["block_walk_fwd"]))
                            {
                                currentAnimation.ResetAnimation();
                                SetSmallFrame();
                            } 
                            currentAnimation = animationList["block_walk_fwd"];
                        }
                        else
                        {
                            if (currentAnimation != null && !currentAnimation.Equals(animationList["block_walk_bkw"]))
                            {
                                currentAnimation.ResetAnimation();
                                SetSmallFrame();
                            } 
                            currentAnimation = animationList["block_walk_bkw"];
                        }
                    }
                    else
                    {
                        if (_controls.XAxis() > 0f)
                        {
                            if (currentAnimation != null && !currentAnimation.Equals(animationList["block_walk_bkw"]))
                            {
                                currentAnimation.ResetAnimation();
                                SetSmallFrame();
                            } 
                            currentAnimation = animationList["block_walk_bkw"];
                        }
                        else
                        {
                            if (currentAnimation != null && !currentAnimation.Equals(animationList["block_walk_fwd"]))
                            {
                                currentAnimation.ResetAnimation();
                                SetSmallFrame();
                            } 
                            currentAnimation = animationList["block_walk_fwd"];
                        }
                    }

                    this.velocity.X = _controls.XAxis() * blockwalkspeed;
                    if (velocity.Y <= 0f)
                    {
                        velocity.Y = 0f;
                        this.pos.Y = GetHighestSensorPoint();
                    }

                    this.accel.X = 0f;
                    this.accel.Y = 0f;

                    blocking = true;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("st_melee_1", new State("st_melee_1",
                c => ((sm.previousControlState.name.Equals("idle")
                    && _controls.ActionPressed())
                    || sm.previousControlState.name.Equals("st_melee_1")),
                c =>
                {
					if (!currentAnimation.Equals(animationList["melee_1"]))
                    {
                        if (currentAnimation != null && !currentAnimation.Equals(animationList["stand_melee_tell"]))
                        {
                            currentAnimation.ResetAnimation();
                            SetHugeFrame();
                        }
                        currentAnimation = animationList["stand_melee_tell"];
                        attackBuffered = false;
                        if (currentAnimation.complete)
                        {
                            currentAnimation.ResetAnimation();
                            currentAnimation = animationList["melee_1"];
                            
                            if (facingDir.Equals(Direction.Right)) // attack deployment
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X+48, this.pos.Y),
                                    new Vector2(192f, 96f),
                                    1,
                                    1f,
                                    this.facingDir,
                                    this));

                            }
                            else
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X - 173, this.pos.Y), //no, i don't know why it's different, i mean i have an idea why but i don't really give a shittt
                                    new Vector2(192f, 96f),
                                    1,
                                    1f,
                                    this.facingDir,
                                    this));
                            } // end of attack deployment

                            //_sound.getSoundInstance("swordSound1").Play();
                            swordSound1.Apply3D(_sound.listener, emitter);
                            swordSound1.Play();
                        }
                    }
                    else
                    {
                        if (_controls.ActionPressed())
                            attackBuffered = true;

                        if (currentAnimation.complete)
                        {
                            sm.currentControlState = sm.control["melee_1_post"];
                            sm.previousControlState = sm.control["melee_1_post"];
                        }
                    }

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
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("ru_melee_1", new State("ru_melee_1",
                c => ((sm.previousControlState.name.Equals("run")
                    && _controls.ActionPressed())
                    || sm.previousControlState.name.Equals("ru_melee_1")),
                c =>
                {
                    if (!currentAnimation.Equals(animationList["melee_1"]))
                    {
                        if (currentAnimation != null && !currentAnimation.Equals(animationList["run_melee_tell"]))
                        {
                            currentAnimation.ResetAnimation();
                            SetHugeFrame();
                        }
                        currentAnimation = animationList["run_melee_tell"];
                        attackBuffered = false;
                        if (currentAnimation.complete)
                        {
                            currentAnimation.ResetAnimation();
                            currentAnimation = animationList["melee_1"];

                            if (facingDir.Equals(Direction.Right)) // attack deployment
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X + 48, this.pos.Y),
                                    new Vector2(192f, 96f),
                                    1,
                                    1f,
                                    this.facingDir,
                                    this));

                            }
                            else
                            {
                                _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                    new Vector2(this.pos.X - 173, this.pos.Y), //no, i don't know why it's different, i mean i have an idea why but i don't really give a shittt
                                    new Vector2(192f, 96f),
                                    1,
                                    1f,
                                    this.facingDir,
                                    this));
                            } // end of attack deployment

                            swordSound1.Apply3D(_sound.listener, emitter);
                            swordSound1.Play();
                            //_sound.getSoundInstance("swordSound1").Play();
                        }
                    }
                    else
                    {
                        if (_controls.ActionPressed())
                            attackBuffered = true;

                        if (currentAnimation.complete)
                        {
                            sm.currentControlState = sm.control["melee_1_post"];
                            sm.previousControlState = sm.control["melee_1_post"];
                        }
                    }

                    if (velocity.X > 0.2f)
                    {
                        accel.X = -0.1f;
                    }
                    else if (velocity.X < -0.2f)
                    {
                        accel.X = 0.1f;
                    }
                    else
                    {
                        accel.X = 0f;
                        velocity.X = 0f;
                    }
                    
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
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("melee_1_post", new State("melee_1_post", // attack during post triggers melee_2
                c => (sm.previousControlState.name.Equals("melee_1_post")
                    && (!_controls.ActionPressed() && !attackBuffered)),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["melee_1_post"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetHugeFrame();

                        setPostAttackHandler(100);
                    }
                    currentAnimation = animationList["melee_1_post"];

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
                    blocking = false;
                }));
            stateList.Add("melee_2", new State("melee_2",
                c => ((sm.previousControlState.name.Equals("melee_1_post")
                    && (_controls.ActionPressed() || attackBuffered))
                    || sm.previousControlState.name.Equals("melee_2")),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["melee_2"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetHugeFrame();
                        if(timer!=null)
                            timer.Enabled = false;
                        attackDeployable = true;
                        attackBuffered = false;
                    }
                    currentAnimation = animationList["melee_2"];

                    if (currentAnimation.complete)
                    {
                        sm.currentControlState = sm.control["melee_2_post"];
                        sm.previousControlState = sm.control["melee_2_post"];
                    }

                    if (attackDeployable && currentAnimation.getCurrentFrameNumber() >= 1)
                    {
                        attackDeployable = false;

                        if (facingDir.Equals(Direction.Right)) // attack deployment
                        {
                            _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                new Vector2(this.pos.X + 48, this.pos.Y),
                                new Vector2(192f, 96f),
                                1,
                                1f,
                                this.facingDir,
                                this));

                        }
                        else
                        {
                            _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                new Vector2(this.pos.X - 173, this.pos.Y), //no, i don't know why it's different, i mean i have an idea why but i don't really give a shittt
                                new Vector2(192f, 96f),
                                1,
                                1f,
                                this.facingDir,
                                this));
                        } // end of attack deployment

                        swordSound2.Apply3D(_sound.listener, emitter);
                        swordSound2.Play();
                        //_sound.getSoundInstance("swordSound2").Play();
                    }

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
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("melee_2_post", new State("melee_2_post", // attack during post triggers melee_3
                c => (sm.previousControlState.name.Equals("melee_2_post")
                    && !_controls.ActionPressed()),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["melee_2_post"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetHugeFrame();

                        setPostAttackHandler(200);
                    }
                    currentAnimation = animationList["melee_2_post"];



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
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("melee_3", new State("melee_3",
                c => ((sm.previousControlState.name.Equals("melee_2_post")
                    && _controls.ActionPressed())
                    || sm.previousControlState.name.Equals("melee_3")),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["melee_3"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetHugeFrame();
                        if (timer != null)
                            timer.Enabled = false;
                        attackDeployable = true;
                    }
                    currentAnimation = animationList["melee_3"];

                    if (attackDeployable && currentAnimation.getCurrentFrameNumber() >= 7)
                    {
                        attackDeployable = false;

                        if (facingDir.Equals(Direction.Right)) // attack deployment
                        {
                            _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                new Vector2(this.pos.X + 48, this.pos.Y + 32),
                                new Vector2(192f, 64f),
                                2,
                                2f,
                                this.facingDir,
                                this));

                        }
                        else
                        {
                            _cf.AddCharacterToList(new Attack(_cf._cm, _cf._gd,
                                new Vector2(this.pos.X - 173, this.pos.Y + 32), //no, i don't know why it's different, i mean i have an idea why but i don't really give a shittt
                                new Vector2(192f, 64f),
                                2,
                                2f,
                                this.facingDir,
                                this));
                        } // end of attack deployment

                        //swordSound3.Play();
                        swordSound3.Apply3D(_sound.listener, emitter);
                        swordSound3.Play();
                        //_sound.getSoundInstance("swordSound3").Play();
                    }

                    if (currentAnimation.getCurrentFrameNumber() <= 7)
                    {
                        if(facingDir.Equals(Direction.Right))
                            this.velocity.X = 3f;
                        else
                            this.velocity.X = -3f;
                    }
                    else
                    {
                        this.velocity.X = 0f;
                    }

                    

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
                        sm.currentControlState = sm.control["melee_3_recovery"];
                        sm.previousControlState = sm.control["melee_3_recovery"];
                    }
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("melee_3_recovery", new State("melee_3_recovery",
                c => sm.previousControlState.name.Equals("melee_3_recovery"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["melee_3_recovery"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetHugeFrame();
                    }
                    currentAnimation = animationList["melee_3_recovery"];

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

                    if(currentAnimation.complete)
                    {
                        sm.currentControlState = sm.control["idle"];
                        sm.previousControlState = sm.control["idle"];
                    }
                    blocking = false;

                    if (hp <= 0)
                        Die();
                }));
            stateList.Add("hurt", new State("hurt",
                c => sm.previousControlState.name.Equals("hurt"),
                c =>
                {
                    if (currentAnimation != null && !currentAnimation.Equals(animationList["hurt"]))
                    {
                        currentAnimation.ResetAnimation();
                        SetSmallFrame();
                    }
                    currentAnimation = animationList["hurt"];

                    if (this.facingDir.Equals(Direction.Left))
                    {
                        this.velocity.X = 3f;
                    }
                    else
                    {
                        this.velocity.X = -3f;
                    }
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

                }));
            stateList.Add("dead", new State("dead",
                c => true,
                c =>
                {

                }));

            sm = new StateMachine(stateList);
            
        }

        protected override void UpdateSoundPosition()
        {
            base.UpdateSoundPosition();

            emitter.Position = centerPoint;

            walkingSound.Apply3D(_sound.listener, emitter);
        }

        public override void StopSoundInstances()
        {
            walkingSound.Stop();
            //swordSound1.Stop();
            //swordSound2.Stop();
            //swordSound3.Stop();
        }

        public override void AttackCallback()
        {
            base.AttackCallback();

            this.velocity.Y = jump_amount;
        }

        private void Die() //can't be removing the hero, need death animation
        {
            StopSoundInstances();

            SetSmallFrame();

            _cf.AddCharacterToList(new BigExplosion(_cf._cm, _cf._gd,
                            this.pos + this.offset - new Vector2(64f, 64f),
                            _cf));
            readyForRemoval = true;
            //timer.Stop();
        }

        public override void TakeDamage(int dmgAmount, float force, Direction atkDir)
        {
            Random random = new Random();
            if (sm.currentControlState.name.Equals("block_hit_deflect")
                || sm.currentControlState.name.Equals("hurt"))
            {
                blockFlash = true;
                return;
            }

            if (blocking && (!atkDir.Equals(this.facingDir)))
            {
                blockFlash = true;
                if(timer!=null)
                    timer.Enabled = false;
                timer = new Timer();
                timer.Elapsed += new ElapsedEventHandler(blockTimerUp);

                timer.Interval = 200 * force;
                timer.Enabled = true;

                _cf._damageNumbersList.Add(new DamageNumber(0, pos + offset));

                if (atkDir.Equals(Direction.Left))
                    velocity.X = -2f * force;
                else
                    velocity.X = 2f * force;

                sm.currentControlState = sm.control["block_hit_deflect"];
                sm.previousControlState = sm.control["block_hit_deflect"];
                return;
            }
            else
            {
                hurtFlash = true;
                blocking = false;
                //timer.Enabled = false;

                hp -= dmgAmount;

                _cf._damageNumbersList.Add(new DamageNumber((int)((dmgAmount * 50) + (random.NextDouble() - 0.5) * 4), pos + offset));

                if (atkDir == Direction.Left)
                    this.facingDir = Direction.Right;
                else
                    this.facingDir = Direction.Left;

                if (hp > 0)
                {
                    if (timer != null)
                        timer.Enabled = false;
                    timer = new Timer();
                    timer.Elapsed += new ElapsedEventHandler(damageTimerUp);

                    timer.Interval = 600;
                    timer.Enabled = true;

                    sm.currentControlState = sm.control["hurt"];
                    sm.previousControlState = sm.control["hurt"];
                }
            }
        }

        private void setPostAttackHandler(double interval)
        {
            if (timer != null)
                timer.Enabled = false;

            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(attackTimerUp);

            timer.Interval = interval;
            timer.Enabled = true;
            return;
        }

        private void blockTimerUp(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            sm.previousControlState = sm.control["block"];
        }

        private void damageTimerUp(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            //sm.currentControlState = sm.control["tree-idle"];
            sm.previousControlState = sm.control["idle"];
        }

        private void attackTimerUp(object source, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            sm.previousControlState = sm.control["idle"];
        }

        private void SetSmallFrame()
        {
            size = new Vector2(128f, 129.7f);
            offset = new Vector2(-32f, -1.5f);
        }

        private void SetHugeFrame()
        {
            size = new Vector2(436f, 220f);
            offset = new Vector2(-186f, -1.5f);
        }
        private void SetAerialAttackFrame()
        {
            size = new Vector2(436f, 220f);
            offset = new Vector2(-186f, -45f);
        }
    }
}
