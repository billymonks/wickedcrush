using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using WickedCrush.Utility;

namespace WickedCrush.GameEntities
{
    public class Fireball : Character
    {
        private int dmgAmount;
        private float force;
        private Direction atkDir;
        private Character creator;

        public CharacterFactory _cf;

        private float speed = 6f;

        public Fireball(ContentManager cm, GraphicsDevice gd, Vector2 pos, int dmgAmount, float force,
            Direction atkDir, Character creator, CharacterFactory cf)
            : base(pos, new Vector2(64f, 16f), new Vector2(0.2f, 0.5f), new Vector2(-25.6f, -4f), 128.5f, gd)
        {
            this.dmgAmount = dmgAmount;
            this.force = force;
            this.atkDir = atkDir;
            this.creator = creator;

            this._cf = cf;

            this.bright = true;

            facingDir = atkDir;
            CreateCharacter(cm);
        }

        public void CreateCharacter(ContentManager cm)
        {
            hp = 0;
            name = "Fireball";
            walkThrough = true;
            invuln = true;
            //ignorePlatforms = true;
            //bright = true;

            CreateAnimationList(cm);
            CreateStateMachine();

            /*_cf.AddLightToList(new PointLight(this,
                175f,
                new Vector4(0.7f, 0.3f, 0.3f, 1f),
                new Vector4(0.6f, 0.3f, 0.3f, 1f),
                300f,
                0.5f,
                new Vector2(20f, 20f)));*/

            if (_cf != null)
                _cf.AddLightToList(new FlickeringPointLight(this,
                    275f,
                    new Vector4(1f, 0.27f, 0f, 1f),
                    new Vector4(1f, 0.55f, 0f, 1f),
                    550f,
                    0.5f,
                    new Vector2(20f, 20f),
                    0.10f));
        }

        public void CreateAnimationList(ContentManager cm)
        {
            animationList = new Dictionary<String, Animation>();
            animationList.Add("fireball", new Animation("fireball", cm));
        }

        public void CreateStateMachine()
        {
            Dictionary<String, State> stateList = new Dictionary<String, State>();
            stateList.Add("load", new State("load", //the only state that does anything
                c => sm.previousControlState == null,
                c =>
                {
                    currentAnimation = animationList["fireball"];
                    //find collisions and make them pay if they aren't creator or invuln
                    


                    //readyForRemoval = true;
                }));
            stateList.Add("fly", new State("fly",
                c => sm.previousControlState != null,
                c =>
                {
                    if (facingDir.Equals(Direction.Left))
                    {
                        velocity.X = -speed;
                    }
                    else
                    {
                        velocity.X = speed;
                    }

                    foreach (Entity ch in collisionList)
                    {
                        if (ch.type.Equals(EntType.Character))
                        {
                            if (!ch.Equals(creator))
                            {
                                readyForRemoval = true;
                            }
                            if (!ch.invuln && !ch.Equals(creator))
                            {
                                ((Character)ch).TakeDamage(dmgAmount, force, atkDir);
                            }
                        }
                        else
                        {
                            readyForRemoval = true;
                        }
                    }
                    //readyForRemoval = true;
                }));

            sm = new StateMachine(stateList);
        }
    }
}
